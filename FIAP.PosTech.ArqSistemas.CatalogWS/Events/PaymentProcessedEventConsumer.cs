using Confluent.Kafka;
using System.Text.Json;
using FIAP.PosTech.ArqSistemas.CatalogWS.Services;
using FIAP.PosTech.ArqSistemas.CatalogWS.DTOs;


namespace FIAP.PosTech.ArqSistemas.CatalogWS.Events
{

    public record PaymentProcessedCreatedEvent(OrderDto Order, DateTime CreatedAt, string? CorrelationId);

    public class PaymentProcessedEventConsumer : IDisposable
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly string _topicName;
        private readonly IConfiguration _configuration;
        private readonly IOrderGameService _orderGameService;
        private readonly IBibliotecaUsuarioService _bibliotecaUsuarioService;

        // Adicionamos IConfiguration no construtor
        public PaymentProcessedEventConsumer(string bootstrapServers, string topicName, string groupId, IConfiguration configuration, IOrderGameService orderGameService,
                IBibliotecaUsuarioService bibliotecaUsuarioService)
        {
            _topicName = topicName;
            _configuration = configuration;
            _orderGameService = orderGameService;
            _bibliotecaUsuarioService = bibliotecaUsuarioService;

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

                                    var order = await _orderGameService.GetOrderAsync(id);

                                    if (order == null)
                                    {
                                        Console.WriteLine($"Não foi encontrado registro do pedido para registro de aprovação e disponibilizá-lo na biblioteca do usuário! Usuário: {nome} | Jogo: {game} | Preço: {preco}");
                                    }

                                    var adcionarBiblioteca = await _bibliotecaUsuarioService.Adicionar(new Models.BibliotecaUsuario
                                    {
                                        IdGame = paymentProcessedEvent.Order.IdGame,
                                        IdUser = paymentProcessedEvent.Order.IdUser
                                    });

                                    if (adcionarBiblioteca == null)
                                    {
                                        Console.WriteLine($"Não foi adicionar o jogo na biblioteca do usuário! Usuário: {nome} | Jogo: {game} | Preço: {preco}");
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
