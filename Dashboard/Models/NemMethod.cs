using Dashboard.DTO;
using Newtonsoft.Json;
using RestSharp;

namespace Dashboard.Models
{
    public class NemMethod
    {
        public static List<Nem> GetAllNemMethod()
        {
            try
            {
                string baseUrl = AppConfig.BaseUrl;
                var client = new RestClient($"{baseUrl}/api/Nem/Get");
                var request = new RestRequest();
                request.Method = Method.Get;

                RestResponse response = client.Execute(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var dataModel = JsonConvert.DeserializeObject<List<Nem>>(response.Content);

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
                return new List<Nem>();
            }
            catch (Exception)
            {
                return new List<Nem>();
            }
        }
    }
}