using FIAP.PosTech.ArqSistemas.CatalogWS.Models;
using System.Net;
using System.Net.Http.Json;

namespace FIAP.PosTech.ArqSistemas.CatalogWS.Services
{
    public class BibliotecaUsuarioService : IBibliotecaUsuarioService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BibliotecaUsuarioService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ApiResponse<BibliotecaUsuario>> Adicionar(BibliotecaUsuario bibliotecaUsuario)
        {
            // Recupera e valida as configurações de URL
            var baseUrl = _configuration["ApiSettings:BaseUrl"]?.TrimEnd('/');
            var bibliotecaEndpoint = _configuration["ApiSettings:BibliotecaEndpoint"]?.TrimStart('/');
            var bibliotecaUrl = $"{baseUrl}/{bibliotecaEndpoint}";

            // Envia o POST diretamente como JSON (Mais limpo e performático)
            var bibliotecaResponse = await _httpClient.PostAsJsonAsync(bibliotecaUrl, bibliotecaUsuario);

            // Verifica se o recurso ou o endpoint não foi encontrado
            if (bibliotecaResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // Nota: Considere retornar um ApiResponse com Success = false no futuro
            }

            bibliotecaResponse.EnsureSuccessStatusCode();

            return await bibliotecaResponse.Content.ReadFromJsonAsync<ApiResponse<BibliotecaUsuario>>();
        }

    }
}
