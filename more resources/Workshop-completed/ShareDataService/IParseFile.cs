namespace ShareDataService
{
    /// <summary>
    /// Extract file content interface.
    /// </summary>
    public interface IParseFile
    {
        /// <summary>
        /// Reading file raw data from file byte data.
        /// </summary>
        /// <param name="data">File resources as byte arrays.</param>
        /// <returns>An array of objects containing raw data.</returns>
        TempData[] ReadFileRawDataFromByteArray(byte[] data);
    }
}
