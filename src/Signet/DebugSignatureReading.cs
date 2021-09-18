namespace Signet
{
    /// <summary>
    /// Represents reading of a debug signature.
    /// </summary>
    public class DebugSignatureReading
    {
        /// <summary>
        /// File which had the signature read.
        /// </summary>
        public string File { get; }

        /// <summary>
        /// Debug signature of the file.
        /// </summary>
        public string DebugSignature { get; }

        /// <summary>
        /// Error that occurred during this reading.
        /// </summary>
        public string Error { get; }
        
        /// <summary>
        /// Flag informing if the reading was successful.
        /// </summary>
        public bool IsSuccessful => string.IsNullOrEmpty(this.Error);


        /// <summary>
        /// Initializes a new instance of <see cref="DebugSignatureReading"/>.
        /// </summary>
        /// <param name="file">Path to the file.</param>
        /// <param name="debugSignature">Value of the debug signature.</param>
        public DebugSignatureReading(string file, string debugSignature, string error = null)
        {
            this.File = file;

            // Using string empty instead of null to allow putting such signature into a dictionary.
            this.DebugSignature = debugSignature ?? string.Empty;
            this.Error = error;
        }
    }
}
