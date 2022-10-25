using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.InteropServices;

namespace FshareAPI
{
    internal class GetTotalFileInFolderResponse
    {
        internal string Code { get; set; } 
        internal string Total { get; set; }
    }
    internal class UploadDownloadResponse
    {
        internal string Code { get; set; }
        internal string Location { get; set; }
    }
    public class FShareFileManager
    {
        public string SessionId { get; set; }
        public string UserAgent { get; set; }
        public string Token { get; set; }
        internal HttpClient ActiveService { get; set; }
        public enum ChangeFavoriteStatus
        {
            FavoriteEnabled,
            FavoriteDisabled
        }
        public enum FileOrFolder
        {
            FileAndFolder,
            FolderOnly
        }
        public async Task<string>UploadFile(string FileName, string DestinationFolder = "/", bool Secured = false)
        {
            FileInfo objFile = new FileInfo(FileName);
            string strSecured;
            if (Secured == false) { strSecured = "0"; } else { strSecured = "1"; }
            StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
            {
                name = objFile.Name,
                size = objFile.Length,
                path = DestinationFolder,
                token = Token,
                secured = strSecured
            }));
            HttpClient objHttpClient = ActiveService;
            objHttpClient.DefaultRequestHeaders.Clear();
            objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
            HttpRequestMessage objRequestBody = new HttpRequestMessage
            {
                RequestUri = new Uri("https://api.fshare.vn/api/session/upload"),
                Method = HttpMethod.Post,
                Content = RequestBody
            };
            objRequestBody.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage objResponse = await objHttpClient.SendAsync(objRequestBody);
            if (objResponse.IsSuccessStatusCode)
            {
                string strResponseText = await objResponse.Content.ReadAsStringAsync();
                if (strResponseText != null)
                {
                    UploadDownloadResponse objJson = JsonConvert.DeserializeObject<UploadDownloadResponse>(strResponseText);
                    if (objJson != null && objJson.Code == "200")
                    {
                        return await UploadFileToURL(objJson.Location, FileName);
                    }
                }    
            }
            return "Upload failed";
        }
        private async Task<string>UploadFileToURL(string URL, string FileName)
        {
            try
            {
                FileStream objFileStream = new FileStream(FileName, FileMode.Open);
                byte[] arrByteArray = new byte[objFileStream.Length];
                objFileStream.Read(arrByteArray, 0, arrByteArray.Length);
                objFileStream.Close();
                ByteArrayContent objByteArrayContent = new ByteArrayContent(arrByteArray);
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Clear();
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequestBody = new HttpRequestMessage
                {
                    Content = objByteArrayContent,
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(URL)
                };
                HttpResponseMessage objResponse = await objHttpClient.SendAsync(objRequestBody);
                if (objResponse.IsSuccessStatusCode)
                {
                    return "Upload completed";
                }
                else
                {
                    return "Upload failed";
                }
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<string>DownloadFile(string URL, string DestinationFolder, string Password = "", int ZipFlag = 0)
        {
            try
            {
                string strZipFlag = "";
                switch (ZipFlag)
                {
                    case 1:
                        strZipFlag = "1";
                        break;
                    case 0:
                        strZipFlag = "0";
                        break;
                }
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    url = URL,
                    password = Password,
                    token = Token,
                    zipflag = strZipFlag
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Clear();
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequestBody = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://api.fshare.vn/api/session/download"),
                    Content = RequestBody
                };
                objRequestBody.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                HttpResponseMessage objResponse = await objHttpClient.SendAsync(objRequestBody);
                if (objResponse.IsSuccessStatusCode)
                {
                    string strResponseText = objResponse.Content.ReadAsStringAsync().Result;
                    if (strResponseText != null)
                    {
                        UploadDownloadResponse objJson = JsonConvert.DeserializeObject<UploadDownloadResponse>(strResponseText);
                        return await DownloadFileFromURL(objJson.Location, DestinationFolder);
                    }
                }
                return "Download failed";
            }
            catch
            {
                throw new Exception();
            }
        }
        private async Task<string>DownloadFileFromURL(string URL, string DestinationFolder)
        {
            try
            {
                string strFile = DestinationFolder + "\\" + GetFileNameFromURL(URL);
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Clear();
                Stream objStream = await objHttpClient.GetStreamAsync(URL);
                FileStream objFileStream = new FileStream(strFile, FileMode.CreateNew);
                objFileStream.CopyTo(objFileStream);
                objStream.Close();
                return "Download completed";
            }
            catch
            {
                return "Download failed";
            }
        }
        public async Task<List<FshareFileOrFolderInfo>>GetUsersFileOrFolderInfo(int PageIndex, string Ext, string Path = "", FileOrFolder DirOnly = FileOrFolder.FileAndFolder, int Limit = 1)
        {
            try
            {
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Clear();
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/list?ext=" + Ext + "?path=" + Path + "?dirOnly=" + DirOnly.ToString() + "?limit=" + Limit.ToString() + "?pageIndex=" + PageIndex.ToString())
                };
                HttpResponseMessage objResponse = await objHttpClient.SendAsync(objRequest);
                if (objResponse.IsSuccessStatusCode)
                {
                    string strResponseText = objResponse.Content.ReadAsStringAsync().Result;
                    if (strResponseText != null)
                    {
                        return JsonConvert.DeserializeObject<List<FshareFileOrFolderInfo>>(strResponseText);
                    }
                }
                return null;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<long>GetTotalFileInFolder(string FolderURL, bool HaveFile = false)
        {
            string strHaveFile;
            if (HaveFile)
            {
                strHaveFile = "true";
            }
            else { strHaveFile = "false"; }
            HttpClient objHttpClient = ActiveService;
            objHttpClient.DefaultRequestHeaders.Clear();
            objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
            StringContent ResponseBody = new StringContent(JsonConvert.SerializeObject(new
            {
                token = Token,
                url = FolderURL,
                have_file = strHaveFile
            }));
            HttpRequestMessage objRequest = new HttpRequestMessage
            {
                RequestUri = new Uri("https://api.fshare.vn/api/fileops/getTotalFileInFolder"),
                Content = ResponseBody,
                Method = HttpMethod.Post
            };
            objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage objResponse = await objHttpClient.SendAsync(objRequest);
            if (objResponse.IsSuccessStatusCode)
            {
                string strResponseText = objResponse.Content.ReadAsStringAsync().Result;
                if (strResponseText != null)
                {
                    GetTotalFileInFolderResponse objJson = JsonConvert.DeserializeObject<GetTotalFileInFolderResponse>(strResponseText);
                    return Convert.ToInt64(objJson.Total);
                }
            }
            return 0;
        }
        private string GetFileNameFromURL(string URL)
        {
            string strURL = System.Web.HttpUtility.UrlDecode(URL);
            string strFileName = strURL.Substring(URL.LastIndexOf("/", strURL.Length), strURL.Length - URL.LastIndexOf("/", strURL.Length));
            return strFileName.Substring(1);
        }
    }
}
