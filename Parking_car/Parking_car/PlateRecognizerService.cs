using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parking_car
{
    public class PlateRecognizerService
    {
        public const string ApiUrl = "https://api.platerecognizer.com/v1/plate-reader/";

        // Đăng ký miễn phí tại https://platerecognizer.com → lấy token dán vào đây
        public const string DefaultToken = "YOUR_TOKEN_HERE";

        private readonly HttpClient _http;
        private readonly Action<string> _log;

        public bool IsConfigured => _token != "YOUR_TOKEN_HERE" && !string.IsNullOrWhiteSpace(_token);

        private readonly string _token;

        public PlateRecognizerService(string apiToken, Action<string> log)
        {
            _token = string.IsNullOrWhiteSpace(apiToken) ? DefaultToken : apiToken;
            _log = log;
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _token);
        }

        // Trả về chuỗi biển số (VD: "51H12345") hoặc "" nếu không nhận diện được
        public async Task<string> RecognizeAsync(Bitmap bmp)
        {
            if (bmp == null || !IsConfigured)
            {
                if (!IsConfigured) _log?.Invoke("[PlateOCR] Chưa cấu hình API token.");
                return "";
            }

            byte[] imageBytes;
            try
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    imageBytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                _log?.Invoke($"[PlateOCR] Lỗi đọc ảnh: {ex.Message}");
                return "";
            }

            try
            {
                using (var form = new MultipartFormDataContent())
                {
                    var imgContent = new ByteArrayContent(imageBytes);
                    imgContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    form.Add(imgContent, "upload", "plate.jpg");
                    form.Add(new StringContent("vn"), "regions"); // gợi ý region Việt Nam

                    var resp = await _http.PostAsync(ApiUrl, form);
                    string body = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                    {
                        _log?.Invoke($"[PlateOCR] HTTP {(int)resp.StatusCode}: {body}");
                        return "";
                    }

                    string plate = ParseFirstPlate(body);
                    _log?.Invoke($"[PlateOCR] Nhận diện: {(plate.Length > 0 ? plate : "(không thấy biển)")}");
                    return plate;
                }
            }
            catch (TaskCanceledException)
            {
                _log?.Invoke("[PlateOCR] Timeout khi gọi API.");
                return "";
            }
            catch (Exception ex)
            {
                _log?.Invoke($"[PlateOCR] Lỗi: {ex.Message}");
                return "";
            }
        }

        // Lấy giá trị "plate" đầu tiên trong JSON response
        private static string ParseFirstPlate(string json)
        {
            var match = Regex.Match(json, @"""plate""\s*:\s*""([^""]+)""");
            return match.Success ? match.Groups[1].Value.ToUpper() : "";
        }
    }
}
