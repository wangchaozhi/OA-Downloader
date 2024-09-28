using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OA_Downloader
{
    public class RestClient
    {
        private static readonly HttpClient _httpClient =  new HttpClient { Timeout = TimeSpan.FromMinutes(30) }; 
        private static readonly string _baseUrl = "http://kzwl.tpddns.cn:8090/officeAutomation"; // 固定的基础 URL
        
        
        
        // 每次发送请求前，添加 Token 到请求头中
        private static void AddAuthorizationHeader()
        {
            string token = Properties.Settings.Default.Token;

            if (!string.IsNullOrEmpty(token))
            {
                // 在这里，假设你需要将 Token 添加到 Authorization 头中，可以根据具体要求调整
                if (_httpClient.DefaultRequestHeaders.Contains("token"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("token");
                }
                // _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                _httpClient.DefaultRequestHeaders.Add("token", token);
            }
        }

        /// <summary>
        /// 发送一个 POST 请求
        /// </summary>
        /// <param name="endpoint">请求的节点，例如 "/project"</param>
        /// <param name="data">请求的主体数据（对象将被序列化为 JSON）</param>
        /// <returns>响应的字符串</returns>
        public static async Task<string> PostAsync(string endpoint, object data)
        {
            try
            {
                // 添加 Authorization 头
                AddAuthorizationHeader();
                string url = $"{_baseUrl}{endpoint}";
                string jsonContent = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"POST 请求错误: {ex.Message}";
            }
        }

        /// <summary>
        /// 发送一个 GET 请求
        /// </summary>
        /// <param name="endpoint">请求的节点，例如 "/project"</param>
        /// <returns>响应的字符串</returns>
        public static async Task<string> GetAsync(string endpoint)
        {
            try
            {
                // 添加 Authorization 头
                AddAuthorizationHeader();
                string url = $"{_baseUrl}{endpoint}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"GET 请求错误: {ex.Message}";
            }
        }

        /// <summary>
        /// 发送一个 PUT 请求
        /// </summary>
        /// <param name="endpoint">请求的节点，例如 "/project"</param>
        /// <param name="data">请求的主体数据（对象将被序列化为 JSON）</param>
        /// <returns>响应的字符串</returns>
        public static async Task<string> PutAsync(string endpoint, object data)
        {
            try
            {
                // 添加 Authorization 头
                AddAuthorizationHeader();
                string url = $"{_baseUrl}{endpoint}";
                string jsonContent = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"PUT 请求错误: {ex.Message}";
            }
        }

        /// <summary>
        /// 发送一个 DELETE 请求
        /// </summary>
        /// <param name="endpoint">请求的节点，例如 "/project"</param>
        /// <returns>响应的字符串</returns>
        public static async Task<string> DeleteAsync(string endpoint)
        {
            try
            {
                // 添加 Authorization 头
                AddAuthorizationHeader();
                string url = $"{_baseUrl}{endpoint}";
                HttpResponseMessage response = await _httpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"DELETE 请求错误: {ex.Message}";
            }
        }
    }
}
