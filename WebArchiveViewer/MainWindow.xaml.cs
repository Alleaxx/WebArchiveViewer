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

namespace WebArchiveViewer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = AppView.Ex.Archive;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void EntryInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement elem = sender as FrameworkElement;
            System.Diagnostics.Process.Start(elem.Tag.ToString());
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            AppView.Ex.Archive.UpdateList();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadWindow window = new LoadWindow();
            window.ShowDialog();
        }

        private void SaveHTML_Click(object sender, RoutedEventArgs e)
        {
            SaveHTMLWindow w = new SaveHTMLWindow();
            w.Show();
        }
    }
}
