using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Signet
{
    /// <summary>
    /// Class that allows reading debug signature from: program databases, portable executables, nuget packages (including symbols) and zip archives.
    /// </summary>
    public class DebugSignaturesReader : IDebugSignaturesReader
    {
        private static string[] ProgramDatabaseExtensions = new[] { ".pdb" };
        private static string[] PortableExecutableExtensions = new[] { ".dll", ".exe", ".sys" };
        private static string[] ArchiveExtensions = new[] { ".zip", ".nupkg", ".snupkg" };
        
        /// <summary>
        /// Gets the list of file extensions supported by this reader.
        /// </summary>
        public static string[] SupportedExtensions =>
            PortableExecutableExtensions
                .Concat(ArchiveExtensions)
                .Concat(ProgramDatabaseExtensions)
                .ToArray();

        /// <summary>
        /// <inheritdoc/>
        /// <para>Supported files: .pdb .dll .exe .sys .nupkg .snupkg .zip</para>
        /// </summary>
        public List<DebugSignatureReading> Read(string path)
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                return new List<DebugSignatureReading> { new DebugSignatureReading(path, string.Empty, $"File or directory {path} does not exist.") };
            }

            var filesToRead = Directory.Exists(path) ? this.GetSupportedFilesFromDirectory(path) : new List<string> { path };

            return filesToRead.SelectMany(this.ReadFromAnyFile).ToList();
        }

        private List<DebugSignatureReading> ReadFromAnyFile(string filePath)
        {
            try
            {
                var extension = Path.GetExtension(filePath);
                switch (extension)
                {
                    case var pdbExtension when ProgramDatabaseExtensions.Contains(extension):
                        return new List<DebugSignatureReading> { this.ReadFromProgramDatabase(filePath) };

                    case var peExtension when PortableExecutableExtensions.Contains(extension):
                        return new List<DebugSignatureReading> { this.ReadFromPortableExecutable(filePath) };

                    case var archiveExtension when ArchiveExtensions.Contains(extension):
                        return this.ReadFromArchive(filePath);

                    default:
                        throw new NotSupportedException($"File {filePath} is of unsupported type.");
                }
            }
            catch (Exception e)
            {
                return new List<DebugSignatureReading> { new DebugSignatureReading(filePath, null, e.Message) };
            }
        }

        private DebugSignatureReading ReadFromPortableExecutable(string portableExecutablePath)
        {
            portableExecutablePath = Path.GetFullPath(portableExecutablePath);
            using (var portableExecutableStream = File.OpenRead(portableExecutablePath))
            {
                return new DebugSignatureReading(portableExecutablePath, this.ReadFromPortableExecutableStream(portableExecutableStream));
            }
        }

        private DebugSignatureReading ReadFromProgramDatabase(string pdbPath)
        {
            pdbPath = Path.GetFullPath(pdbPath);
            using (var pdbStream = File.OpenRead(pdbPath))
            {
                return new DebugSignatureReading(pdbPath, this.ReadFromProgramDatabaseStream(pdbStream));
            }
        }

        private List<DebugSignatureReading> ReadFromArchive(string packagePath)
        {
            var signatures = new List<DebugSignatureReading>();
            using (var archive = ZipFile.OpenRead(packagePath))
            {
                archive.Entries.ToList().ForEach(packedFile =>
                {
                    var fileExtension = Path.GetExtension(packedFile.Name);
                    var isExe = PortableExecutableExtensions.Contains(fileExtension);
                    var isPdb = ProgramDatabaseExtensions.Contains(fileExtension);

                    if (isExe || isPdb)
                    {
                        using (var packedFileStream = packedFile.Open())
                        using (var memoryStream = new MemoryStream())
                        {
                            // PEReader requires stream to support read and seek operations.
                            packedFileStream.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            try
                            {
                                var signature = isPdb ? this.ReadFromProgramDatabaseStream(memoryStream) : this.ReadFromPortableExecutableStream(memoryStream);
                                signatures.Add(new DebugSignatureReading($"{Path.Combine(packagePath, packedFile.FullName)}", signature));
                            }
                            catch (Exception e)
                            {
                                signatures.Add(new DebugSignatureReading($"{Path.Combine(packagePath, packedFile.FullName)}", null, e.Message));
                            }
                        }
                    }
                });

                return signatures;
            }
        }

        private string ReadFromPortableExecutableStream(Stream inputStream)
        {
            using (var peReader = new PEReader(inputStream))
            {
                var debugDirectory = peReader.ReadDebugDirectory().First(entry => entry.Type == DebugDirectoryEntryType.CodeView);
                var codeViewData = peReader.ReadCodeViewDebugDirectoryData(debugDirectory);

                return $"{codeViewData.Guid.ToString("N").Replace("-", string.Empty)}FFFFFFFF".ToUpper();
            }
        }

        private string ReadFromProgramDatabaseStream(Stream pdbStream)
        {
            var metadataProvider = MetadataReaderProvider.FromPortablePdbStream(pdbStream);
            var metadataReader = metadataProvider.GetMetadataReader();
            var id = new BlobContentId(metadataReader.DebugMetadataHeader.Id);
            var guid = id.Guid.ToString("N");

            return $"{guid}FFFFFFFF".ToUpper();
        }

        private List<string> GetSupportedFilesFromDirectory(string directoryPath)
        {
            return
                Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                .Where(file => SupportedExtensions.Contains(Path.GetExtension(file)))
                .ToList();
        }
    }
}
