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
            var requestUrl = "https://sapfioriqas.sap.acacoop.com.ar:443/sap/opu/odata/SAP/API_PROC_ORDER_CONFIRMATION_2_SRV/ProcOrdConf2";

            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    OrderID = confirmation.OrderID,
                    ConfirmationUnit = confirmation.ConfirmationUnit,
                    ConfirmationUnitISOCode = confirmation.ConfirmationUnitISOCode,
                    ConfirmationYieldQuantity = confirmation.ConfirmationYieldQuantity,
                    to_ProcOrdConfMatlDocItm = confirmation.To_ProcOrdConfMatlDocItm
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 🔐 Seguridad ya está configurada en el constructor (_httpClient)

                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SAP Error {response.StatusCode}] {responseContent}");
                    return false;
                }

                Console.WriteLine("[SAP Response] " + responseContent);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SAP Post Error] " + ex.Message);
                return false;
            }
        }
    }
}
