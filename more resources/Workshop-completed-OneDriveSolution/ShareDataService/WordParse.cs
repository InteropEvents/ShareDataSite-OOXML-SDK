using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ShareDataService
{
    /// <summary>
    /// Extract raw data in word.
    /// </summary>
    public class WordParse : WriteRawDataToFile, IParseFile
    {
        /// <summary>
        /// WordNamespace.
        /// </summary>
        private XNamespace wordNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        /// <summary>
        /// WordParse constructor.
        /// </summary>
        /// <param name="data">File resources as byte arrays.</param>
        /// <param name="accessToken">OneDrive access token.</param>
        /// <param name="fileId">File id in OneDrive.</param>
        public WordParse(byte[] data, string accessToken, string fileId)
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
                using (WordprocessingDocument wordprocessingDocument =
                 WordprocessingDocument.Open(new MemoryStream(data), false))
                {
                    List<TempData> result = new List<TempData>();
                    XDocument xDoc = null;
                    var wordPackage = wordprocessingDocument.Package;
                    PackageRelationship docPackageRelationship =
                            wordPackage
                            .GetRelationshipsByType(DocumentRelationshipType)
                            .FirstOrDefault();
                    if (docPackageRelationship != null)
                    {
                        Uri documentUri =
                            PackUriHelper
                            .ResolvePartUri(
                                new Uri("/", UriKind.Relative),
                                        docPackageRelationship.TargetUri);
                        PackagePart documentPart = wordPackage.GetPart(documentUri);

                        //  Load the document XML in the part into an XDocument instance.  
                        xDoc = XDocument.Load(XmlReader.Create(documentPart.GetStream()));
                    }

                    // Find all paragraphs in the document.  
                    var paragraphs =
                        from para in xDoc
                                     .Root
                                     .Element(wordNamespace + "body")
                                     .Descendants(wordNamespace + "p")
                        where !para.Parent.Name.LocalName.Equals("tc")
                        select new
                        {
                            ParagraphNode = para
                        };

                    // Retrieve the text of each paragraph.  
                    var paraWithText =
                        from para in paragraphs
                        select ParagraphText(para.ParagraphNode);

                    result.AddRange(TempData.GetTempDataIEnumerable(StorageType.TextType, paraWithText));

                    // Find all tables in the document.
                    var tables =
                        wordprocessingDocument.MainDocumentPart.Document.Body.Elements<Table>();

                    // Retrieve the text of each table.  
                    var tablesText = tables.Select(table =>
                     {
                         var rows = table.Elements<TableRow>();
                         var rowsText = rows.Select(row =>
                          {
                              var cells = row.Elements<TableCell>();
                              var cellsText = cells.Select(cell =>
                              {
                                  // Find the first paragraph in the table cell.
                                  Paragraph p = cell.Elements<Paragraph>().FirstOrDefault();

                                  if (p == null)
                                  {
                                      return "<td></td>";
                                  }
                                  // Find the first run in the paragraph.
                                  Run r = p.Elements<Run>().FirstOrDefault();
                                  if (r == null)
                                  {
                                      return "<td></td>";
                                  }

                                  // Set the text for the run.
                                  Text t = r.Elements<Text>().FirstOrDefault();
                                  var text = t == null ? string.Empty : t.Text;

                                  // For the brower can display xml snippet normally.
                                  text = text.Replace("<", @"&lt;");
                                  return "<td>" + text + "</td>";
                              });
                              return "<tr>" + string.Join(string.Empty, cellsText) + "</tr>";
                          });
                         if (rowsText != null && rowsText.Count() > 0)
                         {
                             return string.Join(string.Empty, rowsText);
                         }

                         return string.Empty;
                     });

                    result.AddRange(TempData.GetTempDataIEnumerable(StorageType.TableType, tablesText));

                    // Find image and add to the result.
                    var imageParts = wordprocessingDocument.MainDocumentPart.ImageParts;
                    byte[] streamByteArray = null;
                    Stream stream = null;
                    foreach (ImagePart item in imageParts)
                    {
                        stream = item.GetStream();
                        streamByteArray = new byte[stream.Length];
                        stream.Read(streamByteArray, 0, (int)stream.Length);
                        result.Add(new TempData { StorageType = StorageType.ImageType, Data = Convert.ToBase64String(streamByteArray) });
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
        /// Get paragraph text.
        /// </summary>
        /// <param name="e">Paragraph node objects.</param>
        /// <returns>Paragraph text string.</returns>
        private string ParagraphText(XElement e)
        {
            XNamespace w = e.Name.Namespace;
            return e
                   .Descendants(w + "t")
                   .StringConcatenate(element => (string)element);
        }
    }
}
