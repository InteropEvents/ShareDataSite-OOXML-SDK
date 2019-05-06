using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShareDataService;

namespace ShareDataSiteNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RawDataController : ControllerBase
    {
        [HttpPost]
        public string GetHtmlAfterTransformData()
        {
            try
            {
                InputObject inputOjbect = null;
                try
                {
                    using (var reader = new StreamReader(Request.Body))
                    {
                        string json = reader.ReadToEnd();
                        inputOjbect = JsonConvert.DeserializeObject<InputObject>(json);
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }

                string downloadUri = inputOjbect.Downloaduri;
                string accessToken = inputOjbect.AccessToken;
                string fileId = inputOjbect.FileId;

                // Download the file in Onedrive.
                var webclient = new WebClient();
                byte[] data = webclient.DownloadData(downloadUri);

                // Get the file name.
                var fileName = webclient.ResponseHeaders.GetValues("Content-Disposition").FirstOrDefault();
                fileName = fileName.Replace("\"", "");
                var parse = new WriteRawDataToFile();
                if (fileName.ToLower().EndsWith(".docx"))
                {
                    parse = new WordParse(data, accessToken, fileId);
                }
                else if (fileName.ToLower().EndsWith(".xlsx"))
                {
                    parse = new ExcelParse(data, accessToken, fileId);
                }
                else if (fileName.ToLower().EndsWith(".pptx"))
                {
                    parse = new PowerPointParse(data, accessToken, fileId);
                }
                else
                {
                    return string.Empty;
                }

                return parse.TempDataToHtmlAndUploadToOneDrive();
            }
            catch (Exception x)
            {
                Response.StatusCode = 400;
                return x.Message;
            }

        }
    }

    /// <summary>
    /// Request input object.
    /// </summary>
    public class InputObject
    {
        /// <summary>
        /// OneDrive access token.
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// File id in OneDrive.
        /// </summary>
        public string FileId { get; set; }
        public string Downloaduri { get; set; }
    }
}