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
    public partial class RulesWindow : Window
    {
        public RulesView View { get; set; }
        public RulesWindow(RulesView view)
        {
            InitializeComponent();
            DataContext = view;
            View = view;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            View.Owner.UpdateCategories(null);
        }
    }
}
