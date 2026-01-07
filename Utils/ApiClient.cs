using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartTuningSystem.Utils
{
    public class ApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        // 新增日志记录委托
        public Action<string> LogRequestResponse { get; set; }

        /// <summary>
        /// 初始化 API 客户端
        /// </summary>
        /// <param name="baseUrl">API 基础地址</param>
        /// <param name="timeoutSeconds">超时时间（秒）</param>
        public ApiClient(string baseUrl, int timeoutSeconds = 30)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// 执行 GET 请求
        /// </summary>
        public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string> queryParams = null)
        {
            var url = BuildUrl(endpoint, queryParams);
            var response = await _httpClient.GetAsync(url);
            return await ProcessResponse<T>(response);
        }

        /// <summary>
        /// 执行 POST 请求
        /// </summary>
        public async Task<T> PostAsync<T>(string endpoint, object data, Dictionary<string, string> queryParams = null)
        {
            var url = BuildUrl(endpoint, queryParams);
            // 配置 JSON 序列化选项：禁用中文转义
            var jsonOptions = new JsonSerializerOptions
            {
                // 关键：禁用非ASCII字符的转义（让中文正常显示）
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                // 可选：美化JSON格式（调试用，生产可去掉）
                //WriteIndented = true,
            };

            var content = new StringContent(JsonSerializer.Serialize(data, jsonOptions), Encoding.UTF8, "application/json");
            // 记录请求日志
            LogRequest($"POST {url}", content);
            var response = await _httpClient.PostAsync(url, content);
            return await ProcessResponse<T>(response);
        }

        /// <summary>
        /// 添加请求头
        /// </summary>
        public void AddHeader(string name, string value)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(name))
            {
                _httpClient.DefaultRequestHeaders.Remove(name);
            }
            _httpClient.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// 设置 Bearer Token 认证
        /// </summary>
        public void SetBearerToken(string token)
        {
            AddHeader("Authorization", $"Bearer {token}");
        }

        private string BuildUrl(string endpoint, Dictionary<string, string> parameters)
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";

            if (parameters != null && parameters.Count > 0)
            {
                var query = new StringBuilder();
                foreach (var param in parameters)
                {
                    query.Append($"&{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}");
                }
                url += $"?{query.ToString().TrimStart('&')}";
            }

            return url;
        }

        private async Task<T> ProcessResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            // 记录响应日志
            LogRequestResponse?.Invoke(
                $"Response: [{(int)response.StatusCode}] {response.ReasonPhrase}\n" +
                $"Content: {content}");

            if (!response.IsSuccessStatusCode)
            {
                LogHelps.Error($"API 请求失败: [{(int)response.StatusCode}] {response.ReasonPhrase} - {content}");
                throw new HttpRequestException(
                     $"API 请求失败: [{(int)response.StatusCode}] {response.ReasonPhrase} - {content}");
            }

            try
            {
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                LogHelps.Error($"JSON 解析失败:{ex.Message}");
                throw new InvalidOperationException("JSON 解析失败", ex);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        // 新增日志记录方法
        private async void LogRequest(string requestInfo, HttpContent content)
        {
            if (LogRequestResponse == null) return;

            var log = new StringBuilder()
                .AppendLine($"Request: {requestInfo}")
                .AppendLine($"Headers: {_httpClient.DefaultRequestHeaders}")
                .AppendLine($"Content-Type: {content?.Headers?.ContentType}");

            if (content != null)
            {
                var contentStr = await content.ReadAsStringAsync();
                log.AppendLine($"Content: {contentStr}");
            }

            LogRequestResponse?.Invoke(log.ToString());
        }
    }
}
