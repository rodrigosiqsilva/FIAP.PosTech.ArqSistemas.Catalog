using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Publisher;

namespace FIAP.PosTech.ArqSistemas.UserAPI.Services
{
    public class OrderPlacedService : IOrderPlacedService
    {
        private readonly ILogger<OrderPlacedService> _logger;
        private readonly IConfiguration _configuration;

        public OrderPlacedService(ILogger<OrderPlacedService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendNotificationUser(OrderDto order, string? correlationId)
        {
            string bootstrapServers = _configuration["KafkaConfig:BootstrapServers"];
            string topicName = _configuration["KafkaConfig:TopicNameOrderPlaced"];

            // Cria o evento
            var newEvent = new OrderPlacedEventCreated(
                Order: order,
                CreatedAt: DateTime.UtcNow,
                CorrelationId: correlationId
            );

            using (var publisher = new OrderPlacedEventPublisher(bootstrapServers, topicName))
            {
                try
                {
                    _logger.LogInformation("Publicando evento...");
                    await publisher.PublishOrderPlacedEventAsync(newEvent);
                    _logger.LogInformation($"Evento publicado com sucesso! {newEvent}");

                }
                catch (Exception ex) 
                { 
                    _logger.LogError(ex, "Erro ao publicar evento");
                }
            }
        }

    }
}
