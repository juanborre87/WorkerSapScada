using Application.Interfaces;
using Domain.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Infrastructure.Services
{
    public class SapOrderService : ISapOrderService
    {
        private readonly HttpClient _httpClient;

        public SapOrderService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            var byteArray = Encoding.ASCII.GetBytes("DSALAZAR:InterfazZetace25.");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> SendOrderConfirmationAsync(OrderConfirmationRequest confirmation)
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
                    Console.WriteLine($"[SAP CSRF Error {csrfResponse.StatusCode}] {await csrfResponse.Content.ReadAsStringAsync()}");
                    return false;
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
                    Console.WriteLine($"[SAP POST Error {postResponse.StatusCode}] {postContent}");
                    return false;
                }

                Console.WriteLine("[SAP POST Success] " + postContent);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SAP POST Exception] " + ex.Message);
                return false;
            }
        }

    }
}
