using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public class GameSearchService : IGameSearchService
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly ILogger<GameSearchService> _logger;
        private readonly string _indexName;

        public GameSearchService(ElasticsearchClient elasticClient, IConfiguration configuration, ILogger<GameSearchService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _indexName = configuration["ElasticsearchSettings:DefaultIndex"] ?? "games";
            _ = Task.Run(GarantirIndiceCriadoAsync);
        }

        private async Task GarantirIndiceCriadoAsync()
        {
            try
            {
                var existsResponse = await _elasticClient.Indices.ExistsAsync(_indexName);
                if (!existsResponse.Exists)
                {
                    var createResponse = await _elasticClient.Indices.CreateAsync(_indexName, c => c
                        .Mappings(m => m
                            .Properties<Game>(p => p
                                .IntegerNumber(n => n.Id)
                                .Text(t => t.Nome)
                                .FloatNumber(f => f.Preco)
                                .Boolean(b => b.Ativo)
                            )
                        )
                    );

                    if (createResponse.IsValidResponse)
                    {
                        _logger.LogInformation("Índice '{IndexName}' criado com sucesso no Elasticsearch.", _indexName);
                    }
                    else
                    {
                        _logger.LogWarning("Não foi possível criar o índice '{IndexName}': {Error}", _indexName, createResponse.DebugInformation);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Aviso ao verificar/criar índice no Elasticsearch: {Message}", ex.Message);
            }
        }

        public async Task IndexarAsync(Game game)
        {
            try
            {
                var response = await _elasticClient.IndexAsync(game, idx => idx
                    .Index(_indexName)
                    .Id(game.Id)
                );

                if (response.IsValidResponse)
                {
                    _logger.LogInformation("Jogo Id {Id} ({Nome}) indexado com sucesso no Elasticsearch.", game.Id, game.Nome);
                }
                else
                {
                    _logger.LogWarning("Falha ao indexar jogo Id {Id} no Elasticsearch: {Error}", game.Id, response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro de comunicação ao indexar jogo Id {Id} no Elasticsearch: {Message}", game.Id, ex.Message);
            }
        }

        public async Task AtualizarAsync(Game game)
        {
            await IndexarAsync(game);
        }

        public async Task RemoverAsync(int id)
        {
            try
            {
                var response = await _elasticClient.DeleteAsync<Game>(id, d => d.Index(_indexName));
                if (response.IsValidResponse)
                {
                    _logger.LogInformation("Jogo Id {Id} removido com sucesso do Elasticsearch.", id);
                }
                else
                {
                    _logger.LogWarning("Aviso ao remover jogo Id {Id} do Elasticsearch: {Error}", id, response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro de comunicação ao remover jogo Id {Id} do Elasticsearch: {Message}", id, ex.Message);
            }
        }

        public async Task IndexarMuitosAsync(IEnumerable<Game> games)
        {
            foreach (var game in games)
            {
                await IndexarAsync(game);
            }
        }

        public async Task<List<GameSearchResponseDto>> BuscarAsync(string termo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termo))
                {
                    return new List<GameSearchResponseDto>();
                }

                var response = await _elasticClient.SearchAsync<Game>(s => s
                    .Indices(_indexName)
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.Nome)
                            .Query(termo)
                            .Fuzziness(new Fuzziness("AUTO"))
                        )
                    )
                    .Sort(sort => sort
                        .Score(sc => sc.Order(SortOrder.Desc))
                    )
                );

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning("Busca no Elasticsearch retornou erro: {Error}", response.DebugInformation);
                    return new List<GameSearchResponseDto>();
                }

                var resultados = response.Hits.Select(hit => new GameSearchResponseDto
                {
                    Id = hit.Source?.Id ?? 0,
                    Nome = hit.Source?.Nome ?? string.Empty,
                    Preco = hit.Source?.Preco ?? 0,
                    Ativo = hit.Source?.Ativo ?? false,
                    RelevanciaScore = hit.Score
                }).ToList();

                _logger.LogInformation("Busca por '{Termo}' no Elasticsearch encontrou {Count} resultados.", termo, resultados.Count);
                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao realizar busca no Elasticsearch para o termo '{Termo}': {Message}", termo, ex.Message);
                return new List<GameSearchResponseDto>();
            }
        }
    }
}
