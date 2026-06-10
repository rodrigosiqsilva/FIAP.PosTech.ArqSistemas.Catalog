using FIAP.PosTech.ArqSistemas.CatalogWS;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
