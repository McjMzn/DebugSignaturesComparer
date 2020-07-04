using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Vrasoft.DebugSignatures;

namespace Vrasoft.DebugSignatures.ComparerGui
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private DebugSignaturesComparer comparer;
        private string log;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DebugSignatureReading> Readings => comparer.Readings as ObservableCollection<DebugSignatureReading>;
        public Dictionary<string, List<DebugSignatureReading>> ReadingsBySignature =>
            this.Readings
                .GroupBy(reading => reading.DebugSignature)
                .OrderByDescending(readings => readings.Count())
                .ToDictionary(group => group.Key, group => group.ToList());

        public string Log
        {
            get => log;
            set
            {
                log = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Log)));
            }
        }

        public MainWindowViewModel()
        {
            comparer = new DebugSignaturesComparer();
            comparer.Readings = new ObservableCollection<DebugSignatureReading>();
            Readings.CollectionChanged += (sender, args) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReadingsBySignature)));
            comparer.ProcessingError += (sender, message) => Log += $"{message}{Environment.NewLine}";
        }

        public void AddFiles(IEnumerable<string> files)
        {
            comparer.AddFiles(files);
        }
    }
}
