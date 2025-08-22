using Application.Interfaces;
using Domain.Dtos;
using Domain.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Infrastructure.Services
{
    public class SendConfirmSapService : ISendConfirmSapService
    {
        private readonly HttpClient _httpClient;
        private readonly IFileLogger _logger;

        public SendConfirmSapService(HttpClient httpClient, IFileLogger logger)
        {
            _httpClient = httpClient;

            var byteArray = Encoding.ASCII.GetBytes("DSALAZAR:InterfazZetace25.");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
        }

        public async Task<SapResponse> SendOrderConfirmationAsync(OrderConfirmationRequest confirmation)
        {
            var csrfUrl = "https://sapfioriqas.sap.acacoop.com.ar:443/sap/opu/odata/SAP/API_PROC_ORDER_CONFIRMATION_2_SRV/";
            var postUrl = $"{csrfUrl}ProcOrdConf2";

            try
            {
                // 1. Obtener token CSRF
                var csrfRequest = new HttpRequestMessage(HttpMethod.Get, csrfUrl);
                csrfRequest.Headers.Add("x-csrf-token", "Fetch");

                var csrfResponse = await _httpClient.SendAsync(csrfRequest);
                if (!csrfResponse.IsSuccessStatusCode)
                {
                    await _logger.LogErrorAsync($"[SAP CSRF Error {csrfResponse.StatusCode}] {await csrfResponse.Content.ReadAsStringAsync()}",
                        "Metodo: SendOrderConfirmationAsync");
                    return new()
                    {
                        Code = csrfResponse.StatusCode,
                        Response = await csrfResponse.Content.ReadAsStringAsync()
                    };
                }

                var csrfToken = csrfResponse.Headers.GetValues("x-csrf-token").FirstOrDefault();
                var cookies = csrfResponse.Headers.GetValues("Set-Cookie");

                // 2. Serializar el DTO
                var json = JsonConvert.SerializeObject(confirmation, Formatting.Indented);
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 3. Crear solicitud POST
                var postRequest = new HttpRequestMessage(HttpMethod.Post, postUrl);
                postRequest.Headers.Add("x-csrf-token", csrfToken);
                postRequest.Headers.Add("Cookie", string.Join("; ", cookies));
                postRequest.Content = content;

                var postResponse = await _httpClient.SendAsync(postRequest);
                var postContent = await postResponse.Content.ReadAsStringAsync();

                if (!postResponse.IsSuccessStatusCode)
                {
                    await _logger.LogErrorAsync($"[SAP POST Error {postResponse.StatusCode}] {postContent}",
                        "Metodo: SendOrderConfirmationAsync");
                    return new()
                    {
                        Code = postResponse.StatusCode,
                        Response = postContent
                    };
                }

                await _logger.LogErrorAsync($"[SAP POST Success] {postContent}] {postContent}",
                    "Metodo: SendOrderConfirmationAsync");
                return new()
                {
                    Code = postResponse.StatusCode,
                    Response = postContent
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"[SAP POST Exception] {ex.Message}",
                    "Metodo: SendOrderConfirmationAsync");
                return new()
                {
                    Code = HttpStatusCode.InternalServerError,
                    Response = ex.Message
                };
            }
        }

    }
}
