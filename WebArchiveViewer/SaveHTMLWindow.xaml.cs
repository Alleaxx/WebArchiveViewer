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
    public partial class SaveHTMLWindow : Window
    {
        private LinksLoaderView View { get; set; }

        public SaveHTMLWindow(LinksLoaderView view)
        {
            InitializeComponent();
            View = view;
            DataContext = View;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.StopProgressCommand?.Execute(null);
        }
    }
}
