using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vrasoft.DebugSignatures
{
    /// <summary>
    /// Class used to trigger signature readings and group them by the signature's value.
    /// </summary>
    public class DebugSignaturesComparer
    {
        /// <summary>
        /// Invoked when an error occures during processing.
        /// </summary>
        public EventHandler<string> ProcessingError;

        /// <summary>
        /// List of successfull readings done by this instance.
        /// </summary>
        public IList<DebugSignatureReading> Readings { get; set; } = new List<DebugSignatureReading>();
        
        /// <summary>
        /// Readings grouped by the signature ordered by the number of files within group in descending order.
        /// </summary>
        public IList<IGrouping<string, DebugSignatureReading>> ReadingsBySignature =>
            Readings
                .GroupBy(reading => reading.DebugSignature)
                .OrderByDescending(readings => readings.Count())
                .ToList();

        /// <summary>
        /// Adds file to the comparison - reads its debug signature and adds the result to <see cref="Readings"/> and <see cref="ReadingsBySignature"/>if it succeeeds.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        public void AddFile(string path)
        {
            AddFiles(new[] { path });
        }

        /// <summary>
        /// Adds files to the comparison - reads their debug signature and adds the results to <see cref="Readings"/> and <see cref="ReadingsBySignature"/>if they succeeed.
        /// </summary>
        /// <param name="path">Paths to files.</param>
        public void AddFiles(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    switch (path)
                    {
                        case string pdbPath when File.Exists(pdbPath) && DebugSignaturesReader.ProgramDatabaseExtensions.Contains(Path.GetExtension(pdbPath)):
                            Readings.Add(DebugSignaturesReader.ReadFromProgramDatabase(pdbPath));
                            break;
                        case string exePath when File.Exists(exePath) && DebugSignaturesReader.PortableExecutableExtensions.Contains(Path.GetExtension(exePath)):
                            Readings.Add(DebugSignaturesReader.ReadFromPortableExecutable(exePath));
                            break;
                        case string archivePath when File.Exists(archivePath) && DebugSignaturesReader.ArchiveExtensions.Contains(Path.GetExtension(archivePath)):
                            DebugSignaturesReader.ReadFromArchive(archivePath).ForEach(Readings.Add);
                            break;
                        case string directoryPath when Directory.Exists(directoryPath):
                            DebugSignaturesReader.ReadFromDirectory(directoryPath).ForEach(Readings.Add);
                            break;
                        default:
                            ProcessingError?.Invoke(this, $"Could not handle item: \"{path}\". Ensure that path is valid and points to supported type of resource.");
                            break;
                    }
                }
                catch (Exception e)
                {
                    ProcessingError?.Invoke(this, $"Exception occured during processing item: \"{path}\". Exception message {e.Message}");
                }
            }

            // Remove duplicate files
            Readings
                .GroupBy(reading => reading.File)
                .ToList()
                .ForEach(group => group.Skip(1).ToList().ForEach(reading => Readings.Remove(reading)));
        }
    }
}
