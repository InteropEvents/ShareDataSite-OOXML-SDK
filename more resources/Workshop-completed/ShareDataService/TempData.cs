using System.Collections.Generic;

namespace ShareDataService
{
    /// <summary>
    /// Objects with RawData.
    /// </summary>
    public class TempData
    {
        /// <summary>
        /// Raw data type.
        /// </summary>
        public StorageType StorageType { get; set; }

        /// <summary>
        /// Raw data.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Wrap raw data into a TempData object of the specified type.
        /// </summary>
        /// <param name="storageType">Raw data type.</param>
        /// <param name="dataList">Raw data list.</param>
        /// <returns>A collection of objects with raw data.</returns>
        public static IEnumerable<TempData> GetTempDataIEnumerable(StorageType storageType, IEnumerable<string> dataList)
        {
            foreach (var data in dataList)
            {
                yield return new TempData { StorageType = storageType, Data = data };
            }
        }
    }
}
