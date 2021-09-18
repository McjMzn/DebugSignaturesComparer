using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signet.Tests
{
    class DebugSignaturesComparerTests
    {
        private string aDllPath = "directory/a.dll";
        private string aPdbPath = "directory/a.pdb";
        private string aDirectoryPath = "directory";

        private string bDllPath = "randomDirectory/b.dll";
        private string cPdbPath = "randomDirectory/c.pdb";
        private string randomDirectoryPath = "randomDirectory";

        private string invalidPath = "invalid";

        IDebugSignaturesReader mockedReader;

        [OneTimeSetUp]
        public void Setup()
        {
            Dictionary<string, List<DebugSignatureReading>> mockedReadings = null;

            mockedReadings = new Dictionary<string, List<DebugSignatureReading>>
            {
                [aDllPath] = new List<DebugSignatureReading> { new DebugSignatureReading(aDllPath, "AAAA") },
                [aPdbPath] = new List<DebugSignatureReading> { new DebugSignatureReading(aPdbPath, "AAAA") },
                [aDirectoryPath] = new List<DebugSignatureReading> { new DebugSignatureReading(aDllPath, "AAAA"), new DebugSignatureReading(aPdbPath, "AAAA") },

                [bDllPath] = new List<DebugSignatureReading> { new DebugSignatureReading(bDllPath, "BBBB") },
                [cPdbPath] = new List<DebugSignatureReading> { new DebugSignatureReading(cPdbPath, "CCCC") },
                [randomDirectoryPath] = new List<DebugSignatureReading> { new DebugSignatureReading(bDllPath, "BBBB"), new DebugSignatureReading(cPdbPath, "CCCC") },

                [invalidPath] = new List<DebugSignatureReading> { { new DebugSignatureReading(invalidPath, null, "The path is invalid.")} }
            };

            var readerMock = new Mock<IDebugSignaturesReader>();
            readerMock.Setup(r => r.Read(It.IsAny<string>())).Returns<string>(path => mockedReadings[path]);
            this.mockedReader = readerMock.Object;
        }

        [Test]
        public void AddItem__GivenSingleValidItem__ComparerContains_OneReading_OneSignatureGroup_NoFailedReadings_AndReadingMatchedReturnsFalse()
        {
            var comparer = new DebugSignaturesComparer(this.mockedReader);

            comparer.AddItem(aDllPath);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Readings.Count, Is.EqualTo(1));
                Assert.That(comparer.ReadingsBySignature.Count, Is.EqualTo(1));
                Assert.That(comparer.FailedReadings.Count, Is.Zero);
                Assert.That(comparer.ReadingsMatched, Is.False);
            });
        }

        [Test]
        public void AddItem__GivenSingleInvalidItem__ComparerContains_OneReading_NoSignatureGroups_OneFailedReading_AndReadingMatchedReturnsFalse()
        {
            var comparer = new DebugSignaturesComparer(this.mockedReader);

            comparer.AddItem(invalidPath);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Readings.Count, Is.EqualTo(1));
                Assert.That(comparer.ReadingsBySignature.Count, Is.Zero);
                Assert.That(comparer.FailedReadings.Count, Is.EqualTo(1));
                Assert.That(comparer.ReadingsMatched, Is.False);
            });
        }

        [Test]
        public void AddItem__GivenTwoValidMatchingItems__ComparerContains_TwoReadings_OneSignatureGroup_NoFailedReadings_AndReadingMatchedReturnsTrue()
        {
            var comparer = new DebugSignaturesComparer(this.mockedReader);

            comparer.AddItem(aDllPath);
            comparer.AddItem(aPdbPath);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Readings.Count, Is.EqualTo(2));
                Assert.That(comparer.ReadingsBySignature.Count, Is.EqualTo(1));
                Assert.That(comparer.FailedReadings.Count, Is.Zero);
                Assert.That(comparer.ReadingsMatched, Is.True);
            });
        }

        [Test]
        public void AddItem__GivenTwoValidMismatchingItems__ComparerContains_TwoReadings_TwoSignatureGroups_NoFailedReadings_AndReadingMatchedReturnsFalse()
        {
            var comparer = new DebugSignaturesComparer(this.mockedReader);

            comparer.AddItem(bDllPath);
            comparer.AddItem(aPdbPath);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Readings.Count, Is.EqualTo(2));
                Assert.That(comparer.ReadingsBySignature.Count, Is.EqualTo(2));
                Assert.That(comparer.FailedReadings.Count, Is.Zero);
                Assert.That(comparer.ReadingsMatched, Is.False);
            });
        }

        [Test]
        public void AddItem__GivenSingleValidItemTwice__ComparerContains_OneReading_OneSignatureGroup_NoFailedReadings_AndReadingMatchedReturnsFalse()
        {
            var comparer = new DebugSignaturesComparer(this.mockedReader);

            comparer.AddItem(aPdbPath);
            comparer.AddItem(aPdbPath);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Readings.Count, Is.EqualTo(1));
                Assert.That(comparer.ReadingsBySignature.Count, Is.EqualTo(1));
                Assert.That(comparer.FailedReadings.Count, Is.Zero);
                Assert.That(comparer.ReadingsMatched, Is.False);
            });
        }

        [Test]
        public void AddItem__GivenSingleValidAndSingleInvalidItem__ComparerContains_TwoReadings_OneSignatureGroups_OneFailedReading_AndReadingMatchedReturnsFalse()
        {
            var comparer = new DebugSignaturesComparer(this.mockedReader);

            comparer.AddItem(aDllPath);
            comparer.AddItem(invalidPath);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Readings.Count, Is.EqualTo(2));
                Assert.That(comparer.ReadingsBySignature.Count, Is.EqualTo(1));
                Assert.That(comparer.FailedReadings.Count, Is.EqualTo(1));
                Assert.That(comparer.ReadingsMatched, Is.False);
            });
        }

        [Test]
        public void AddItem__GivenTwoValidMatchingFilesAndOneInvalid__ComparerContains_ThreeReadings_OneSignatureGroup_OneFailedReading_AndReadingMatchedReturnsTrue()
        {
            var comparer = new DebugSignaturesComparer(this.mockedReader);

            comparer.AddItem(aDllPath);
            comparer.AddItem(aPdbPath);
            comparer.AddItem(invalidPath);

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Readings.Count, Is.EqualTo(3));
                Assert.That(comparer.ReadingsBySignature.Count, Is.EqualTo(1));
                Assert.That(comparer.FailedReadings.Count, Is.EqualTo(1));
                Assert.That(comparer.ReadingsMatched, Is.True);
            });
        }

        [Test]
        public void AreMatching_GivenTwoMatchingItems_ReturnsTrue()
        {
            var result = DebugSignaturesComparer.AreMatching(new[] { aDllPath, aPdbPath }, this.mockedReader);
            Assert.That(result, Is.True);
        }

        [Test]
        public void AreMatching_GivenTwoMismatchingItems_ReturnsFalse()
        {
            var result = DebugSignaturesComparer.AreMatching(new[] { aDllPath, cPdbPath }, this.mockedReader);
            Assert.That(result, Is.False);
        }

        // TODO: Write tests for AddItems
    }
}
