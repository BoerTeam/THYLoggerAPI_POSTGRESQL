using Newtonsoft.Json;
using RestSharp;

namespace Dashboard.Models
{
    public class GpsDatumMethod
    {
        public static List<THYLoggerAPI_POSTGRESQL.Model.Gpsdatum> GetAllGpsDatumMethod()
        {
            try
            {
                // Sabit URL (Sicaklik ile aynı mantık)
                var client = new RestClient("https://localhost:44347/api/Gps/Get");
                var request = new RestRequest();
                request.Method = Method.Get;

                RestResponse response = client.Execute(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var dataModel = JsonConvert.DeserializeObject<List<THYLoggerAPI_POSTGRESQL.Model.Gpsdatum>>(response.Content);

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
                return new List<THYLoggerAPI_POSTGRESQL.Model.Gpsdatum>();
            }
            catch (Exception)
            {
                return new List<THYLoggerAPI_POSTGRESQL.Model.Gpsdatum>();
            }
        }
        public static List<GPSHistoryModel> GetHistoryData(int id, string start, string end)
        {
            try
            {
                string baseUrl = "https://localhost:44347/api/Gps/GetHistoryData";
                var client = new RestClient(baseUrl);
                var request = new RestRequest();
                request.Method = Method.Get;

                request.AddParameter("id", id);
                if (!string.IsNullOrEmpty(start)) request.AddParameter("start", start);
                if (!string.IsNullOrEmpty(end)) request.AddParameter("end", end);

                RestResponse response = client.Execute(request);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<List<GPSHistoryModel>>(response.Content);
                }

                return new List<GPSHistoryModel>();
            }
            catch (Exception)
            {
                return new List<GPSHistoryModel>();
            }
        }

        // API'den gelen veriyi karşılamak için basit bir model
        public class GPSHistoryModel
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
            public string Time { get; set; }
        }
    }
}
