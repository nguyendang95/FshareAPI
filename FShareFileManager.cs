using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Web.UI.WebControls;

namespace FshareAPI
{
    internal class Status
    {
        public string Code { get; set; }
        public string Msg { get; set; }
    }
    internal class Root
    {
        public NewFolder Folder { get; set; }
    }
    internal class NewFolder
    {
        public string Name { get; set; }
        public string LinkCode { get; set; }
    }
    internal class GetTotalFileInFolderResponse
    {
        public string Code { get; set; }
        public string Total { get; set; }
    }
    internal class UploadDownloadResponse
    {
        public string Code { get; set; }
        public string Location { get; set; }
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
        public async Task UploadFileAsync(string FileName, string DestinationFolder = "/", bool Secured = false)
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
            Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequestBody);
            HttpResponseMessage objResponse = await HttpResponseMessageTask;
            if (objResponse.IsSuccessStatusCode)
            {
                Task<string> strResponseTextTask = objResponse.Content.ReadAsStringAsync();
                string strResponseText = await strResponseTextTask;
                if (strResponseText != null)
                {
                    UploadDownloadResponse objJson = JsonConvert.DeserializeObject<UploadDownloadResponse>(strResponseText);
                    if (objJson != null && objJson.Code == "200")
                    {
                        Task UploadFileToURLTask = UploadFileToURLAsync(objJson.Location, FileName);
                        await UploadFileToURLTask;
                    }
                }    
            }
        }
        private async Task UploadFileToURLAsync(string URL, string FileName)
        {
            try
            {
                FileStream objFileStream = new FileStream(FileName, FileMode.Open);
                byte[] arrByteArray = new byte[objFileStream.Length];
                objFileStream.Read(arrByteArray, 0, arrByteArray.Length);
                objFileStream.Close();
                ByteArrayContent objByteArrayContent = new ByteArrayContent(arrByteArray);
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequestBody = new HttpRequestMessage
                {
                    Content = objByteArrayContent,
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(URL)
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequestBody);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                if (!objResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to upload file");
                }
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task DownloadFileAsync(string URL, string DestinationFolder, string Password = "", int ZipFlag = 0)
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
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequestBody);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                if (objResponse.IsSuccessStatusCode)
                {
                    Task<string> strResponseTextTask = objResponse.Content.ReadAsStringAsync();
                    string strResponseText = await strResponseTextTask;
                    if (strResponseText != null)
                    {
                        UploadDownloadResponse objJson = JsonConvert.DeserializeObject<UploadDownloadResponse>(strResponseText);
                        Task DownloadFileFromURLTask = DownloadFileFromURLAsync(objJson.Location, DestinationFolder);
                        await DownloadFileFromURLTask;
                    }
                }
            }
            catch
            {
                throw new Exception();
            }
        }
        private async Task DownloadFileFromURLAsync(string URL, string DestinationFolder)
        {
            try
            {
                string strFile = DestinationFolder + "\\" + GetFileNameFromURL(URL);
                HttpClient objHttpClient = ActiveService;
                Task<Stream> StreamTask = objHttpClient.GetStreamAsync(URL);
                Stream objStream = await StreamTask;
                FileStream objFileStream = new FileStream(strFile, FileMode.CreateNew);
                objStream.CopyTo(objFileStream);
                objStream.Close();
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<List<FshareFileOrFolderInfo>>GetUsersFileOrFolderInfoAsync(int PageIndex, string Ext, string Path = "", FileOrFolder DirOnly = FileOrFolder.FileAndFolder, int Limit = 1)
        {
            try
            {
                HttpClient objHttpClient = ActiveService;
                
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/list?ext=" + Ext + "?path=" + Path + "?dirOnly=" + DirOnly.ToString() + "?limit=" + Limit.ToString() + "?pageIndex=" + PageIndex.ToString())
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
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
        public async Task<long>GetTotalFileInFolderAsync(string FolderURL, bool HaveFile = false)
        {
            string strHaveFile;
            if (HaveFile)
            {
                strHaveFile = "true";
            }
            else { strHaveFile = "false"; }
            HttpClient objHttpClient = ActiveService;
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
            Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
            HttpResponseMessage objResponse = await HttpResponseMessageTask;
            if (objResponse.IsSuccessStatusCode)
            {
                Task<string> strResponseTextTask = objResponse.Content.ReadAsStringAsync();
                string strResponseText = await strResponseTextTask;
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
        public async Task<string> CreateFolderAsync(string FolderName, string InDir = "0")
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    name = FolderName,
                    token = Token,
                    in_dir = InDir
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/createFolder"),
                    Content = RequestBody
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                if (objResponse.IsSuccessStatusCode)
                {
                    Task<string> ResponseTextTask = objResponse.Content.ReadAsStringAsync();
                    string strResponseText = await ResponseTextTask;
                    if (strResponseText != null)
                    {
                        Root objJson = JsonConvert.DeserializeObject<Root>(strResponseText);
                        return objJson.Folder.LinkCode;
                    }
                }
                return null;
            }
            catch
            {
                throw new Exception();
            }
        }
        /// <summary>
        /// Đổi tên hoặc tập tin. Trả về mã Status Code giúp xác định hành động đổi tên có thành công hay không.
        /// </summary>
        /// <param name="NewName"></param>
        /// <param name="LinkCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> RenameFileOrFolderAsync(string NewName, string LinkCode)
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    new_name = NewName,
                    file = LinkCode,
                    token = Token
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/rename"),
                    Method = HttpMethod.Post,
                    Content = RequestBody
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<int>MoveFileOrFolderAsync(List<string> FileOrFolderLinkCodes, string DestinationFolderLinkCode = "0")
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    items = FileOrFolderLinkCodes,
                    token = Token,
                    to = DestinationFolderLinkCode
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/move"),
                    Method = HttpMethod.Post,
                    Content = RequestBody
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<int>DeleteFileOrFolderAsync(List<string>FileOrFolderLinkCodes)
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    items = FileOrFolderLinkCodes,
                    token = Token
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/delete"),
                    Method = HttpMethod.Post,
                    Content = RequestBody
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<int>CreateFilePassAsync(List<string>FileOrFolderLinkCodes, string Password = "")
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    items = FileOrFolderLinkCodes,
                    token = Token,
                    pass = Password
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/createFilePass"),
                    Method = HttpMethod.Post,
                    Content = RequestBody
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<int>ChangeSecureAsync(List<string>FileLinkCodes, int Status = 0)
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    items = FileLinkCodes,
                    token = Token,
                    status = Status
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/changSecure"),
                    Method = HttpMethod.Post,
                    Content = RequestBody
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<int> SetDirectLinkAsync(List<string>FileLinkCodes, int Status = 0)
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    items = FileLinkCodes,
                    token = Token,
                    status = Status
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/setDirectLink"),
                    Method = HttpMethod.Post,
                    Content = RequestBody
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<int> Duplicate(string FileLinkCode, string DestinationFolderLinkCode)
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    linkcode = FileLinkCode,
                    path = DestinationFolderLinkCode,
                    confirm = true,
                    token = Token
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/duplicate"),
                    Content = RequestBody,
                    Method = HttpMethod.Post
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<List<FshareFileOrFolderInfo>>GetPublicFolderListAsync(string FolderURL, int PageIndex, string Path = null, int DirOnly = 0, int Limit = 100)
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    pageIndex = PageIndex,
                    dirOnly = DirOnly,
                    limit = Limit,
                    path = Path,
                    url = FolderURL,
                    token = Token
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/getFolderList"),
                    Content = RequestBody,
                    Method = HttpMethod.Post
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                if (objResponse.IsSuccessStatusCode)
                {
                    Task<string> ResponseTextTask = objResponse.Content.ReadAsStringAsync();
                    string strResponseText = await ResponseTextTask;
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
        public async Task<FShareTopFollowMovie>GetTopFollowMovieAsync()
        {
            HttpClient objHttpClient = ActiveService;
            objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
            objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            HttpRequestMessage objRequest = new HttpRequestMessage
            {
                RequestUri = new Uri("https://api.fshare.vn/api/fileops/getTopFollowMovie"),
                Method = HttpMethod.Get
            };
            Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
            HttpResponseMessage objResponse = await HttpResponseMessageTask;
            if (objResponse.IsSuccessStatusCode)
            {
                Task<string>ResponseTextTask = objResponse.Content.ReadAsStringAsync();
                string strResponseText = await ResponseTextTask;
                if (strResponseText != null)
                {
                    return JsonConvert.DeserializeObject<FShareTopFollowMovie>(strResponseText);
                }
            }
            return null;
        }
        public async Task<FShareListFollow>GetListFollowAsync()
        {
            try
            {
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/getListFollow"),
                    Method = HttpMethod.Get
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                if (objResponse.IsSuccessStatusCode)
                {
                    Task<string> ResponseTextTask = objResponse.Content.ReadAsStringAsync();
                    string strResponseText = await ResponseTextTask;
                    if (strResponseText != null)
                    {
                        return JsonConvert.DeserializeObject<FShareListFollow>(strResponseText);
                    }
                }
                return null;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<int>FollowFolderAsync(string FolderURL)
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    link = FolderURL,
                    token = Token
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/followFolder"),
                    Method = HttpMethod.Post,
                    Content = RequestBody
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<int>UnfollowFolderAsync(string FolderURL)
        {
            try
            {
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    link = FolderURL,
                    token = Token
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/unfollowFolder"),
                    Method = HttpMethod.Post,
                    Content = RequestBody
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
        public async Task<List<FshareFileOrFolderInfo>>ListFavoriteAsync()
        {
            try
            {
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/listFavorite"),
                    Method = HttpMethod.Get
                };
                Task<HttpResponseMessage> HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse = await HttpResponseMessageTask;
                if (objResponse.IsSuccessStatusCode)
                {
                    Task<string> ResponseTextTask = objResponse.Content.ReadAsStringAsync();
                    string strResponseText = await ResponseTextTask;
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
        public async Task<int>ChangeFavoriteAsync(List<string>LinkCodes, ChangeFavoriteStatus Status = ChangeFavoriteStatus.FavoriteEnabled)
        {
            try
            {
                int intStatus;
                switch (Status)
                {
                    case ChangeFavoriteStatus.FavoriteEnabled:
                        intStatus = 1;
                        break;
                    case ChangeFavoriteStatus.FavoriteDisabled:
                        intStatus = 0;
                        break;
                    default: throw new ArgumentException("The value you entered for Status pamameter does not lie within the given range. Use a constant from the ChangeFavoriteStatus enumeration.");
                }
                StringContent RequestBody = new StringContent(JsonConvert.SerializeObject(new
                {
                    items = LinkCodes,
                    status = intStatus
                }));
                HttpClient objHttpClient = ActiveService;
                objHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                objHttpClient.DefaultRequestHeaders.Add("session_id", SessionId);
                objHttpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                HttpRequestMessage objRequest = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.fshare.vn/api/fileops/changeFavorite"),
                    Content = RequestBody,
                    Method = HttpMethod.Post
                };
                objRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                Task<HttpResponseMessage>HttpResponseMessageTask = objHttpClient.SendAsync(objRequest);
                HttpResponseMessage objResponse =  await HttpResponseMessageTask;
                return (int)objResponse.StatusCode;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
