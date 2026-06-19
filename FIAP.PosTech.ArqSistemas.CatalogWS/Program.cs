using FIAP.PosTech.ArqSistemas.CatalogWS.Services;
using FIAP.PosTech.ArqSistemas.CatalogWS.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<KafkaConsumerWorker>();
builder.Services.AddScoped<IBibliotecaUsuarioService, BibliotecaUsuarioService>();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var host = builder.Build();
host.Run();
