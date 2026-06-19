using FIAP.PosTech.ArqSistemas.CatalogWS.DTOs;
using FIAP.PosTech.ArqSistemas.CatalogWS.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace FIAP.PosTech.ArqSistemas.CatalogWS.Services
{
    public class OrderGameService : IOrderGameService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OrderGameService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public Task<bool> AproveOrderAsync(int orderId)
        {
            // 1. Resgata os valores do appsettings.json
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var orderEndpoint = _configuration["ApiSettings:OrderEndpoint"];

            // 2. Busca o Pedido com o Token (GET)
            var orderUrl = $"{baseUrl}{orderEndpoint}{orderId}";

            // Utilizamos HttpRequestMessage para injetar o header de forma segura para esta requisição específica
            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, orderUrl);

            var orderResponse = _httpClient.SendAsync(requestMessage);

            // Verifica se o pedido não foi encontrado
            if (orderResponse == HttpStatusCode.NotFound)
            {
                return null;
            }

            orderResponse.EnsureSuccessStatusCode();

            // Retorna o JSON do pedido como string
            // return await orderResponse.Content.ReadAsStringAsync();
            return await orderResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        }

        public async Task<ApiResponse<OrderDto>> GetOrderAsync(int orderId)
        {
            // 1. Resgata os valores do appsettings.json
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var orderEndpoint = _configuration["ApiSettings:OrderEndpoint"];

            // 2. Busca o Pedido com o Token (GET)
            var orderUrl = $"{baseUrl}{orderEndpoint}{orderId}";

            // Utilizamos HttpRequestMessage para injetar o header de forma segura para esta requisição específica
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, orderUrl);

            var orderResponse = await _httpClient.SendAsync(requestMessage);

            // Verifica se o pedido não foi encontrado
            if (orderResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            orderResponse.EnsureSuccessStatusCode();

            // Retorna o JSON do pedido como string
            // return await orderResponse.Content.ReadAsStringAsync();
            return await orderResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        }

    }
}
