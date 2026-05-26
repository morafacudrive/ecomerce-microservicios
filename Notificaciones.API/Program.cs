using Microsoft.AspNetCore.Mvc;
using Notifications.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Notificaciones.API")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servicio}] [{CorrelationId}] {Message:lj} {NewLine}{Exception}")
    .WriteTo.File(
        formatter: new Serilog.Formatting.Json.JsonFormatter(),
        path: "logs/products-.log",
        rollingInterval: RollingInterval.Day)
);

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errores = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
            .ToList();

        var mensaje = string.Join("; ", errores);

        return new BadRequestObjectResult(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = "La solicitud contiene datos inválidos.",
            instance = context.HttpContext.Request.Path.Value,
            errorCode = "NTF-002",
            errorMessage = mensaje
        });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<NotificationService>();

// Exception Handlers
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
// Activa el manejo global de excepciones
app.UseExceptionHandler();

app.MapControllers();

try
{
    Log.Information("Iniciando Notificaciones.API...");
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                estado = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    nombre = e.Key,
                    estado = e.Value.Status.ToString()
                })
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                estado = report.Status.ToString()
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                estado = report.Status.ToString()
            });
            await context.Response.WriteAsync(result);
        }
    });
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Notificaciones.API termin� inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}