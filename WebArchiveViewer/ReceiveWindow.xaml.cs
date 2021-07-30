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
using System.Windows.Shapes;

namespace WebArchiveViewer
{
    /// <summary>
    /// Логика взаимодействия для LoadWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        public LoadWindow()
        {
            InitializeComponent();
            DataContext = AppView.Ex.Receiving;
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
