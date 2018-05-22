namespace ShareDataSite.Models
{
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