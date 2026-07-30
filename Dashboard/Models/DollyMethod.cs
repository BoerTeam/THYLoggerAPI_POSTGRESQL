using Dashboard.DTO;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using RestSharp;

namespace Dashboard.Models
{
    public class DollyMethod
    {
        
        public static List<Dolly> GetAllDolly()
        {
            try
            {
                string baseUrl = AppConfig.BaseUrl;
                var client = new RestClient($"{baseUrl}/api/Dolly/Get");
                var request = new RestRequest();
                request.Method = Method.Get;

                RestResponse response = client.Execute(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    // Dolly tablosunda Time (zaman) alanı yoksa döngüye gerek kalmaz
                    return JsonConvert.DeserializeObject<List<Dolly>>(response.Content);
                }
                return new List<Dolly>();
            }
            catch (Exception)
            {
                return new List<Dolly>();
            }
        }
        public static Dolly GetDollyById(int id)
        {
            try
            {
                string baseUrl = AppConfig.BaseUrl;
                // API'deki GetById endpoint adresin (ID'yi URL'ye ekliyoruz)
                var client = new RestClient($"{baseUrl}/api/Dolly/GetById/{id}");
                var request = new RestRequest();
                request.Method = Method.Get;

                RestResponse response = client.Execute(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    // Tek bir nesne döneceği için List yerine direkt Model tipine deserialize ediyoruz
                    return JsonConvert.DeserializeObject<Dolly>(response.Content);
                }

                return null;
            }
            catch (Exception)
            {
                // Hata durumunda null dönerek kontrolü kolaylaştırıyoruz
                return null;
            }
        }
        public static bool UpdateDolly(Dolly dolly)
        {
            try
            {
                string baseUrl = AppConfig.BaseUrl;
                // API'deki Update endpoint adresin
                var client = new RestClient($"{baseUrl}/api/Dolly/Update");
                var request = new RestRequest();

                // Güncelleme için PUT metodu kullanılır (API'n POST bekliyorsa Method.Post yapabilirsin)
                request.Method = Method.Put;

                // Gönderilecek nesneyi JSON formatında gövdeye (Body) ekliyoruz
                request.AddJsonBody(dolly);

                RestResponse response = client.Execute(request);

                // İşlem başarılı mı kontrol ediyoruz
                if (response.IsSuccessful)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                // Hata durumunda loglama yapabilir veya false dönebilirsin
                return false;
            }
        }
        public static async Task<bool> AddDolly(Dolly dolly)
        {
            try
            {
                string baseUrl = AppConfig.BaseUrl;
                var client = new RestClient($"{baseUrl}/api/Dolly/Add");
                var request = new RestRequest();
                request.Method = Method.Post; // Ekleme için POST
                request.AddJsonBody(dolly);

                RestResponse response = await client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception)
            {
                return false;
            }
        }
       
    }
}
