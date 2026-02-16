//using System.Net;
//using System.Text.Json;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Data.SqlClient;

//namespace Armala.Auth.Interfaces.Middleware;

//public class ExceptionHandlingMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

//    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
//    {
//        _next = next;
//        _logger = logger;
//    }

//    public async Task InvokeAsync(HttpContext context)
//    {
//        try
//        {
//            await _next(context);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
//            await HandleExceptionAsync(context, ex);
//        }
//    }

//    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
//    {
//        HttpStatusCode statusCode;
//        string errorMessage;

//        // Manejar errores específicos de Entity Framework y SQL Server
//        if (exception is DbUpdateException dbUpdateEx)
//        {
//            if (dbUpdateEx.InnerException is SqlException sqlEx)
//            {
//                // Error 2627: Violación de restricción PRIMARY KEY o UNIQUE
//                // Error 2601: Violación de índice único
//                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
//                {
//                    statusCode = HttpStatusCode.Conflict;

//                    // Intentar extraer el nombre de la columna del mensaje de error
//                    var message = sqlEx.Message;
//                    if (message.Contains("Email", StringComparison.OrdinalIgnoreCase))
//                        errorMessage = "El email ya está registrado";
//                    else if (message.Contains("PhoneNumber", StringComparison.OrdinalIgnoreCase))
//                        errorMessage = "El número de teléfono ya está registrado";
//                    else if (message.Contains("Dni", StringComparison.OrdinalIgnoreCase))
//                        errorMessage = "El DNI ya está registrado";
//                    else
//                        errorMessage = "Ya existe un registro con estos datos";
//                }
//                else
//                {
//                    statusCode = HttpStatusCode.InternalServerError;
//                    errorMessage = "Error al guardar los datos en la base de datos";
//                }
//            }
//            else
//            {
//                statusCode = HttpStatusCode.InternalServerError;
//                errorMessage = "Error al guardar los cambios en la base de datos";
//            }
//        }
//        else
//        {
//            // Manejo de otros tipos de excepciones
//            (statusCode, errorMessage) = exception switch
//            {
//                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
//                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
//                InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
//                KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
//                _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno en el servidor")
//            };
//        }

//        var response = new
//        {
//            error = errorMessage,
//            statusCode = (int)statusCode,
//            timestamp = DateTime.UtcNow
//        };

//        context.Response.ContentType = "application/json";
//        context.Response.StatusCode = (int)statusCode;

//        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
//    }
//}
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Armala.Auth.Interfaces.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Logging más detallado
            await LogDetailedErrorAsync(context, ex);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task LogDetailedErrorAsync(HttpContext context, Exception exception)
    {
        var logMessage = new StringBuilder();
        logMessage.AppendLine("=== ERROR DETALLADO ===");
        logMessage.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");
        logMessage.AppendLine($"Path: {context.Request.Path}");
        logMessage.AppendLine($"Method: {context.Request.Method}");
        logMessage.AppendLine($"Query: {context.Request.QueryString}");
        logMessage.AppendLine($"User: {context.User?.Identity?.Name ?? "Anonymous"}");

        // Headers (sin datos sensibles)
        logMessage.AppendLine("Headers:");
        foreach (var header in context.Request.Headers.Where(h =>
            !h.Key.Contains("Authorization", StringComparison.OrdinalIgnoreCase) &&
            !h.Key.Contains("Cookie", StringComparison.OrdinalIgnoreCase)))
        {
            logMessage.AppendLine($"  {header.Key}: {header.Value}");
        }

        // Body (si es pequeño y no es multipart)
        if (context.Request.ContentLength.HasValue &&
            context.Request.ContentLength < 5000 &&
            context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Request.EnableBuffering();
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            logMessage.AppendLine($"Body: {body}");
        }

        logMessage.AppendLine($"\nException Type: {exception.GetType().FullName}");
        logMessage.AppendLine($"Message: {exception.Message}");
        logMessage.AppendLine($"StackTrace: {exception.StackTrace}");

        // Detalles de excepciones SQL
        if (exception is DbUpdateException dbUpdateEx && dbUpdateEx.InnerException is SqlException sqlEx)
        {
            logMessage.AppendLine("\n=== SQL EXCEPTION DETAILS ===");
            logMessage.AppendLine($"SQL Error Number: {sqlEx.Number}");
            logMessage.AppendLine($"SQL Error State: {sqlEx.State}");
            logMessage.AppendLine($"SQL Error Class: {sqlEx.Class}");
            logMessage.AppendLine($"SQL Server: {sqlEx.Server}");
            logMessage.AppendLine($"Procedure: {sqlEx.Procedure}");
            logMessage.AppendLine($"Line Number: {sqlEx.LineNumber}");

            foreach (SqlError error in sqlEx.Errors)
            {
                logMessage.AppendLine($"\nSQL Error #{error.Number}:");
                logMessage.AppendLine($"  Message: {error.Message}");
                logMessage.AppendLine($"  Procedure: {error.Procedure}");
                logMessage.AppendLine($"  Line: {error.LineNumber}");
            }

            // Entidades afectadas
            if (dbUpdateEx.Entries.Any())
            {
                logMessage.AppendLine("\n=== AFFECTED ENTITIES ===");
                foreach (var entry in dbUpdateEx.Entries)
                {
                    logMessage.AppendLine($"Entity Type: {entry.Entity.GetType().Name}");
                    logMessage.AppendLine($"State: {entry.State}");

                    // Valores de propiedades
                    foreach (var property in entry.Properties)
                    {
                        logMessage.AppendLine($"  {property.Metadata.Name}: {property.CurrentValue}");
                    }
                }
            }
        }

        // Inner exceptions
        var innerEx = exception.InnerException;
        var level = 1;
        while (innerEx != null)
        {
            logMessage.AppendLine($"\n=== INNER EXCEPTION {level} ===");
            logMessage.AppendLine($"Type: {innerEx.GetType().FullName}");
            logMessage.AppendLine($"Message: {innerEx.Message}");
            logMessage.AppendLine($"StackTrace: {innerEx.StackTrace}");
            innerEx = innerEx.InnerException;
            level++;
        }

        logMessage.AppendLine("=== END ERROR DETAILS ===");

        _logger.LogError(exception, logMessage.ToString());
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        string errorMessage;
        object? details = null;

        // Manejar errores específicos de Entity Framework y SQL Server
        if (exception is DbUpdateException dbUpdateEx)
        {
            if (dbUpdateEx.InnerException is SqlException sqlEx)
            {
                (statusCode, errorMessage, details) = HandleSqlException(sqlEx);
            }
            else
            {
                statusCode = HttpStatusCode.InternalServerError;
                errorMessage = "Error al guardar los cambios en la base de datos";

                // TEMPORALMENTE: Siempre incluir detalles para debugging
                details = new
                {
                    innerException = dbUpdateEx.InnerException?.Message,
                    fullStackTrace = dbUpdateEx.StackTrace,
                    entities = dbUpdateEx.Entries.Select(e => new
                    {
                        entityType = e.Entity.GetType().Name,
                        state = e.State.ToString(),
                        properties = e.Properties.ToDictionary(
                            p => p.Metadata.Name,
                            p => p.CurrentValue?.ToString() ?? "null"
                        )
                    })
                };
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

            // TEMPORALMENTE: Siempre incluir detalles para debugging
            // TODO: Cambiar a _env.IsDevelopment() en producción
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                details = new
                {
                    exceptionType = exception.GetType().Name,
                    message = exception.Message,
                    stackTrace = exception.StackTrace,
                    innerException = exception.InnerException?.Message,
                    source = exception.Source
                };
            }
        }

        var response = new
        {
            error = errorMessage,
            statusCode = (int)statusCode,
            timestamp = DateTime.UtcNow,
            details = details,
            // Incluir ID de correlación para tracking
            correlationId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private (HttpStatusCode statusCode, string errorMessage, object? details) HandleSqlException(SqlException sqlEx)
    {
        HttpStatusCode statusCode;
        string errorMessage;
        object? details = null;

        switch (sqlEx.Number)
        {
            // Violación de restricción PRIMARY KEY o UNIQUE
            case 2627:
            case 2601:
                statusCode = HttpStatusCode.Conflict;
                errorMessage = ExtractConstraintMessage(sqlEx.Message);

                // TEMPORALMENTE: Siempre incluir detalles para debugging
                details = new
                {
                    sqlErrorNumber = sqlEx.Number,
                    constraintViolation = true,
                    fullMessage = sqlEx.Message,
                    stackTrace = sqlEx.StackTrace
                };
                break;

            // Violación de FOREIGN KEY
            case 547:
                statusCode = HttpStatusCode.BadRequest;
                errorMessage = "No se puede realizar la operación debido a restricciones de integridad referencial";

                // TEMPORALMENTE: Siempre incluir detalles para debugging
                details = new
                {
                    sqlErrorNumber = sqlEx.Number,
                    foreignKeyViolation = true,
                    fullMessage = sqlEx.Message,
                    stackTrace = sqlEx.StackTrace
                };
                break;

            // Timeout
            case -2:
                statusCode = HttpStatusCode.RequestTimeout;
                errorMessage = "La operación tardó demasiado tiempo en completarse";
                break;

            // Deadlock
            case 1205:
                statusCode = HttpStatusCode.Conflict;
                errorMessage = "La operación fue cancelada debido a un conflicto con otra transacción. Por favor, intente nuevamente";
                break;

            // Connection errors
            case 53:
            case -1:
                statusCode = HttpStatusCode.ServiceUnavailable;
                errorMessage = "No se pudo conectar con la base de datos";
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorMessage = "Error al guardar los datos en la base de datos";

                // TEMPORALMENTE: Siempre incluir detalles para debugging
                details = new
                {
                    sqlErrorNumber = sqlEx.Number,
                    sqlState = sqlEx.State,
                    sqlClass = sqlEx.Class,
                    message = sqlEx.Message,
                    procedure = sqlEx.Procedure,
                    lineNumber = sqlEx.LineNumber,
                    stackTrace = sqlEx.StackTrace
                };
                break;
        }

        return (statusCode, errorMessage, details);
    }

    private static string ExtractConstraintMessage(string sqlMessage)
    {
        // Intentar extraer el nombre de la columna del mensaje de error
        if (sqlMessage.Contains("Email", StringComparison.OrdinalIgnoreCase))
            return "El email ya está registrado";

        if (sqlMessage.Contains("PhoneNumber", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("Phone", StringComparison.OrdinalIgnoreCase))
            return "El número de teléfono ya está registrado";

        if (sqlMessage.Contains("Dni", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("Document", StringComparison.OrdinalIgnoreCase))
            return "El DNI ya está registrado";

        if (sqlMessage.Contains("Username", StringComparison.OrdinalIgnoreCase) ||
            sqlMessage.Contains("UserName", StringComparison.OrdinalIgnoreCase))
            return "El nombre de usuario ya está registrado";

        return "Ya existe un registro con estos datos";
    }
}