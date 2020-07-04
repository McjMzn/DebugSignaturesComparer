using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vrasoft.DebugSignatures
{
    public class DebugSignaturesComparer
    {
        public EventHandler<string> ProcessingError;

        public IList<DebugSignatureReading> Readings { get; set; } = new List<DebugSignatureReading>();
        public IList<IGrouping<string, DebugSignatureReading>> ReadingsBySignature =>
            Readings
                .GroupBy(reading => reading.DebugSignature)
                .OrderByDescending(readings => readings.Count())
                .ToList();

        public void AddFile(string path)
        {
            AddFiles(new[] { path });
        }

        public void AddFiles(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    switch (path)
                    {
                        case string pdbPath when File.Exists(pdbPath) && pdbPath.EndsWith(".pdb"):
                            Readings.Add(DebugSignaturesReader.ReadFromPdb(pdbPath));
                            break;
                        case string executablePath when File.Exists(executablePath) && (executablePath.EndsWith(".dll") || executablePath.EndsWith(".exe")):
                            Readings.Add(DebugSignaturesReader.ReadFromExecutable(executablePath));
                            break;
                        case string archivePath when File.Exists(archivePath) && (archivePath.EndsWith(".nupkg") || archivePath.EndsWith(".snupkg") || archivePath.EndsWith(".zip")):
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
