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
    /// Логика взаимодействия для SaveHTMLWindow.xaml
    /// </summary>
    public partial class SaveHTMLWindow : Window
    {
        public SaveHTMLWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveHTMLView view = DataContext as SaveHTMLView;
            view.StopProgressCommand?.Execute(null);
        }
    }
}
