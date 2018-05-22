using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace ShareDataService
{
    /// <summary>
    /// Write raw data to file and upload to onedrive.
    /// </summary>
    public class WriteRawDataToFile
    {
        /// <summary>
        /// DocumentRelationshipType.
        /// </summary>
        public const string DocumentRelationshipType =
          "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";

        /// <summary>
        /// Base address.
        /// </summary>
        public const string EndpointBase = @"https://graph.microsoft.com/v1.0";

        /// <summary>
        /// RawData upload address.
        /// </summary>
        public const string RawDataPath = @"/SharedDataApp/RawData/";

        /// <summary>
        /// StylesRelationshipType.
        /// </summary>
        public const string StylesRelationshipType =
          "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles";

        /// <summary>
        /// Delegate to upload instance object.
        /// </summary>
        public UploadFile UploadFileMethod { get; set; }

        /// <summary>
        /// OneDrive access token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// File id on ondrive.
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// An array of RawData.
        /// </summary>
        public TempData[] ParseTempDataArray { get; set; }

        /// <summary>
        /// Delegation definition of upload file.
        /// </summary>
        /// <param name="accessToken">OneDrive Access Token.</param>
        /// <param name="file">file stream.</param>
        /// <param name="fileName">File Name.</param>
        /// <param name="endpointBase">Base address.</param>
        /// <returns></returns>
        public delegate Task UploadFile(string accessToken, Stream file, string fileName, string endpointBase);

        /// <summary>
        /// Wrap Raw data in html and upload it to OneDrive.
        /// </summary>
        /// <returns>Raw data is converted to html strings.</returns>
        public virtual string TempDataToHtmlAndUploadToOneDrive()
        {
            var result = string.Empty;
            foreach (var tempData in this.ParseTempDataArray)
            {
                if (tempData==null||string.IsNullOrEmpty(tempData.Data))
                {
                    continue;
                }
                switch (tempData.StorageType)
                {
                    case StorageType.TextType:

                        // For the brower can display xml snippet normally.
                        tempData.Data = tempData.Data.Replace("<", @"&lt;");
                        result += "<div class=\"base text\">" + tempData.Data + @"</div>";
                        break;
                    case StorageType.ImageType:
                        result += "<div class=\"base image\"><img src=\"data:image/png;base64, " + tempData.Data + "\"/></div>";
                        break;
                    case StorageType.TableType:
                        result += "<div class=\"table-responsive\"><button class=\"ms-Button ms-Button--small\" style='display:none;'>" +
                                "<span class=\"ms-Button-label\">Insert</span>" +
                                "</button><table class=\"table\"><tbody>" + tempData.Data + @"</tbody></table></div>";
                        break;
                    default:
                        break;
                }
            }

            //// Convert raw data to stream objects.
            //var fileStream = GenerateStreamFromString(result);

            //// Judgment file stream content length greater than 4MB use UploadBigFileToOneDrive method, otherwise use UploadSmallFileToOneDrive method.
            //if (new byte[fileStream.Length].Length < (4 * 1024 * 1024))
            //{
            //    this.UploadFileMethod = UploadFileToOneDrive.UploadSmallFileToOneDrive;
            //}
            //else
            //{
            //    this.UploadFileMethod = UploadFileToOneDrive.UploadBigFileToOneDrive;
            //}

            //var fileName = RawDataPath;

            //try
            //{
            //    GraphServiceClient graphServiceClient = new GraphServiceClient(EndpointBase,
            //            new DelegateAuthenticationProvider(
            //               async (requestMessage) =>
            //                {
            //                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", this.AccessToken);
            //                }));

            //    var itemResponse = graphServiceClient.Me.Drive.Items[FileId].Request().GetAsync();
            //    fileName += itemResponse.Result.Name + ".rawdata";
            //}
            //catch (Exception ex)
            //{
            //    fileName += "noname.rawdata";
            //    throw ex;
            //}

            //// Call upload method.
            //UploadFileMethod(AccessToken, fileStream, fileName, EndpointBase);
            return result;
        }

        /// <summary>
        /// Convert string to stream object.
        /// </summary>
        /// <param name="s">String Object.</param>
        /// <returns>Stream Object.</returns>
        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
