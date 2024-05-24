using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebArchiveViewer.ViewModels;

namespace WebArchiveViewer.Views.Windows
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel ArchiveContext;
        public MainWindow()
        {
            InitializeComponent();
            ArchiveContext = new MainWindowViewModel();
            DataContext = ArchiveContext;
        }

        private async void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            await ArchiveContext.UpdatePagerLinks();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var closeCommand = ArchiveContext.CloseSnapshotCommand;
            if (closeCommand.CanExecute(null))
            {
                closeCommand?.Execute(null);
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new AboutWindow();
            window.ShowDialog();
        }
    }
}
