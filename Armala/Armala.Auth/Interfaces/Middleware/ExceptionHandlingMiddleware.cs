using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Armala.Auth.Interfaces.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        string errorMessage;

        // Manejar errores específicos de Entity Framework y SQL Server
        if (exception is DbUpdateException dbUpdateEx)
        {
            if (dbUpdateEx.InnerException is SqlException sqlEx)
            {
                // Error 2627: Violación de restricción PRIMARY KEY o UNIQUE
                // Error 2601: Violación de índice único
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    statusCode = HttpStatusCode.Conflict;
                    
                    // Intentar extraer el nombre de la columna del mensaje de error
                    var message = sqlEx.Message;
                    if (message.Contains("Email", StringComparison.OrdinalIgnoreCase))
                        errorMessage = "El email ya está registrado";
                    else if (message.Contains("PhoneNumber", StringComparison.OrdinalIgnoreCase))
                        errorMessage = "El número de teléfono ya está registrado";
                    else if (message.Contains("Dni", StringComparison.OrdinalIgnoreCase))
                        errorMessage = "El DNI ya está registrado";
                    else
                        errorMessage = "Ya existe un registro con estos datos";
                }
                else
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    errorMessage = "Error al guardar los datos en la base de datos";
                }
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                errorMessage = "Error al guardar los cambios en la base de datos";
            }
        }
        else
        {
            // Manejo de otros tipos de excepciones
            (statusCode, errorMessage) = exception switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno en el servidor")
            };
        }

        var response = new
        {
            error = errorMessage,
            statusCode = (int)statusCode,
            timestamp = DateTime.UtcNow
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
