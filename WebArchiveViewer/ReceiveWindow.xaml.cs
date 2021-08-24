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
    public partial class LoadWindow : Window
    {
        public LoadWindow(ArchiveSnapLoader loader)
        {
            InitializeComponent();
            DataContext = loader;
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
