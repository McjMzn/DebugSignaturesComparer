using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Signet
{
    /// <summary>
    /// Class wrapping the <see cref="IDebugSignaturesReader"/> that can be used to comparing the debug signatures of given files.
    /// </summary>
    public class DebugSignaturesComparer
    {
        private IDebugSignaturesReader reader;

        /// <summary>
        /// Initializes a new instance of <see cref="DebugSignaturesComparer"/>.
        /// </summary>
        public DebugSignaturesComparer()
            : this(new DebugSignaturesReader())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DebugSignaturesComparer"/>.
        /// </summary>
        /// <param name="reader">Debug signatures reader.</param>
        public DebugSignaturesComparer(IDebugSignaturesReader reader)
        {
            this.Readings = new List<DebugSignatureReading>();
            this.reader = reader;
        }

        /// <summary>
        /// Gets the list of readings based on files added to this comparer.
        /// </summary>
        public List<DebugSignatureReading> Readings { get; }
        
        /// <summary>
        /// Gets the list of failed readings.
        /// </summary>
        public List<DebugSignatureReading> FailedReadings =>
            this.Readings
                .Where(r => !r.IsSuccessful)
            .ToList();

        /// <summary>
        /// Gets the dictionary with debug signatures as keys and the lists of readings with the signature matching the key as value.
        /// </summary>
        public Dictionary<string, List<DebugSignatureReading>> ReadingsBySignature =>
            this.Readings
                .Where(r => r.IsSuccessful)
                .GroupBy(r => r.DebugSignature)
                .OrderByDescending(readings => readings.Count())
                .ToDictionary(group => group.Key, group => group.ToList());

        /// <summary>
        /// Gets the bool value telling if all successful reading share the same debug signature.
        /// </summary>
        public bool ReadingsMatched =>
            this.Readings.Count(r => r.IsSuccessful) > 1 &&
            this.Readings.Where(r => r.IsSuccessful).GroupBy(r => r.DebugSignature).Count() == 1;

        /// <summary>
        /// Checks if items under the given paths have matching debug signatures.
        /// </summary>
        /// <param name="paths">Paths to check.</param>
        /// <returns>True if are matching, false otherwise.</returns>
        public static bool AreMatching(IEnumerable<string> items)
        {
            return AreMatching(items, new DebugSignaturesReader());
        }

        /// <summary>
        /// Checks if elements under the given paths have matching debug signatures.
        /// </summary>
        /// <param name="paths">Paths to check.</param>
        /// <param name="reader">Debug signature reader to use.</param>
        /// <returns>True if are matching, false otherwise.</returns>
        public static bool AreMatching(IEnumerable<string> items, IDebugSignaturesReader reader)
        {
            var comparer = new DebugSignaturesComparer(reader);
            comparer.AddItems(items);

            return comparer.ReadingsBySignature.Keys.Count == 1;
        }

        /// <summary>
        /// Add a file or a directory to the comparer.
        /// </summary>
        /// <param name="item">The item.</param>
        public void AddItem(string item)
        {
            var readings = this.reader.Read(item);
            foreach(var reading in readings)
            {
                var conflictingReading = this.Readings.FirstOrDefault(r => r.File == reading.File);
                if (conflictingReading != null)
                {
                    this.Readings.Remove(conflictingReading);
                }

                this.Readings.Add(reading);
            }
        }

        /// <summary>
        /// Add a list of file or directories to the comparer.
        /// </summary>
        /// <param name="item">The item.</param>
        public void AddItems(IEnumerable<string> items)
        {
            items.ToList().ForEach(this.AddItem);
        }

        /// <summary>
        /// Clears the readings list.
        /// </summary>
        public void ClearReadings()
        {
            this.Readings.Clear();
        }
    }
}
