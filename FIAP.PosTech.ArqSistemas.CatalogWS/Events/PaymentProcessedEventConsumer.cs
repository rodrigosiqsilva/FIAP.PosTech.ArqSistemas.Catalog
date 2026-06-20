// ... seus outros usings
using Confluent.Kafka;
using FIAP.PosTech.ArqSistemas.CatalogWS.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogWS.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace FIAP.PosTech.ArqSistemas.CatalogWS.Events
{
    public record PaymentProcessedCreatedEvent(OrderDto Order, DateTime CreatedAt, string? CorrelationId);

    public class PaymentProcessedEventConsumer : IDisposable
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly string _topicName;
        private readonly IServiceProvider _serviceProvider; 

        public PaymentProcessedEventConsumer(string bootstrapServers, string topicName, string groupId, IServiceProvider serviceProvider)
        {
            _topicName = topicName;
            _serviceProvider = serviceProvider; 

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
                                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                var paymentProcessedEvent = JsonSerializer.Deserialize<PaymentProcessedCreatedEvent>(consumeResult.Message.Value, options);

                                if (paymentProcessedEvent != null && paymentProcessedEvent.Order != null)
                                {
     
                                    using (var scope = _serviceProvider.CreateScope())
                                    {
                                        var orderGameService = scope.ServiceProvider.GetRequiredService<IOrderGameService>();
                                        var bibliotecaUsuarioService = scope.ServiceProvider.GetRequiredService<IBibliotecaUsuarioService>();

                                        string nome = paymentProcessedEvent.Order.Usuario;
                                        string game = paymentProcessedEvent.Order.Game;
                                        decimal preco = paymentProcessedEvent.Order.Preco;
                                        int id = paymentProcessedEvent.Order.Id;

                                        Console.WriteLine($"[Kafka] Novo pedido recebido! Usuário: {nome} | Jogo: {game}");

                                        // Uso dos serviços locais extraídos do escopo
                                        var order = await orderGameService.GetOrderAsync(id);

                                        if (order == null)
                                        {
                                            Console.WriteLine($"Não foi encontrado registro do pedido...");
                                        }

                                        var adcionarBiblioteca = await bibliotecaUsuarioService.Adicionar(new Models.BibliotecaUsuario
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