namespace Vrasoft.DebugSignatures
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
        /// Initializes a new instance of <see cref="DebugSignatureReading"/>
        /// </summary>
        /// <param name="file">Path to the file.</param>
        /// <param name="debugSignature">Value of the debug signature.</param>
        public DebugSignatureReading(string file, string debugSignature)
        {
            File = file;
            DebugSignature = debugSignature;
        }
    }
}
