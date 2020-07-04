using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Vrasoft.DebugSignatures
{
    public class DebugSignaturesReader
    {
        public static DebugSignatureReading ReadFromExecutable(string executablePath)
        {
            using (var dllStream = File.OpenRead(executablePath))
            {
                return new DebugSignatureReading(executablePath, ReadFromExecutableStream(dllStream));
            }
        }

        public static DebugSignatureReading ReadFromPdb(string pdbPath)
        {
            using (var pdbStream = File.OpenRead(pdbPath))
            {
                return new DebugSignatureReading(pdbPath, ReadFromPdbStream(pdbStream));
            }
        }

        public static List<DebugSignatureReading> ReadFromArchive(string packagePath)
        {
            var signatures = new List<DebugSignatureReading>();
            using (var archive = ZipFile.OpenRead(packagePath))
            {
                archive.Entries.ToList().ForEach(packedFile =>
                {
                    var isDll = packedFile.Name.EndsWith(".dll");
                    var isPdb = packedFile.Name.EndsWith(".pdb");
                    var isExe = packedFile.Name.EndsWith(".exe");

                    if (isDll || isPdb || isExe)
                    {
                        using (var packedFileStream = packedFile.Open())
                        using (var memoryStream = new MemoryStream())
                        {
                            // PEReader requires stream to support read and seek operations.
                            packedFileStream.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            var signature = isPdb ? ReadFromPdbStream(memoryStream) : ReadFromExecutableStream(memoryStream);
                            signatures.Add(new DebugSignatureReading($"[{packagePath}]/{packedFile.FullName}", signature));
                        }

                    }
                });

                return signatures;
            }
        }

        public static List<DebugSignatureReading> ReadFromDirectory(string directoryPath)
        {
            var supportedExtensions = new[] { ".dll", ".exe", ".pdb", ".zip", ".nupkg", ".snupkg" };
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).Where(file => supportedExtensions.Contains(Path.GetExtension(file)));
            var signatures = new List<DebugSignatureReading>();

            foreach (var file in files)
            {
                try
                {
                    if (file.EndsWith(".pdb"))
                    {
                        signatures.Add(ReadFromPdb(file));
                    }
                    else if (file.EndsWith(".dll") || file.EndsWith(".exe"))
                    {
                        signatures.Add(ReadFromExecutable(file));
                    }
                    else
                    {
                        signatures.AddRange(ReadFromArchive(file));
                    }
                }
                catch (Exception e)
                {
                    // Just don't add the reading to results.
                }
            }

            return signatures;
        }

        private static string ReadFromExecutableStream(Stream inputStream)
        {
            using (var peReader = new PEReader(inputStream))
            {
                var debugDirectory = peReader.ReadDebugDirectory().First(entry => entry.Type == DebugDirectoryEntryType.CodeView);
                var codeViewData = peReader.ReadCodeViewDebugDirectoryData(debugDirectory);

                return $"{codeViewData.Guid.ToString("N").Replace("-", string.Empty)}FFFFFFFF".ToUpper();
            }
        }

        private static string ReadFromPdbStream(Stream pdbStream)
        {
            var metadataProvider = MetadataReaderProvider.FromPortablePdbStream(pdbStream);
            var metadataReader = metadataProvider.GetMetadataReader();
            var id = new BlobContentId(metadataReader.DebugMetadataHeader.Id);
            var guid = id.Guid.ToString("N");

            return $"{guid}FFFFFFFF".ToUpper();
        }
    }
}
