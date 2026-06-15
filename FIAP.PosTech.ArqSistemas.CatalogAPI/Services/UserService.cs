namespace FIAP.PosTech.ArqSistemas.CatalogAPI.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using FIAP.PosTech.ArqSistemas.UserAPI.Models;

    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UserService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            // 1. Resgata os valores do appsettings.json
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var loginEndpoint = _configuration["ApiSettings:LoginEndpoint"];
            var userEndpoint = _configuration["ApiSettings:UserEndpoint"];
            var email = _configuration["ApiSettings:Credentials:Email"];
            var senha = _configuration["ApiSettings:Credentials:Senha"];

            // 2. Realiza a Autenticação (POST)
            var loginUrl = $"{baseUrl}{loginEndpoint}";
            var loginPayload = new { email = email, senha = senha };

            var loginResponse = await _httpClient.PostAsJsonAsync(loginUrl, loginPayload);

            // Garante que o login ocorreu com sucesso (Status 200-299)
            loginResponse.EnsureSuccessStatusCode();

            // Lê a resposta do login para extrair o token
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var token = loginResult?.Token;

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Falha ao recuperar o token de autenticação da API.");
            }

            // 3. Busca o Usuário com o Token (GET)
            var userUrl = $"{baseUrl}{userEndpoint}{userId}";

            // Utilizamos HttpRequestMessage para injetar o header de forma segura para esta requisição específica
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, userUrl);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userResponse = await _httpClient.SendAsync(requestMessage);

            // Verifica se o usuário não foi encontrado
            if (userResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            userResponse.EnsureSuccessStatusCode();

            // Retorna o JSON do usuário como string
            // return await userResponse.Content.ReadAsStringAsync();
            return await userResponse.Content.ReadFromJsonAsync<User>();
        }
    }

    // Classe auxiliar para desserializar o retorno do Login
    public class AuthResponse
    {
        [JsonPropertyName("token")] // Altere aqui se sua API retornar algo como "accessToken"
        public string? Token { get; set; }
    }
}
