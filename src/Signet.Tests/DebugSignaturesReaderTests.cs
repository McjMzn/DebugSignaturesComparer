using NUnit.Framework;
using Signet;
using System;
using System.IO;

namespace Signet.Tests
{
    public class DebugSignaturesReaderTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ReadFromPortableExecutable_ExecutableExists_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => DebugSignaturesReader.ReadFromPortableExecutable("Signet.Tests.dll"));
        }

        [Test]
        public void ReadFromProgramDatabase_ProgramDatabaseExists_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => DebugSignaturesReader.ReadFromPortableExecutable(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Signet.Tests.pdb")));
        }
    }
}