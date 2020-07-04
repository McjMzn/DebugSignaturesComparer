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

        public IDictionary<string, List<DebugSignatureReading>> ReadingsBySignature { get; set; } = new Dictionary<string, List<DebugSignatureReading>>();

        /// <summary>
        /// Gets a flag indicating if all successful readings shared the same debug signature.
        /// </summary>
        public bool AllMatching => this.ReadingsBySignature.Keys.Count == 1;

        /// <summary>
        /// Checks if elements under the given paths have matching debug signatures.
        /// </summary>
        /// <param name="paths">Paths to check.</param>
        /// <returns>True if are matching, false otherwise.</returns>
        public static bool AreMatching(params string[] paths)
        {
            var comparer = new DebugSignaturesComparer();
            comparer.AddFiles(paths);
            return comparer.AllMatching;
        }

        /// <summary>
        /// Adds file or directory to the comparison - reads its debug signature and adds the result to <see cref="Readings"/>.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        public void AddFile(string path)
        {
            AddFiles(new[] { path });
        }

        /// <summary>
        /// Adds files or directories to the comparison - reads their debug signature and adds the results to <see cref="Readings"/>.
        /// </summary>
        /// <param name="path">Paths to files.</param>
        public void AddFiles(IEnumerable<string> paths)
        {
            paths = this.FlattenPathList(paths);
            
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
            this.Readings
                .GroupBy(reading => reading.File)
                .ToList()
                .ForEach(group => group.Skip(1).ToList().ForEach(reading => Readings.Remove(reading)));

            // Group reading by signature
            this.ReadingsBySignature = this.Readings
                .GroupBy(reading => reading.DebugSignature)
                .OrderByDescending(readings => readings.Count())
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        /// <summary>
        /// Processes the given list of paths. Replaces directory paths with paths to supported files within them.
        /// </summary>
        /// <param name="paths">List of paths</param>
        /// <returns>List of file paths</returns>
        private List<string> FlattenPathList(IEnumerable<string> paths)
        {
            return paths
                .ToList()
                .SelectMany(path =>
                {
                    var isDirectory = Directory.Exists(path);
                    if (!isDirectory)
                    {
                        return new[] { path };
                    }

                    return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Where(file => DebugSignaturesReader.SupportedExtensions.Contains(Path.GetExtension(file)));
                }).Distinct().ToList();
        }
    }
}
