using System;
using System.Collections.Generic;
using System.Text;

namespace Signet
{
    /// <summary>
    /// Represents a class capable of reading the debug signatures.
    /// </summary>
    public interface IDebugSignaturesReader
    {
        /// <summary>
        /// If the path points to a file reads its debug signature.
        /// If the path points to a directory, reads the signatures from all supported types of files.
        /// </summary>
        /// <param name="path">Path to the file or a directory.</param>
        /// <returns>Debug signature reading.</returns>
        List<DebugSignatureReading> Read(string path);
    }
}
