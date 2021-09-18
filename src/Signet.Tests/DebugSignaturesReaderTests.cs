using NUnit.Framework;
using Signet;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Signet.Tests
{
    public class DebugSignaturesReaderTests
    {
        private static string PathToValidPortableExecutableFile;
        private static string PathToValidProgramDatabaseFile;
        private static string PathToInvalidPortableExecutableFile;
        private static string PathToInvalidProgramDatabaseFile;

        private static string PathToArchive;

        [OneTimeSetUp]
        public void Setup()
        {
            PathToValidPortableExecutableFile = Assembly.GetExecutingAssembly().Location;
            PathToInvalidPortableExecutableFile = "InvalidPortableExecutable.exe";
            File.WriteAllText(PathToInvalidPortableExecutableFile, "This file is invalid.");

            PathToValidProgramDatabaseFile = PathToValidPortableExecutableFile.Replace(Path.GetExtension(PathToValidPortableExecutableFile), ".pdb");
            PathToInvalidProgramDatabaseFile = "InvalidPortableExecutable.pdb";
            File.WriteAllText(PathToInvalidProgramDatabaseFile, "This file is invalid.");

            PathToArchive = "archive.zip";
            using (ZipArchive zip = ZipFile.Open(PathToArchive, ZipArchiveMode.Update))
            {
                zip.CreateEntryFromFile(PathToValidProgramDatabaseFile, "valid.pdb");
                zip.CreateEntryFromFile(PathToInvalidProgramDatabaseFile, "invalid.pdb");
                zip.CreateEntryFromFile(PathToValidPortableExecutableFile, "valid.exe");
                zip.CreateEntryFromFile(PathToInvalidPortableExecutableFile, "invalid.exe");
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            File.Delete(PathToInvalidProgramDatabaseFile);
            File.Delete(PathToInvalidPortableExecutableFile);
            File.Delete(PathToArchive);
        }

        [Test]
        public void Read_FromExecutableFile_ReadingIsSuccessful_ReadingContainsProperPath_DebugSignatureIsNeiterNullNorEmpty_ErrorIsNullNorEmpty()
        {
            var reader = new DebugSignaturesReader();
            
            var reading = reader.Read(PathToValidPortableExecutableFile).Single();

            Assert.Multiple(() =>
            {
                Assert.That(reading.IsSuccessful, Is.True);
                Assert.That(reading.Error, Is.Null.Or.Empty);
                Assert.That(reading.File, Is.EqualTo(Path.GetFullPath(PathToValidPortableExecutableFile)));
                Assert.That(reading.DebugSignature, Is.Not.Null.And.Not.Empty);
            });
        }

        [Test]
        public void Read_FromInvalidExecutableFile_ReadingIsNotSuccessful_ReadingContainsProperPath_DebugSignatureIsNull_ErrorIsNeitherNullNorEmpty()
        {
            var reader = new DebugSignaturesReader();

            var reading = reader.Read(PathToInvalidPortableExecutableFile).Single();

            Assert.Multiple(() =>
            {
                Assert.That(reading.IsSuccessful, Is.False);
                Assert.That(reading.Error, Is.Not.Null.And.Not.Empty);
                Assert.That(reading.File, Is.EqualTo(Path.GetFullPath(PathToInvalidPortableExecutableFile)));
                Assert.That(reading.DebugSignature, Is.Null.Or.Empty);
            });
        }

        [Test]
        public void Read_FromArchiveContainingValidAndInvalidPortableExecutableAndValidAndInvalidProgramDatabase_ReturnsFourReadingsFromWhichTwoAresuccessfulAndContainMatchingSignatures()
        {
            var reader = new DebugSignaturesReader();
            var readings = reader.Read(PathToArchive);

            Assert.Multiple(() =>
            {
                Assert.That(readings.Count, Is.EqualTo(4));
                Assert.That(readings.Count(r => r.IsSuccessful), Is.EqualTo(2));
                Assert.That(readings.Count(r => !r.IsSuccessful), Is.EqualTo(2));
                Assert.That(readings.Where(r => r.IsSuccessful).GroupBy(r => r.DebugSignature).Count, Is.EqualTo(1));
            });
        }
    }
}