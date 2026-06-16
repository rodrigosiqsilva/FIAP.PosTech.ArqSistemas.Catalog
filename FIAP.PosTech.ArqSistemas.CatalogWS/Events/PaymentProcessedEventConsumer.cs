using Confluent.Kafka;
using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Enums;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using System.Text.Json;

namespace FIAP.PosTech.ArqSistemas.CatalogWS.Events
{

    public record PaymentProcessedCreatedEvent(OrderDto Order, DateTime CreatedAt, string? CorrelationId);

    public class PaymentProcessedEventConsumer : IDisposable
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly string _topicName;
        private readonly IConfiguration _configuration;
        private readonly IOrderGameService _orderGameService;

        // Adicionamos IConfiguration no construtor
        public PaymentProcessedEventConsumer(string bootstrapServers, string topicName, string groupId, IConfiguration configuration, IOrderGameService orderGameService)
        {
            _topicName = topicName;
            _configuration = configuration;
            _orderGameService = orderGameService;

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        public Task StartConsumingAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topicName);

            // Marcamos a Action como async para podermos usar o "await" lá embaixo no EmailService
            return Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = _consumer.Consume(cancellationToken);

                            if (consumeResult != null)
                            {
                                var options = new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                };

                                var paymentProcessedEvent = JsonSerializer.Deserialize<PaymentProcessedCreatedEvent>(consumeResult.Message.Value, options);

                                if (paymentProcessedEvent != null && paymentProcessedEvent.Order != null)
                                {
                                    string nome = paymentProcessedEvent.Order.Usuario;
                                    string game = paymentProcessedEvent.Order.Game;
                                    decimal preco = paymentProcessedEvent.Order.Preco;
                                    string emailUsuario = paymentProcessedEvent.Order.EmailUser;
                                    int id = paymentProcessedEvent.Order.Id;

                                    Console.WriteLine($"[Kafka] Novo pedido recebido! Usuário: {nome} | Jogo: {game} | Preço: {preco}");

                                    var order = _orderGameService.ObterPorId(id);

                                    if (order == null)
                                    {
                                        Console.WriteLine($"Não foi encontrado registro do pedido para registro de aprovação e disponibilizá-lo na biblioteca do usuário! Usuário: {nome} | Jogo: {game} | Preço: {preco}");
                                    }
                                    else
                                    {
                                        order.Status = OrderStatus.Approved;
                                    }
                                }
                            }
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"[Kafka Consumer] Erro ao consumir mensagem: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[Kafka Consumer] Encerramento solicitado.");
                }
                finally
                {
                    _consumer.Close();
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            _consumer?.Dispose();
        }
    }
}
