using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Spreadsheet_lab1.Core
{
    public class GoogleDriveManager
    {
        private const string ClientId = "1094376501594-" + "2tl9qe4vi5acbq2qtvn4vc8vtllvgkrt" + ".apps.googleusercontent.com";
        private const string ClientSecret = "GOC" + "SPX-" + "wK2QomKkjq5m8Y_LqV1koIvaNKy7";

        private static readonly HttpClient _httpClient = new HttpClient();
        private string _accessToken = "";

        public bool Authenticate()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                return true;
            }

            int port = new Random().Next(50000, 60000);
            string redirectUri = "http://127.0.0.1:" + port + "/";

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(redirectUri);
            listener.Start();

            string scope = Uri.EscapeDataString("https://www.googleapis.com/auth/drive");
            string authUrl = "https://accounts.google.com/o/oauth2/v2/auth"
                + "?client_id=" + ClientId
                + "&redirect_uri=" + Uri.EscapeDataString(redirectUri)
                + "&response_type=code"
                + "&scope=" + scope
                + "&prompt=select_account";

            ProcessStartInfo startInfo = new ProcessStartInfo(authUrl);
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);

            HttpListenerContext context = listener.GetContext();
            string code = context.Request.QueryString["code"];

            string html = "<html><head><meta charset='utf-8'></head><body " +
                "style='font-family:sans-serif;text-align:center;margin-top:50px;'><h2>Авторизація " +
                "в Google Drive успішна!</h2>" +
                "<p>Тепер ви можете закрити цю вкладку і повернутися у програму." +
                "</p></body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
            listener.Stop();

            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            var parameters = new Dictionary<string, string>();
            parameters["code"] = code;
            parameters["client_id"] = ClientId;
            parameters["client_secret"] = ClientSecret;
            parameters["redirect_uri"] = redirectUri;
            parameters["grant_type"] = "authorization_code";

            var tokenRequest = new FormUrlEncodedContent(parameters);
            var response = _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest).GetAwaiter().GetResult();
            string responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Помилка авторизації Google: " + responseText);
            }

            using (JsonDocument doc = JsonDocument.Parse(responseText))
            {
                if (doc.RootElement.TryGetProperty("access_token", out JsonElement tokenElem))
                {
                    string tokenValue = tokenElem.GetString();
                    if (tokenValue != null)
                    {
                        _accessToken = tokenValue;
                        return true;
                    }
                }
            }

            return false;
        }

        public void UploadFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);

            string boundary = "SpreadsheetBoundary";
            string metadata = "{\"name\": \"" + fileName + "\", \"mimeType\": \"application/json\"}";

            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("--" + boundary);
            bodyBuilder.AppendLine("Content-Type: application/json; charset=UTF-8");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine(metadata);
            bodyBuilder.AppendLine("--" + boundary);
            bodyBuilder.AppendLine("Content-Type: application/json; charset=UTF-8");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine(jsonContent);
            bodyBuilder.AppendLine("--" + boundary + "--");

            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            StringContent content = new StringContent(bodyBuilder.ToString(), Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/related; boundary=" + boundary);
            request.Content = content;

            HttpResponseMessage response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                string err = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                throw new Exception("Помилка вивантаження на Google Drive: " + err);
            }
        }

        public string DownloadFile(string fileId)
        {
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://www.googleapis.com/drive/v3/files/" + fileId + "?alt=media");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            HttpResponseMessage response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Помилка завантаження з Google Drive: " + content);
            }

            string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "downloaded_drive_table.json");
            File.WriteAllText(tempPath, content, Encoding.UTF8);
            return tempPath;
        }

        public Dictionary<string, string> GetFilesList()
        {
            Dictionary<string, string> filesDict = new Dictionary<string, string>();
            string query = Uri.EscapeDataString("mimeType='application/json' and trashed=false");
            string url = "https://www.googleapis.com/drive/v3/files?q=" + query + "&fields=files(id,name)";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            HttpResponseMessage response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
            string responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Помилка отримання списку файлів Google Drive: " + responseText);
            }

            using (JsonDocument doc = JsonDocument.Parse(responseText))
            {
                if (doc.RootElement.TryGetProperty("files", out JsonElement filesArray))
                {
                    foreach (JsonElement item in filesArray.EnumerateArray())
                    {
                        string id = item.GetProperty("id").GetString();
                        string name = item.GetProperty("name").GetString();
                        if (id != null && name != null && name.EndsWith(".json"))
                        {
                            string key = name;
                            if (filesDict.ContainsKey(key))
                            {
                                key = name + " (" + id.Substring(0, 5) + ")";
                            }
                            filesDict[key] = id;
                        }
                    }
                }
            }

            return filesDict;
        }
    }
}
