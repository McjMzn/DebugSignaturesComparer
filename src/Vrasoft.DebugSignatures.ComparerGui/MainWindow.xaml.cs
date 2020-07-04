using System.Windows;
using System.Windows.Controls;
using Vrasoft.DebugSignatures.ComparerGui;

namespace Vrasoft.DebugSignatures.ComparerGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        private void FilesDropped(object sender, DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            (this.DataContext as MainWindowViewModel).AddFiles(paths);
        }

        private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }
    }
}
