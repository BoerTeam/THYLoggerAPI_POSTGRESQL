using Newtonsoft.Json;
using RestSharp;

namespace Dashboard.Models
{
    public class SicaklikMethod
    {
        public static List<THYLoggerAPI_POSTGRESQL.Model.Sicaklik> GetAllSicaklikMethod()
        {
            try
            {
                // Link birleştirme hatasını önlemek için Trim kullanıyoruz
                var client = new RestClient("https://localhost:44347/api/Sicaklik");
                var request = new RestRequest();
                request.Method = Method.Get;

                RestResponse response = client.Execute(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var dataModel = JsonConvert.DeserializeObject<List<THYLoggerAPI_POSTGRESQL.Model.Sicaklik>>(response.Content);

                    if (dataModel != null)
                    {
                        foreach (var item in dataModel)
                        {
                            if (item.Time.HasValue)
                                item.Time = item.Time.Value.ToLocalTime();
                        }
                    }
                    return dataModel;
                }

                return new List<THYLoggerAPI_POSTGRESQL.Model.Sicaklik>();
            }
            catch (Exception)
            {
                return new List<THYLoggerAPI_POSTGRESQL.Model.Sicaklik>();
            }
        } 
    } 
} 