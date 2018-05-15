using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace ShareDataService
{
    /// <summary>
    /// Extract raw data in PowerPoint.
    /// </summary>
    public class PowerPointParse : WriteRawDataToFile, IParseFile
    {
        /// <summary>
        /// PowerpointmlNamespace.
        /// </summary>
        private XNamespace powerpointmlNamespace = "http://schemas.openxmlformats.org/drawingml/2006/main";

        /// <summary>
        /// PowerPointParse constructor.
        /// </summary>
        /// <param name="data">File resources as byte arrays.</param>
        /// <param name="accessToken">OneDrive access token.</param>
        /// <param name="fileId">File id in OneDrive.</param>
        public PowerPointParse(byte[] data, string accessToken, string fileId)
        {
            base.ParseTempDataArray = this.ReadFileRawDataFromByteArray(data);
            base.AccessToken = accessToken;
            base.FileId = fileId;
        }

        /// <summary>
        /// Reading file raw data from file byte data.
        /// </summary>
        /// <param name="data">File resources as byte arrays.</param>
        /// <returns>An array of objects containing raw data.</returns>

        public TempData[] ReadFileRawDataFromByteArray(byte[] data)
        {
            try
            {
                using (PresentationDocument presentationDocument =
                 PresentationDocument.Open(new MemoryStream(data), false))
                {
                    List<TempData> result = new List<TempData>();

                    PresentationPart presentationPart = presentationDocument.PresentationPart;

                    OpenXmlElementList openXmlElementList = presentationPart.Presentation.SlideIdList.ChildElements;

                    // Get all SlideParts.
                    var slideParts = from item in openXmlElementList
                                     select (SlidePart)presentationPart.GetPartById((item as SlideId).RelationshipId);

                    // Retrieve the text of each slidePart.  
                    var slideText = from item in slideParts
                                    select GetSlideText(item);

                    result.AddRange(TempData.GetTempDataIEnumerable(StorageType.TextType, slideText));
                    Stream stream = null;
                    byte[] streamByteArray = null;

                    // Find image and add to the result.
                    foreach (var slide in slideParts)
                    {
                        result.AddRange(slide.ImageParts.Select(m =>
                        {
                            stream = m.GetStream();
                            streamByteArray = new byte[stream.Length];
                            stream.Read(streamByteArray, 0, (int)stream.Length);
                            return new TempData { StorageType = StorageType.ImageType, Data = Convert.ToBase64String(streamByteArray) };
                        }).ToArray());
                    }

                    return result.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get text in SlidePart.
        /// </summary>
        /// <param name="slidePart">SlidePart Object.</param>
        /// <returns>Text string.</returns>
        private string GetSlideText(SlidePart slidePart)
        {
            XDocument xDoc = XDocument.Load(XmlReader.Create(slidePart.GetStream()));
            if (xDoc == null)
            {
                return string.Empty;
            }
            return string.Join(string.Empty, xDoc.Root.Descendants(powerpointmlNamespace + "t").Select(m => (string)m));
        }

    }
}
