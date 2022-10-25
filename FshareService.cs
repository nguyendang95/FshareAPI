using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FshareAPI
{
    internal class LogOutStatus
    {
        internal string Code { get; set; }
        internal string Msg { get; set; }
    }
    public class FShareService
    {
        public enum CredentialType
        {
            UserInput,
            TextFile
        }
        public string UserEmail { get; set; }
        public string Password { get; set; }
        public string AppKey { get; set; }
        public string UserAgent { get; set; }
        public string Token { get; set; }
        public string SessionId { get; set; }
        public bool IsloggedIn { get; set; }
        public bool IsLoggedOut { get; set; }
        internal HttpClient ActiveService { get; set; }
        public async Task<FShareFileManager> LoginAsync(CredentialType CredentialType = CredentialType.UserInput, string CredentialFile = null)
        {
            try
            {
                string strUserEmail = "";
                string strPassword = "";
                string strAppKey = "";
                string strUserAgent = "";
                switch (CredentialType)
                {
                    case CredentialType.TextFile:
                        System.IO.StreamReader objStreamReader = new System.IO.StreamReader(CredentialFile);
                        strUserEmail = objStreamReader.ReadLine();
                        strPassword = objStreamReader.ReadLine();
                        strAppKey = objStreamReader.ReadLine();
                        strUserAgent = objStreamReader.ReadLine();
                        objStreamReader.Close();
                        UserEmail = strUserEmail;
                        Password = strPassword;
                        AppKey = strAppKey;
                        UserAgent = strUserAgent;
                        break;
                    case CredentialType.UserInput:
                        strUserEmail = UserEmail;
                        strPassword = Password;
                        strAppKey = AppKey;
                        strUserAgent = UserAgent;
                        break;
                }
                HttpClient objHttpClient = new HttpClient();
                objHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", "FileManager95-TJUMQN");
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    user_email = strUserEmail,
                    password = strPassword,
                    app_key = strAppKey
                }));
                HttpRequestMessage objRequestBody = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/user/login"),
                    Method = HttpMethod.Post,
                    Content = RequestBody
                };
                objRequestBody.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                Task<HttpResponseMessage> objHttpResponseMessageTask = objHttpClient.SendAsync(objRequestBody);
                HttpResponseMessage objResponse = await objHttpResponseMessageTask;
                if (objResponse.IsSuccessStatusCode)
                {
                    Task<string>strResponseTextTask = objResponse.Content.ReadAsStringAsync();
                    string strResponseText = await strResponseTextTask;
                    FShareService objResponseBodyJson = JsonConvert.DeserializeObject<FShareService>(strResponseText);
                    Token = objResponseBodyJson.Token;
                    SessionId = objResponseBodyJson.SessionId;
                    ActiveService = objHttpClient;
                    IsloggedIn = true;
                    IsLoggedOut = false;
                    FShareFileManager objFshareManager = new FShareFileManager
                    {
                        ActiveService = ActiveService,
                        SessionId = SessionId,
                        Token = Token,
                        UserAgent = strUserAgent
                    };
                    return objFshareManager;
                }
                else { return null; }
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task LogOutAsync()
        {
            try
            {
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.GetAsync(new Uri("https://api.fshare.vn/api/user/logout"));
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                if (objResponse.IsSuccessStatusCode)
                {
                    Task<string> strResponseTextTask = objResponse.Content.ReadAsStringAsync();
                    string strResponseText = await strResponseTextTask;
                    if (strResponseText != null)
                    {
                        IsLoggedOut = true;
                        IsloggedIn = false;
                        Token = null;
                        SessionId = null;
                        ActiveService = null;
                    }
                }
            }
            catch
            {
                throw new Exception();
            }      
        }
        public async Task<FshareAccountInfo> GetAccountInfoAsync()
        {
            try
            {
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.GetAsync(new Uri("https://api.fshare.vn/api/user/get"));
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                if (objResponse.IsSuccessStatusCode)
                {
                    Task<string> strResponseTextTask = objResponse.Content.ReadAsStringAsync();
                    string strResponseText = await strResponseTextTask;
                    if (strResponseText != null)
                    {
                        FshareAccountInfo objJson = JsonConvert.DeserializeObject<FshareAccountInfo>(strResponseText);
                        return objJson;
                    }
                }
                return null;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
