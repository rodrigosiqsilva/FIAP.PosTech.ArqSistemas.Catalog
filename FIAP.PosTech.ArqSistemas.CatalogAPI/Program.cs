using FIAP.PosTech.ArqSistemas.CatalogAPI.Middlewares;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Configurar logging estruturado
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();

// Register Usuario Service
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IOrderGameService, OrderGameService>();
builder.Services.AddHttpClient<IUserService, UserService>();

// Configure OpenAPI/Swagger using Swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
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
}

);



var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FIAP Cloud Games (FCG) v1");
    c.RoutePrefix = "swagger";
});

app.UseCorrelationId();

app.UseHttpsRedirection();

app.MapControllers();

// Log ao iniciar a aplicação
logger.LogInformation("Iniciando aplicação FIAP Cloud Games (FCG)");

try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Aplicação encerrada com erro");
}

