
using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using FIAP.PosTech.ArqSistemas.CatalogWS.Events;

namespace FIAP.PosTech.ArqSistemas.CatalogWS.Workers
{
    public class KafkaConsumerWorker : BackgroundService
    {
        private readonly PaymentProcessedEventConsumer _consumerPaymentProcessed;

        public KafkaConsumerWorker(IConfiguration configuration, IOrderGameService orderGameService)
        {
            var bootstrapServers = configuration["KafkaConfig:BootstrapServers"];
            var topicNameUserCreated = configuration["KafkaConfig:TopicNameUserCreated"];
            var topicNamePaymentProcessed = configuration["KafkaConfig:TopicNamePaymentProcessed"];
            var groupId = configuration["KafkaConfig:GroupId"];

            _consumerPaymentProcessed = new PaymentProcessedEventConsumer(bootstrapServers, topicNamePaymentProcessed, groupId, configuration, orderGameService);
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