using Dashboard.DTO;
using Newtonsoft.Json;
using RestSharp;

namespace Dashboard.Models
{
    public class DoluBosMethod
    {
        public static List<BosDolu> GetAllDoluBosMethod()
        {
            try
            {
                string baseUrl = AppConfig.BaseUrl;
                var client = new RestClient($"{baseUrl}/api/DoluBos/Get");
                var request = new RestRequest();
                request.Method = Method.Get;

                RestResponse response = client.Execute(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var dataModel = JsonConvert.DeserializeObject<List<BosDolu>>(response.Content);

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
                return new List<BosDolu>();
            }
            catch (Exception)
            {
                return new List<BosDolu>();
            }
        }
    }
}
