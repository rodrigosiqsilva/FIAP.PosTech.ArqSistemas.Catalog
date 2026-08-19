using FIAP.PosTech.ArqSistemas.CatalogAPI.Middlewares;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using FIAP.PosTech.ArqSistemas.UserAPI.Services;
using Microsoft.OpenApi;
using Prometheus;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging estruturado
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();

// Configurar MongoDB Client e Database
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["MongoDbSettings:ConnectionString"] 
        ?? "mongodb://admin:adminpassword@localhost:27017";
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDbSettings:DatabaseName"] ?? "CatalogDb";
    return client.GetDatabase(databaseName);
});

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IOrderGameService, OrderGameService>();
builder.Services.AddScoped<IOrderPlacedService, OrderPlacedService>();
builder.Services.AddScoped<IBibliotecaUsuarioService, BibliotecaUsuarioService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient();

// Configurar OpenTelemetry para Tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("catalog-api"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317");
            });
    });

// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FIAP Cloud Games (FCG)",
        Version = "v1",
        Description = "A FIAP Cloud Games (FCG) é uma plataforma de venda de jogos digitais e gestão de servidores para partidas on-line",
        Contact = new OpenApiContact
        {
            Name = "Rodrigo Siqueira Silva",
            Email = "rodrigosiqueirasilva@hotmail.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Habilitar métricas do Prometheus
app.UseHttpMetrics();

app.UseRouting(); // Importante para o Swagger e Controllers se acharem

app.UseCorrelationId(); // Seu middleware personalizado

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./v1/swagger.json", "FIAP Cloud Games (FCG) v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();

app.MapControllers();
app.MapMetrics(); // Expõe métricas em /metrics

logger.LogInformation("Iniciando aplicação FIAP Cloud Games (FCG)");

try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Aplicação encerrada com erro");
}