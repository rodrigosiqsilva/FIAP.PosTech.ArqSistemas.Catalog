using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Models;
using FIAP.PosTech.ArqSistemas.CatalogAPI.Services;
using FIAP.PosTech.ArqSistemas.CatalogAPI.DTOs;

namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(ICatalogService catalogService, ILogger<CatalogController> logger )
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o CorrelationId do contexto HTTP
        /// </summary>
        private string? GetCorrelationId() => HttpContext.Items["CorrelationId"]?.ToString();

        /// <summary>
        /// Obtém todos os catálogos
        /// </summary>
        /// <returns>Lista de todos os catálogos</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<List<Catalog>>> ObterTodos()
        {
            try
            {
                var catalogs = _catalogService.ObterTodos();
                var response = ApiResponse<List<Catalog>>.SucessoList(catalogs, $"Total de {catalogs.Count} catálogos encontrados");
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todos os catálogos");
                var response = ApiResponse<List<Catalog>>.Erro(ex.Message, "Erro ao obter catálogos");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Obtém um catálogo pelo Id
        /// </summary>
        /// <param name="id">Id do catálogo</param>
        /// <returns>Catálogo encontrado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]       
        public ActionResult<ApiResponse<Catalog>> ObterPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var errorResponse = ApiResponse<Catalog>.Erro("Id deve ser um número positivo", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var catalog = _catalogService.ObterPorId(id);

                if (catalog == null)
                {
                    var notFoundResponse = ApiResponse<Catalog>.NotFound($"Catálogo com Id {id} não encontrado");
                    notFoundResponse.CorrelationId = GetCorrelationId();
                    return NotFound(notFoundResponse);
                }

                var response = ApiResponse<Catalog>.SucessoOk(catalog, "Catálogo encontrado com sucesso");
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter catálogo com Id {Id}", id);
                var response = ApiResponse<Catalog>.Erro(ex.Message, "Erro ao obter catálogo");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Cria um novo catálogo
        /// </summary>
        /// <param name="catalog">Dados do catálogo a ser criado (não incluir Id)</param>
        /// <returns>Catálogo criado com Id gerado automaticamente</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ApiResponse<Catalog>> Criar([FromBody] Catalog catalog)
        {
            try
            {
                if (catalog == null)
                {
                    var errorResponse = ApiResponse<Catalog>.Erro("Corpo da requisição não pode estar vazio", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var (sucesso, mensagem, catalogCriado) = _catalogService.Criar(catalog);

                if (!sucesso)
                {
                    var errorResponse = ApiResponse<Catalog>.Erro(mensagem, "Erro ao criar catálogo");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var response = ApiResponse<Catalog>.SucessoCreate(catalogCriado, mensagem);
                response.CorrelationId = GetCorrelationId();

                return CreatedAtAction(nameof(ObterPorId), new { id = catalogCriado.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar catálogo");
                var response = ApiResponse<Catalog>.Erro(ex.Message, "Erro ao criar catálogo");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Altera um catálogo existente (partial update)
        /// </summary>
        /// <param name="id">Id do catálogo a ser alterado (obrigatório)</param>
        /// <param name="catalogAtualizado">Dados a serem atualizados. Todos os campos são opcionais - apenas os fornecidos serão alterados.</param>
        /// <returns>Catálogo alterado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<Catalog>> Alterar(int id, [FromBody] AtualizarCatalogDto catalogAtualizado)
        {
            try
            {
                if (catalogAtualizado == null)
                {
                    var errorResponse = ApiResponse<Catalog>.Erro("Corpo da requisição não pode estar vazio", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                if (id <= 0)
                {
                    var errorResponse = ApiResponse<Catalog>.Erro("Id deve ser um número positivo", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var (sucesso, mensagem, catalogAlterado) = _catalogService.Alterar(id, catalogAtualizado);

                if (!sucesso)
                {
                    if (mensagem == "Catálogo não encontrado")
                    {
                        var notFoundResponse = ApiResponse<Catalog>.NotFound(mensagem);
                        notFoundResponse.CorrelationId = GetCorrelationId();
                        return NotFound(notFoundResponse);
                    }

                    var errorResponse = ApiResponse<Catalog>.Erro(mensagem, "Erro ao alterar catálogo");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var response = ApiResponse<Catalog>.SucessoOk(catalogAlterado, mensagem);
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar catálogo com Id {Id}", id);
                var response = ApiResponse<Catalog>.Erro(ex.Message, "Erro ao alterar catálogo");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        /// <summary>
        /// Exclui um catálogo existente
        /// </summary>
        /// <param name="id">Id do catálogo a ser excluído (obrigatório)</param>
        /// <returns>Resultado da exclusão</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<object?>> Excluir(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var errorResponse = ApiResponse<object?>.Erro("Id deve ser um número positivo", "Validação falhou");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var (sucesso, mensagem) = _catalogService.Excluir(id);

                if (!sucesso)
                {
                    if (mensagem == "Catálogo não encontrado")
                    {
                        var notFoundResponse = ApiResponse<object?>.NotFound(mensagem);
                        notFoundResponse.CorrelationId = GetCorrelationId();
                        return NotFound(notFoundResponse);
                    }

                    var errorResponse = ApiResponse<object?>.Erro(mensagem, "Erro ao excluir catálogo");
                    errorResponse.CorrelationId = GetCorrelationId();
                    return BadRequest(errorResponse);
                }

                var response = ApiResponse<object?>.SucessoOk(null, mensagem);
                response.CorrelationId = GetCorrelationId();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir catálogo com Id {Id}", id);
                var response = ApiResponse<object?>.Erro(ex.Message, "Erro ao excluir catálogo");
                response.CorrelationId = GetCorrelationId();
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    }
}
