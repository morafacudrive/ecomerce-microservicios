using Cart.API.Services;
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
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{Servicio}] {Message:lj} {NewLine}{Exception}")
    .WriteTo.File("logs/cart-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{Servicio}] {Message:lj}{NewLine}{Exception}",
        formatter: new Serilog.Formatting.Json.JsonFormatter())
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<CartService>();

var app = builder.Build();

app.UseSerilogRequestLogging(); // loggea inicio/fin de cada request con duración

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

try
{
    Log.Information("Iniciando Cart.API...");
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