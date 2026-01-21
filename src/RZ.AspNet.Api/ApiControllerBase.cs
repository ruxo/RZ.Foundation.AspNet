using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RZ.Foundation;
using RZ.Foundation.Types;

namespace RZ.AspNet.Api;

[PublicAPI]
public static class RzApi
{
    public static IActionResult Respond<T>(this ApiControllerBase controller, Func<T> handler) {
        try{
            return controller.ReturnOk(handler());
        }
        catch (Exception e){
            return controller.ReturnStatusCode(e);
        }
    }

    public static IActionResult Execute<T>(this ApiControllerBase controller, Action handler) {
        try{
            handler();
            return controller.ReturnOk();
        }
        catch (Exception e){
            return controller.ReturnStatusCode(e);
        }
    }

    public static async Task<IActionResult> Respond<T>(this ApiControllerBase controller, Func<Task<T>> handler) {
        try{
            return controller.ReturnOk(await handler());
        }
        catch (Exception e){
            return controller.ReturnStatusCode(e);
        }
    }

    public static async Task<IActionResult> Execute<T>(this ApiControllerBase controller, Func<Task> handler) {
        try{
            await handler();
            return controller.ReturnOk();
        }
        catch (Exception e){
            return controller.ReturnStatusCode(e);
        }
    }

    public static IActionResult ReturnStatusCode(this ApiControllerBase controller, Exception e)
        => controller.RaiseHttpError(controller.InterpretError(e));

    public static HttpStatusCode? ErrorCodeToHttpStatus(string code) =>
        code switch {
            StandardErrorCodes.AuthenticationNeeded     => HttpStatusCode.Unauthorized,
            StandardErrorCodes.Cancelled                => HttpStatusCode.InternalServerError,
            StandardErrorCodes.DatabaseTransactionError => HttpStatusCode.InternalServerError,
            StandardErrorCodes.Duplication              => HttpStatusCode.Conflict,
            StandardErrorCodes.HttpError                => HttpStatusCode.InternalServerError,
            StandardErrorCodes.InvalidRequest           => HttpStatusCode.BadRequest,
            StandardErrorCodes.InvalidResponse          => HttpStatusCode.BadGateway,
            StandardErrorCodes.InvalidOrder             => HttpStatusCode.BadRequest,
            StandardErrorCodes.MissingConfiguration     => HttpStatusCode.InternalServerError,
            StandardErrorCodes.NetworkError             => HttpStatusCode.ServiceUnavailable,
            StandardErrorCodes.NotFound                 => HttpStatusCode.NotFound,
            StandardErrorCodes.PermissionNeeded         => HttpStatusCode.Forbidden,
            StandardErrorCodes.RaceCondition            => HttpStatusCode.Conflict,
            StandardErrorCodes.ServiceError             => HttpStatusCode.InternalServerError,
            StandardErrorCodes.Timeout                  => HttpStatusCode.GatewayTimeout,
            StandardErrorCodes.Unhandled                => HttpStatusCode.InternalServerError,
            StandardErrorCodes.ValidationFailed         => HttpStatusCode.BadRequest,
            _                                           => null
        };
}

[PublicAPI]
public class ApiControllerBase(ILogger? logger = null) : ControllerBase
{
    public virtual ErrorInfo InterpretError(Exception e) =>
        e is ErrorInfoException einfo ? einfo.ToErrorInfo() : ErrorFrom.Exception(e);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IActionResult ReturnOk() => Ok();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IActionResult ReturnOk<T>(T data) => Ok(data);

    public virtual HttpStatusCode? ConvertErrorCodeToHttpStatus(string code)
        => RzApi.ErrorCodeToHttpStatus(code);

    public IActionResult RaiseHttpError(ErrorInfo error) {
        logger?.LogDebug("API Response Error: {@Error}", error);
        return StatusCode((int) (ConvertErrorCodeToHttpStatus(error.Code) ?? HttpStatusCode.InternalServerError), error with{
            DebugInfo = null,
            Stack = null
        });
    }
}