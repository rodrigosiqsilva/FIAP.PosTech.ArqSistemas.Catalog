using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    public class CatalogService : ICatalogService
    {

        private readonly List<Catalog> _catalog;
        private readonly ILogger<CatalogService> _logger;
        private int _proximoId = 6;

        public CatalogService(ILogger<CatalogService> logger)
        {
            _logger = logger;
            _catalog = new List<Catalog>();
            InicializarDados();
        }

        /// <summary>
        /// Inicializa 5 registros fictícios para testes
        /// </summary>
        private void InicializarDados()
        {
            _catalog.AddRange(new[]
            {
                new Catalog { Id = 1, Nome = "Minecraft", Preco = 100.00m, Ativo = true },
                new Catalog { Id = 2, Nome = "Grand Theft Auto V", Preco = 200.00m, Ativo = true },
                new Catalog { Id = 3, Nome = "EA SPORTS FC 26", Preco = 150.00m, Ativo = true },
                new Catalog { Id = 4, Nome = "Forza Horizon 5", Preco = 300.00m, Ativo = true },
                new Catalog { Id = 5, Nome = "Destiny 2", Preco = 250.00m, Ativo = true }
            });

            _logger.LogInformation("Dados iniciais de catálogos carregados com sucesso. Total de registros: {TotalRegistros}", _catalog.Count);
        }

        public List<Catalog> ObterTodos()
        {
            _logger.LogInformation("Obtendo todos os catálogos. Total: {Total}", _catalog.Count);
            return _catalog.ToList();
        }

        public Catalog ObterPorId(int id)
        {
            var catalog = _catalog.FirstOrDefault(c => c.Id == id);
            if (catalog == null)
            {
                _logger.LogWarning("Catálogo com Id {Id} não encontrado", id);
            }
            else
            {
                _logger.LogInformation("Catálogo com Id {Id} encontrado: {Nome}", id, catalog.Nome);
            }
            return catalog;
        }

        public (bool Sucesso, string Mensagem, Catalog Catalog) Criar(Catalog catalog)
        {
            var erros = new List<string>();

            // Validar obrigatoriedade
            if (string.IsNullOrWhiteSpace(catalog.Nome))
                erros.Add("Nome é obrigatório");

            if (catalog.Preco <= 0)
                erros.Add("Preço é obrigatório e deve ser positivo");

            if (erros.Count > 0)
            {
                var mensagem = string.Join("; ", erros);
                _logger.LogWarning("Erro ao criar catálogo: {Erros}", mensagem);
                return (false, mensagem, null);
            }

            // Criar novo catálogo com Id gerado
            var novoCatalog = new Catalog
            {
                Id = _proximoId++,
                Nome = catalog.Nome.Trim(),
                Preco = catalog.Preco,
                Ativo = catalog.Ativo   
            };

            _catalog.Add(novoCatalog);
            _logger.LogInformation("Catálogo criado com sucesso. Id: {Id}, Nome: {Nome}, Preco: {Preco}",
                novoCatalog.Id, novoCatalog.Nome, novoCatalog.Preco);

            return (true, "Catálogo criado com sucesso", novoCatalog);
        }

        public (bool Sucesso, string Mensagem, Catalog Catalog) Alterar(int id, AtualizarCatalogDto catalogAtualizado)
        {
            var erros = new List<string>();

            // Validar Id obrigatório
            if (id <= 0)
                erros.Add("Id deve ser um número positivo");

            var catalogExistente = _catalog.FirstOrDefault(c => c.Id == id);
            if (catalogExistente == null)
            {
                _logger.LogWarning("Erro ao alterar: Catálogo com Id {Id} não encontrado", id);
                return (false, "Catálogo não encontrado", null);
            }

            // Validar e atualizar Nome (se fornecido)
            if (!string.IsNullOrWhiteSpace(catalogAtualizado.Nome))
            {
                catalogExistente.Nome = catalogAtualizado.Nome.Trim();
                _logger.LogInformation("Campo Nome atualizado para o catálogo Id {Id}", id);
            }

            // Validar e atualizar Preco (se fornecido)
            if (catalogAtualizado.Preco.HasValue && catalogAtualizado.Preco.Value > 0)
            {
                catalogExistente.Preco = catalogAtualizado.Preco.Value;
                _logger.LogInformation("Campo Preco atualizado para o catálogo Id {Id}", id);
            }

            // Validar e atualizar Ativo (se fornecido)
            if (catalogAtualizado.Ativo.HasValue)
            {
                catalogExistente.Ativo = catalogAtualizado.Ativo.Value;
                _logger.LogInformation("Campo Ativo atualizado para o catálogo Id {Id}", id);
            }

            if (erros.Count > 0)
            {
                var mensagem = string.Join("; ", erros);
                _logger.LogWarning("Erro ao alterar catálogo {Id}: {Erros}", id, mensagem);
                return (false, mensagem, null);
            }

            _logger.LogInformation("Catálogo alterado com sucesso. Id: {Id}, Nome: {Nome}, Preco: {Preco}, Ativo: {Ativo}",
                catalogExistente.Id, catalogExistente.Nome, catalogExistente.Preco, catalogExistente.Ativo);

            return (true, "Catálogo alterado com sucesso", catalogExistente);
        }

        public (bool Sucesso, string Mensagem) Excluir(int id)
        {
            // Validar Id
            if (id <= 0)
                return (false, "Id deve ser um número positivo");

            var catalogExistente = _catalog.FirstOrDefault(c => c.Id == id);
            if (catalogExistente == null)
            {
                _logger.LogWarning("Erro ao excluir: Catálogo com Id {Id} não encontrado", id);
                return (false, "Catálogo não encontrado");
            }

            _catalog.Remove(catalogExistente);
            _logger.LogInformation("Catálogo excluído com sucesso. Id: {Id}, Nome: {Nome}", id, catalogExistente.Nome);

            return (true, "Catálogo excluído com sucesso");
        }
    }
}
