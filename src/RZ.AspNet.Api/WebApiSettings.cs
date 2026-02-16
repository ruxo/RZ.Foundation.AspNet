using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using IPNetwork = System.Net.IPNetwork;

namespace RZ.AspNet.Api;

[PublicAPI]
public static class WebApiSettings
{
    #region HTTP Logging

    /// <summary>
    /// Enables HTTP information logging for API error debugging.
    /// Logs detailed HTTP request and response information only when the response status code is 300 or above,
    /// helping to diagnose API errors while minimizing log verbosity for successful requests.
    ///
    /// <p>Make sure to call <c>UseHttpLogging()</c> in your startup code to enable this feature.</p>
    /// </summary>
    /// <typeparam name="T">The host application builder type.</typeparam>
    /// <param name="builder">The host application builder instance.</param>
    /// <param name="bodyLimit">Maximum response body size to log in kilobytes (KB). Default is 32 KB.</param>
    /// <returns>The host application builder for method chaining.</returns>
    /// <remarks>
    /// This method configures HTTP logging to capture:
    /// <list type="bullet">
    /// <item>Request properties, headers, and query parameters</item>
    /// <item>Response properties, headers, and body (for errors only)</item>
    /// </list>
    /// Response bodies are only logged when the status code is 300 or higher, keeping successful request logs clean.
    /// Supports logging for JSON, plain text, and problem+json media types.
    /// </remarks>
    public static T EnableHttpInformationLogging<T>(this T builder, int bodyLimit = 32 * 1024) where T : IHostApplicationBuilder {
        builder.Services.AddHttpLoggingInterceptor<OnlyErrorsHttpLoggingInterceptor>();
        builder.Services.AddHttpLogging(o => {
            o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders
                            | HttpLoggingFields.RequestQuery
                            | HttpLoggingFields.ResponsePropertiesAndHeaders
                            | HttpLoggingFields.ResponseBody;
            o.ResponseBodyLogLimit = bodyLimit;

            o.MediaTypeOptions.AddText("application/json");
            o.MediaTypeOptions.AddText("text/plain");
            o.MediaTypeOptions.AddText("application/problem+json");
        });
        return builder;
    }

    sealed class OnlyErrorsHttpLoggingInterceptor : IHttpLoggingInterceptor
    {
        public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext) => default;

        public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext) {
            var status = logContext.HttpContext.Response.StatusCode;

            if (status < 300){
                // Keep headers/properties, but drop the body
                logContext.TryDisable(HttpLoggingFields.ResponseBody);
            }

            return default;
        }
    }

    #endregion

    /// <summary>
    /// Enables forwarding of proxy headers (X-Forwarded-Proto, X-Forwarded-Host, X-Forwarded-Prefix) for API services behind a proxy or API gateway.
    /// This allows the application to correctly interpret the original client request information when deployed behind reverse proxies or load balancers.
    ///
    /// <p>Make sure to call <c>UseForwardedHeaders()</c> in your startup code to enable this feature.</p>
    /// </summary>
    /// <typeparam name="T">The host application builder type.</typeparam>
    /// <param name="builder">The host application builder instance.</param>
    /// <param name="knownIPNetworks">
    /// Optional list of known IP networks that are allowed to send forwarded headers.
    /// When <c>null</c> (default), clears the known networks list to accept headers from any network.
    /// Pass an empty list or specific networks to restrict which sources can provide forwarded headers.
    /// </param>
    /// <param name="knownProxies">
    /// Optional list of known proxy IP addresses that are allowed to send forwarded headers.
    /// When <c>null</c> (default), clears the known proxies list to accept headers from any proxy.
    /// Pass an empty list or specific addresses to restrict which proxies can provide forwarded headers.
    /// </param>
    /// <returns>The host application builder for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures the forwarded headers middleware to trust and process:
    /// <list type="bullet">
    /// <item><b>X-Forwarded-Proto</b>: The original protocol (http/https) used by the client</item>
    /// <item><b>X-Forwarded-Host</b>: The original host requested by the client</item>
    /// <item><b>X-Forwarded-Prefix</b>: The path prefix where the application is mounted</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Security Note:</b> The default behavior (when both parameters are <c>null</c>) is to accept forwarded headers
    /// from all clients under the assumption that your API service is deployed behind a trusted API gateway or reverse proxy that:
    /// <list type="bullet">
    /// <item>Strips any client-provided forwarded headers</item>
    /// <item>Sets its own trusted forwarded headers</item>
    /// <item>Provides network-level access control</item>
    /// </list>
    /// If your service is directly exposed to untrusted clients, provide specific values for <paramref name="knownIPNetworks"/>
    /// and/or <paramref name="knownProxies"/> to configure trusted sources explicitly.
    /// </para>
    /// </remarks>
    public static T EnableAllProxyForwards<T>(this T builder,
                                              IReadOnlyList<IPNetwork>? knownIPNetworks = null,
                                              IReadOnlyList<IPAddress>? knownProxies = null) where T : IHostApplicationBuilder {
        builder.Services
               .Configure<ForwardedHeadersOptions>(opts => {
                    opts.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedPrefix;

                    if (knownIPNetworks is null)
                        opts.KnownIPNetworks.Clear();
                    else
                        knownIPNetworks.Iter(ip => opts.KnownIPNetworks.Add(ip));

                    if (knownProxies is null)
                        opts.KnownProxies.Clear();
                    else
                        knownProxies.Iter(opts.KnownProxies.Add);
                });
        return builder;
    }

    #region Authentication

    /// <summary>
    /// Configures a permissive bearer token authentication scheme that accepts any bearer token without validation.
    /// This authentication handler is designed for API services deployed behind a trusted API gateway or reverse proxy
    /// that performs token validation. Invalid tokens are expected to be stripped by the gateway before reaching this service.
    ///
    /// <p>Make sure to call <c>UseAuthentication()</c> and <c>UseAuthorization()</c> in your startup code to enable this feature.</p>
    /// </summary>
    /// <typeparam name="T">The host application builder type.</typeparam>
    /// <param name="builder">The host application builder instance.</param>
    /// <returns>The host application builder for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures:
    /// <list type="bullet">
    /// <item>A fallback authorization policy requiring authenticated users</item>
    /// <item>A custom "AnyBearer" authentication scheme that accepts any properly formatted bearer token</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Security Warning:</b> This authentication handler does NOT validate tokens. It only checks that:
    /// <list type="bullet">
    /// <item>An Authorization header is present</item>
    /// <item>The header uses the "Bearer" scheme</item>
    /// <item>A non-empty token value exists</item>
    /// </list>
    /// This setup is only secure when deployed behind a trusted API gateway that validates and removes invalid tokens.
    /// Never expose this service directly to untrusted clients.
    /// </para>
    /// </remarks>
    public static T SetupAnyBearerTokenAuthentication<T>(this T builder) where T : IHostApplicationBuilder {
        builder.Services
               .AddAuthorization(opts => {
                    opts.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                })
               .AddAuthentication(AnyBearerDefaults.Scheme)
               .AddScheme<AuthenticationSchemeOptions, AnyBearerHandler>(AnyBearerDefaults.Scheme, null);
        return builder;
    }

    /// <summary>
    /// Defines constants for the permissive bearer token authentication scheme.
    /// </summary>
    static class AnyBearerDefaults
    {
        /// <summary>
        /// The authentication scheme name used for the permissive bearer token handler.
        /// </summary>
        public const string Scheme = "AnyBearer";
    }

    /// <summary>
    /// Authentication handler that accepts any bearer token without validation.
    /// This handler is intended for use behind a trusted API gateway that performs actual token validation.
    /// It creates an authenticated identity for any request with a properly formatted bearer token.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The handler checks only for the presence and basic format of a bearer token:
    /// <list type="bullet">
    /// <item>Requires an Authorization header with "Bearer" scheme</item>
    /// <item>Requires a non-empty token value after the "Bearer" prefix</item>
    /// <item>Does NOT validate token signature, expiration, claims, or issuer</item>
    /// </list>
    /// </para>
    /// <para>
    /// Returns <see cref="AuthenticateResult.NoResult"/> if no valid bearer token format is found,
    /// allowing other authentication schemes to handle the request.
    /// Returns <see cref="AuthenticateResult.Success"/> with an empty claims identity for any valid bearer token format.
    /// </para>
    /// </remarks>
    sealed class AnyBearerHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder )
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var auth))
                return Task.FromResult(AuthenticateResult.NoResult());

            var value = auth.ToString();

            const string prefix = "Bearer ";
            if (!value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.NoResult());

            var token = value[prefix.Length..].Trim();
            if (token.Length == 0)
                return Task.FromResult(AuthenticateResult.NoResult());

            var identity = new ClaimsIdentity(AnyBearerDefaults.Scheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AnyBearerDefaults.Scheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.Headers.WWWAuthenticate = "Bearer";
            return Task.CompletedTask;
        }
    }

    #endregion
}