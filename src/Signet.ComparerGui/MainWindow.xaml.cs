using System.Windows;
using System.Windows.Controls;

namespace Signet.ComparerGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void FilesDropped(object sender, DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            (DataContext as MainWindowViewModel).AddFiles(paths);
        }

        private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }
    }
}
