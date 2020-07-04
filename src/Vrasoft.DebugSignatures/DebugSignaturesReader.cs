using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Vrasoft.DebugSignatures
{
    /// <summary>
    /// Class that allows reading debug signature from: DLLs, EXEs, PDBs, NuGet packages, ZIP archives and directories.
    /// </summary>
    public class DebugSignaturesReader
    {
        public static string[] ProgramDatabaseExtensions = new[] { ".pdb" };
        public static string[] PortableExecutableExtensions = new[] { ".dll", ".exe", ".sys" };
        public static string[] ArchiveExtensions = new[] { ".zip", ".nupkg", ".snupkg" };
        public static string[] SupportedExtensions = PortableExecutableExtensions.Concat(ArchiveExtensions).Concat(ProgramDatabaseExtensions).ToArray();

        /// <summary>
        /// Reads the debug signature from the given Portable Executable (i.e. DLL or EXE.) file.
        /// </summary>
        /// <param name="portableExecutable">Path to the PE file.</param>
        /// <returns>Debug signature reading.</returns>
        public static DebugSignatureReading ReadFromPortableExecutable(string portableExecutable)
        {
            using (var portableExecutableStream = File.OpenRead(portableExecutable))
            {
                return new DebugSignatureReading(portableExecutable, ReadFromPortableExecutableStream(portableExecutableStream));
            }
        }

        /// <summary>
        /// Reads the debug signature from the given Program Database (PDB) file.
        /// </summary>
        /// <param name="executablePath">Path to the PDB file.</param>
        /// <returns>Debug signature reading.</returns>
        public static DebugSignatureReading ReadFromProgramDatabase(string pdbPath)
        {
            using (var pdbStream = File.OpenRead(pdbPath))
            {
                return new DebugSignatureReading(pdbPath, ReadFromProgramDatabase(pdbStream));
            }
        }

        /// <summary>
        /// Reads the debug signatures of DLLs, EXEs, PDBs, NUPKGs, SNUPKGs and ZIPs that the given archive contains.
        /// </summary>
        /// <param name="executablePath">Path to the archive.</param>
        /// <returns>List of debug signature readings.</returns>
        public static List<DebugSignatureReading> ReadFromArchive(string packagePath)
        {
            var signatures = new List<DebugSignatureReading>();
            using (var archive = ZipFile.OpenRead(packagePath))
            {
                archive.Entries.ToList().ForEach(packedFile =>
                {
                    var isExe = PortableExecutableExtensions.Contains(Path.GetExtension(packedFile.Name));
                    var isPdb = ProgramDatabaseExtensions.Contains(packedFile.Name);

                    if (isExe || isPdb)
                    {
                        using (var packedFileStream = packedFile.Open())
                        using (var memoryStream = new MemoryStream())
                        {
                            // PEReader requires stream to support read and seek operations.
                            packedFileStream.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            var signature = isPdb ? ReadFromProgramDatabase(memoryStream) : ReadFromPortableExecutableStream(memoryStream);
                            signatures.Add(new DebugSignatureReading($"[{packagePath}]/{packedFile.FullName}", signature));
                        }
                    }
                });

                return signatures;
            }
        }

        /// <summary>
        /// Reads the debug signatures of DLLs, EXEs, PDBs, NUPKGs, SNUPKGs and ZIPs that the given directory contains.
        /// <para>ATTENTION: This method will not throw if any reading fails. Returns only the successful results.</para>
        /// </summary>
        /// <param name="executablePath">Path to the directory.</param>
        /// <returns>List of debug signature readings.</returns>
        public static List<DebugSignatureReading> ReadFromDirectory(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).Where(file => SupportedExtensions.Contains(Path.GetExtension(file)));
            var signatures = new List<DebugSignatureReading>();

            foreach (var file in files)
            {
                try
                {
                    if (ProgramDatabaseExtensions.Contains(file))
                    {
                        signatures.Add(ReadFromProgramDatabase(file));
                    }
                    else if (PortableExecutableExtensions.Contains(Path.GetExtension(file)))
                    {
                        signatures.Add(ReadFromPortableExecutable(file));
                    }
                    else if (ArchiveExtensions.Contains(Path.GetExtension(file)))
                    {
                        signatures.AddRange(ReadFromArchive(file));
                    }
                }
                catch
                {
                    // Just don't add the reading to results.
                }
            }

            return signatures;
        }

        private static string ReadFromPortableExecutableStream(Stream inputStream)
        {
            using (var peReader = new PEReader(inputStream))
            {
                var debugDirectory = peReader.ReadDebugDirectory().First(entry => entry.Type == DebugDirectoryEntryType.CodeView);
                var codeViewData = peReader.ReadCodeViewDebugDirectoryData(debugDirectory);

                return $"{codeViewData.Guid.ToString("N").Replace("-", string.Empty)}FFFFFFFF".ToUpper();
            }
        }

        private static string ReadFromProgramDatabase(Stream pdbStream)
        {
            var metadataProvider = MetadataReaderProvider.FromPortablePdbStream(pdbStream);
            var metadataReader = metadataProvider.GetMetadataReader();
            var id = new BlobContentId(metadataReader.DebugMetadataHeader.Id);
            var guid = id.Guid.ToString("N");

            return $"{guid}FFFFFFFF".ToUpper();
        }
    }
}
