using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace RZ.AspNet.Helper;

[PublicAPI]
public static class FailWithExtensions
{
    [DoesNotReturn]
    public static T FailWith<T>(this ILogger logger, string message) {
        logger.LogCritical(message);
        Environment.FailFast(message);
        return default!;
    }
}