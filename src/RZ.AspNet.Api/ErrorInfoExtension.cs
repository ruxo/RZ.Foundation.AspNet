using Microsoft.AspNetCore.Http;
using RZ.Foundation;
using RZ.Foundation.Types;

namespace RZ.AspNet.Api;

[PublicAPI]
public static class ErrorInfoExtension
{
    public static IResult HandleResult(this ErrorInfo e)
        => e.Code switch {
            AuthenticationNeeded               => Results.Unauthorized(),
            RaceCondition or Duplication       => Results.Conflict(e),
            InvalidRequest or ValidationFailed => Results.BadRequest(e),
            InvalidResponse                    => Results.Json(e, statusCode: StatusCodes.Status502BadGateway),
            NetworkError                       => Results.Json(e, statusCode: StatusCodes.Status503ServiceUnavailable),
            NotFound                           => Results.NotFound(e),
            PermissionNeeded                   => Results.StatusCode(StatusCodes.Status403Forbidden),
            StandardErrorCodes.Timeout         => Results.Json(e, statusCode: StatusCodes.Status504GatewayTimeout),
            HttpError                          => Results.Json(e, statusCode: StatusCodes.Status502BadGateway),

            _ => Results.InternalServerError(e)
        };
}