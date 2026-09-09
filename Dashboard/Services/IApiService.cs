using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dashboard.Services
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null);
        Task<bool> PostAsync<T>(string endpoint, T data);
        Task<TResult?> PostAsync<TResult, TData>(string endpoint, TData data);
        Task<bool> PutAsync<T>(string endpoint, T data);
    }
}