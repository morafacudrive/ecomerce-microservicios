using Microsoft.AspNetCore.Mvc;
using Cart.API.ExceptionHandlers;
using Cart.API.Services;
using Cart.API.Middlewares; 
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Cart.API")
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
            detail = "La solicitud contiene datos invÃ¡lidos.",
            instance = context.HttpContext.Request.Path.Value,
            errorCode = "CRT-004",
            errorMessage = mensaje
        });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();


builder.Services.AddSingleton<CartService>();

builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<UnprocessableExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

var app = builder.Build();


app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

try
{
    Log.Information("Iniciando Cart.API...");

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
    Log.Fatal(ex, "Cart.API terminó inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}