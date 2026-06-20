using FIAP.PosTech.ArqSistemas.CatalogWS.Services;
using FIAP.PosTech.ArqSistemas.CatalogWS.Workers;

var builder = Host.CreateApplicationBuilder(args);

// 1. Registra os serviços usando HttpClient (Injeta o HttpClient automaticamente)
builder.Services.AddHttpClient<IBibliotecaUsuarioService, BibliotecaUsuarioService>();
builder.Services.AddHttpClient<IOrderGameService, OrderGameService>();

// 2. Registra o Worker do Kafka
builder.Services.AddHostedService<KafkaConsumerWorker>();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var host = builder.Build();
host.Run();