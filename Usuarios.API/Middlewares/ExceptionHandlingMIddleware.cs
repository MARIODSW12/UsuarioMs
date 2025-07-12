using System.Net;
using System.Text.Json;
using log4net;

namespace Usuarios.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ExceptionHandlingMiddleware));

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Continua con la ejecución de la solicitud
            }
            catch (Exception ex)
            {
                _logger.Error("Ocurrió una excepción no manejada.", ex);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = new
            {
                Message = "Ocurrió un error inesperado en el servidor.",
                Error = exception.Message,
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
