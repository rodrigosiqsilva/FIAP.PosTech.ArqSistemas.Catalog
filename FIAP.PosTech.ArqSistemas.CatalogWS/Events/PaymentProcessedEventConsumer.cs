using Confluent.Kafka;
using FIAP.PosTech.ArqSistemas.CatalogWS.Models;
using FIAP.PosTech.ArqSistemas.CatalogWS.Services;
using System.Text.Json;

namespace FIAP.PosTech.ArqSistemas.NotificationWS.Events
{

    public record PaymentProcessedCreatedEvent(Order Order, DateTime CreatedAt, string? CorrelationId);

    public class PaymentProcessedEventConsumer : IDisposable
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly string _topicName;
        private readonly IConfiguration _configuration;
        private readonly IOrderGameService _orderGameService;
        private readonly IBibliotecaUsuarioService _bibliotecaUsuarioService;

        // Adicionamos IConfiguration no construtor
        public PaymentProcessedEventConsumer(string bootstrapServers, string topicName, string groupId, IConfiguration configuration, 
            IOrderGameService orderGameService, IBibliotecaUsuarioService bibliotecaUsuarioService)
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

        public async Task StartConsumingAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topicName);

            await Task.Yield();

            await Task.Run(async () =>
            {
                Console.WriteLine($"[Kafka] Iniciado loop de consumo para o tópico: {_topicName}");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);

                        if (consumeResult != null)
                        {
                            var message = consumeResult.Message.Value;

                            Console.WriteLine($"[Kafka] Mensagem recebida no tópico {_topicName}: {message}");


                            var paymentProcessedEvent = JsonSerializer.Deserialize<PaymentProcessedCreatedEvent>(message);

                            string nome = paymentProcessedEvent.Order.Usuario;
                            string game = paymentProcessedEvent.Order.Game;
                            decimal preco = paymentProcessedEvent.Order.Preco;
                            int id = paymentProcessedEvent.Order.Id;

                            Console.WriteLine($"[Kafka] Novo pedido recebido! Usuário: {nome} | Jogo: {game}");

                            // Uso dos serviços locais extraídos do escopo
                            var order = await _orderGameService.GetOrderAsync(id);

                            if (order == null)
                            {
                                Console.WriteLine($"Não foi encontrado registro do pedido...");
                            }

                            var adcionarBiblioteca = await _bibliotecaUsuarioService.Adicionar(new BibliotecaUsuario
                            {
                                IdGame = paymentProcessedEvent.Order.IdGame,
                                IdUser = paymentProcessedEvent.Order.IdUser
                            });

                            if (adcionarBiblioteca == null)
                            {
                                Console.WriteLine($"Não foi possível adicionar o jogo na biblioteca...");
                            }

                        }
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"[Kafka Erro] Erro ao consumir mensagem: {e.Error.Reason}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Erro Geral] Erro processando evento de pagamento: {ex.Message}");
                    }
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            _consumer?.Dispose();
        }
    }
}
