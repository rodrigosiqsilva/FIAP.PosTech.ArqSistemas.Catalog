using FIAP.PosTech.ArqSistemas.CatalogAPI.Middlewares;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using FIAP.PosTech.ArqSistemas.UserAPI.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging estruturado
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();

// CORREÇÃO: Mudado de Singleton para Scoped para evitar problemas de concorrência 
// e conflito com o HttpClient (Transient/Scoped)
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IOrderGameService, OrderGameService>();
builder.Services.AddScoped<IOrderPlacedService, OrderPlacedService>();

// Configura o HttpClient corretamente como Scoped/Transient por trás dos panos
builder.Services.AddHttpClient<IUserService, UserService>();

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

// CORREÇÃO: Ordem correta do pipeline do ASP.NET Core
app.UseHttpsRedirection();

app.UseRouting(); // Importante para o Swagger e Controllers se acharem

app.UseCorrelationId(); // Seu middleware personalizado

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FIAP Cloud Games (FCG) v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();

app.MapControllers();

logger.LogInformation("Iniciando aplicação FIAP Cloud Games (FCG)");

try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Aplicação encerrada com erro");
}