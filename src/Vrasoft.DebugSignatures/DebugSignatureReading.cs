namespace Vrasoft.DebugSignatures
{
    public class DebugSignatureReading
    {
        public string File { get; }
        public string DebugSignature { get; }

        public DebugSignatureReading(string file, string debugSignature)
        {
            File = file;
            DebugSignature = debugSignature;
        }
    }
}
