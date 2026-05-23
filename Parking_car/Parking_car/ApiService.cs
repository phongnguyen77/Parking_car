using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Parking_car
{
    public class ApiService
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        private readonly string _baseUrl;
        private readonly Action<string> _log;

        public const string CloudUrl = "https://parking-backend-nh7k.onrender.com";
        

        public string BaseUrl => _baseUrl;

        public ApiService(Action<string> log, string baseUrl = CloudUrl)
        {
            _log = log;
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public Task PostEntryAsync(string rfid, string plate, string lane, string timeIn, string imageInPath)
        {
            string json = BuildJson(new[]
            {
                ("rfid",           rfid          ?? ""),
                ("plate",          plate         ?? ""),
                ("lane",           lane          ?? "ENTRY"),
                ("time_in",        timeIn        ?? ""),
                ("image_in_path",  ""),
            });
            return PostAsync("/api/parking/entry", json);
        }

        public Task PostExitAsync(string rfid, string plate, string lane, string timeOut, string imageOutPath)
        {
            string json = BuildJson(new[]
            {
                ("rfid",            rfid          ?? ""),
                ("plate",           plate         ?? ""),
                ("lane",            lane          ?? "EXIT"),
                ("time_out",        timeOut       ?? ""),
                ("image_out_path",  ""),
            });
            return PostAsync("/api/parking/exit", json);
        }

        private async Task PostAsync(string path, string json)
        {
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync(_baseUrl + path, content);
                string body = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                    _log?.Invoke($"[API] {path} OK: {body}");
                else
                    _log?.Invoke($"[API] {path} lỗi {(int)resp.StatusCode}: {body}");
            }
            catch (Exception ex)
            {
                _log?.Invoke($"[API] {path} exception: {ex.Message}");
            }
        }

        private static string BuildJson((string key, string val)[] fields)
        {
            var sb = new StringBuilder("{");
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append('"').Append(Esc(fields[i].key)).Append("\":\"").Append(Esc(fields[i].val)).Append('"');
            }
            sb.Append('}');
            return sb.ToString();
        }

        private static string Esc(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
}
