using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ShareDataService
{
    /// <summary>
    /// Extract raw data in Excel.
    /// </summary>
    public class ExcelParse : WriteRawDataToFile, IParseFile
    {
        /// <summary>
        /// ExcelParse constructor.
        /// </summary>
        /// <param name="data">File resources as byte arrays.</param>
        /// <param name="accessToken">OneDrive access token.</param>
        /// <param name="fileId">File id in OneDrive.</param>
        public ExcelParse(byte[] data, string accessToken, string fileId)
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
                using (SpreadsheetDocument spreadsheetDocument =
                 SpreadsheetDocument.Open(new MemoryStream(data), false))
                {
                    IEnumerable<Row> rows = null;
                    string[] cellTexts = null;
                    IEnumerable<string> rowTexts = null;
                    Stream stream = null;
                    byte[] streamByteArray = null;
                    List<TempData> result = new List<TempData>();

                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                    // Get all Sheets in the document.
                    var sheets = workbookPart.Workbook.Descendants<Sheet>();

                    foreach (var sheet in sheets)
                    {
                        WorksheetPart worksheetPart =
                       (WorksheetPart)workbookPart.GetPartById(sheet.Id);

                        // Get all the rows in the sheet.
                        rows = worksheetPart.Worksheet.Descendants<Row>();

                        // Get the text in each row.
                        rowTexts = rows.Select(m =>
                        {
                            // Get the text in each cell.
                            cellTexts = m.Descendants<Cell>().Select(cell =>
                            {
                                var cellText = GetCellText(cell, workbookPart);
                                if (string.IsNullOrEmpty(cellText))
                                {
                                    return "";
                                }
                                // For the brower can display xml snippet normally.
                                cellText = cellText.Replace("<", @"&lt;");
                                return "<td>" + cellText + "</td>";
                            }).ToArray();

                            return "<tr>" + string.Join(string.Empty, cellTexts) + "</tr>";
                        });

                        result.Add(new TempData { StorageType = StorageType.TableType, Data = string.Join(string.Empty, rowTexts) });

                        // Get all the images in the document.
                        // Insert code here!!


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
        /// Get text in a cell.
        /// </summary>
        /// <param name="cell">Cell object.</param>
        /// <param name="workbookPart">WorkbookPart object.</param>
        /// <returns>Cell text.</returns>
        private string GetCellText(Cell cell, WorkbookPart workbookPart)
        {
            var value = cell.InnerText;
            if (cell.DataType != null)
            {
                switch (cell.DataType.Value)
                {
                    case CellValues.SharedString:

                        var sharedStringTablePart =
                           workbookPart.GetPartsOfType<SharedStringTablePart>()
                           .FirstOrDefault();

                        if (sharedStringTablePart != null)
                        {
                            value =
                               sharedStringTablePart.SharedStringTable
                               .ElementAt(int.Parse(value)).InnerText;
                        }

                        break;

                    case CellValues.Boolean:
                        switch (value)
                        {
                            case "0":
                                value = "FALSE";
                                break;
                            default:
                                value = "TRUE";
                                break;
                        }

                        break;
                }
            }

            return value;
        }
    }
}
