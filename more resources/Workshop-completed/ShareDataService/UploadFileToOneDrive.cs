using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace ShareDataService
{
    /// <summary>
    /// Upload files to OneDrive.
    /// </summary>
    public class UploadFileToOneDrive
    {
        /// <summary>
        /// Upload up to 4MB size files to Onedrive.
        /// </summary>
        /// <param name="accessToken">OneDrive access token.</param>
        /// <param name="file">File stream object.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="endpointBase">Base address.</param>
        /// <returns></returns>
        public static async Task UploadSmallFileToOneDrive(string accessToken, Stream file, string fileName, string endpointBase)
        {
            string endpoint = string.Format("{0}/me/drive/root:/{1}:/content", endpointBase, fileName);
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, endpoint))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    request.Content = new StreamContent(file);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                    using (var response = await client.SendAsync(request))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new Exception("Upload to OneDrive Fail");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Upload files larger than 4MB to Onedrive.
        /// </summary>
        /// <param name="accessToken">OneDrive access token.</param>
        /// <param name="file">File stream object.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="endpointBase">Base address.</param>
        /// <returns></returns>
        public static async Task UploadBigFileToOneDrive(string accessToken, Stream file, string fileName, string endpointBase)
        {
            GraphServiceClient graphServiceClient = new GraphServiceClient(endpointBase,
                        new DelegateAuthenticationProvider(
                            async (requestMessage) =>
                            {
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                            }));

            var uploadSession = await graphServiceClient.Me.Drive.Root.ItemWithPath(fileName).CreateUploadSession().Request().PostAsync();
            // 320 KB - Change this to your chunk size. 5MB is the default.
            var maxChunkSize = 320 * 1024; 
            var provider = new ChunkedUploadProvider(uploadSession, graphServiceClient, file, maxChunkSize);
            // Setup the chunk request necessities.
            var chunkRequests = provider.GetUploadChunkRequests();
            var readBuffer = new byte[maxChunkSize];
            var trackedExceptions = new List<Exception>();
            DriveItem itemResult = null;
            // upload the chunks.
            foreach (var request in chunkRequests)
            {
                // Send chunk request.
                var result = await provider.GetChunkRequestResponseAsync(request, readBuffer, trackedExceptions);

                if (result.UploadSucceeded)
                {
                    itemResult = result.ItemResponse;
                }
            }

            if (itemResult == null)
            {
                throw new Exception("Upload to OneDirve Fail");
            }
        }
    }
}
