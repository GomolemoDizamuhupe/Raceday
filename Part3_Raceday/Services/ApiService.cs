using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Part3_Raceday.Services
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = "";
    }

    public class ApiError
    {
        public string? Message { get; set; }
    }

    public class ApiService
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResult<T>> GetAsync<T>(string url)
        {
            try { return await ReadAsync<T>(await _http.GetAsync(url)); }
            catch (HttpRequestException) { return Unreachable<T>(); }
        }

        public async Task<ApiResult<T>> PostAsync<T>(string url, object body)
        {
            try { return await ReadAsync<T>(await _http.PostAsJsonAsync(url, body, _json)); }
            catch (HttpRequestException) { return Unreachable<T>(); }
        }

        private static ApiResult<T> Unreachable<T>() =>
            new() { Success = false, Message = "Cannot reach the server. Please try again." };

        private static async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                T? data = default;

                if (response.StatusCode != HttpStatusCode.NoContent)
                    data = await response.Content.ReadFromJsonAsync<T>(_json);

                return new ApiResult<T> { Success = true, Data = data };
            }

            string message = "Something went wrong. Please try again.";

            try
            {
                var err = await response.Content.ReadFromJsonAsync<ApiError>(_json);
                if (!string.IsNullOrWhiteSpace(err?.Message)) message = err.Message;
            }
            catch { }

            return new ApiResult<T> { Success = false, Message = message };
        }
    }
}