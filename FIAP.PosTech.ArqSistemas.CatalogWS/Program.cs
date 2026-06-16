using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using FIAP.PosTech.ArqSistemas.CatalogWS.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<KafkaConsumerWorker>();
builder.Services.AddSingleton<IOrderGameService, OrderGameService>();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var host = builder.Build();
host.Run();
