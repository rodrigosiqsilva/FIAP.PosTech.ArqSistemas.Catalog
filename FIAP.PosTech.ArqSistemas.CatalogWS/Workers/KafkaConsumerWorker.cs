using FIAP.PosTech.ArqSistemas.CatalogWS.Services;
using FIAP.PosTech.ArqSistemas.NotificationWS.Events;

namespace FIAP.PosTech.ArqSistemas.CatalogWS.Workers
{
    public class KafkaConsumerWorker : BackgroundService
    {
        private readonly PaymentProcessedEventConsumer _consumerPaymentProcessed;

        // Injetamos o IServiceProvider em vez dos serviços Scoped diretamente
        public KafkaConsumerWorker(IConfiguration configuration, IServiceProvider serviceProvider, IOrderGameService orderGameService, 
                IBibliotecaUsuarioService bibliotecaUsuarioService)
        {
            var bootstrapServers = configuration["KafkaConfig:BootstrapServers"];
            var topicNamePaymentProcessed = configuration["KafkaConfig:TopicNamePaymentProcessed"];
            var groupId = configuration["KafkaConfig:GroupId"];

            // Passamos o serviceProvider para o Consumer
            _consumerPaymentProcessed = new PaymentProcessedEventConsumer(
                bootstrapServers,
                topicNamePaymentProcessed,
                groupId,
                configuration,  
                orderGameService,
                bibliotecaUsuarioService
              );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[Worker] Iniciando o consumo de mensagens do Kafka...");
            await _consumerPaymentProcessed.StartConsumingAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _consumerPaymentProcessed.Dispose();
            base.Dispose();
        }
    }
}