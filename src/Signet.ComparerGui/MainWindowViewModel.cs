using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Signet.ComparerGui
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static readonly Version signetVersion = typeof(MainWindowViewModel).Assembly.GetName().Version;
        private DebugSignaturesComparer comparer;

        public event PropertyChangedEventHandler PropertyChanged;

        # region Window title

        public string SignetVersion => signetVersion.ToString();
        public string Title { get; set; } = $"SigNET {signetVersion} - Debug Signatures Comparer GUI";

        #endregion

        public List<DebugSignatureReading> Readings => comparer.Readings;
        public List<DebugSignatureReading> FailedReadings => comparer.FailedReadings;
        public Dictionary<string, List<DebugSignatureReading>> ReadingsBySignature => comparer.ReadingsBySignature;
        public bool ErrorsOcurred => this.FailedReadings.Count > 0;
        
        private bool showErrors = false;
        public bool ShowErrors
        {
            get => this.showErrors;
            set
            {
                this.showErrors = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ShowErrors)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ErrorsVisibility)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ReadingsVisibility)));
            }
        }

        public Visibility ErrorsVisibility => this.ShowErrors ? Visibility.Visible : Visibility.Hidden;
        public Visibility ReadingsVisibility => this.ShowErrors ? Visibility.Hidden: Visibility.Visible;
        public Visibility PromptVisibility => this.Readings.Count == 0 ? Visibility.Visible : Visibility.Hidden;

        public GenericCommand ShowErrorsCommand { get; set; }
        public GenericCommand ShowReadingsCommand { get; set; }
        public GenericCommand OpenGitHubCommand { get; set; }
        public GenericCommand ClearReadingsCommand { get; set; }
        public GenericCommand CloseApplicationCommand { get; set; }
        public GenericCommand MinimizeWindowCommand { get; set; }

        public MainWindowViewModel()
        {
            this.comparer = new DebugSignaturesComparer();
            this.ShowErrorsCommand = new GenericCommand(_ => this.ShowErrors = true, _ => this.FailedReadings.Count > 0);
            this.ShowReadingsCommand = new GenericCommand(_ => this.ShowErrors = false, _ => true);
            this.OpenGitHubCommand = new GenericCommand(_ => Process.Start(new ProcessStartInfo { FileName = "https://github.com/McjMzn/DebugSignaturesComparer", UseShellExecute = true }), _ => true);
            this.ClearReadingsCommand = new GenericCommand(_ => this.ClearReadings(), _ => this.Readings.Count > 0);
            this.CloseApplicationCommand = new GenericCommand(_ => Application.Current.Shutdown(), _ => true);
            this.MinimizeWindowCommand = new GenericCommand(_ => Application.Current.MainWindow.WindowState = WindowState.Minimized, _ => true);
        }

        public void AddItems(IEnumerable<string> files)
        {
            this.comparer.AddItems(files);
            this.Update();
        }

        public void ClearReadings()
        {
            this.comparer.ClearReadings();
            this.ShowErrors = false;
            this.Update();
        }

        private void Update()
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.PromptVisibility)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Readings)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FailedReadings)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ErrorsOcurred)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.ReadingsBySignature)));
            this.ShowErrorsCommand.InvokeCanExecuteChanged();
            this.ClearReadingsCommand.InvokeCanExecuteChanged();
        }
    }
}
