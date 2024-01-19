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
using WebArchive.Data;
using WebArchiveViewer.UI;

namespace WebArchiveViewer.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для ListLinks.xaml
    /// </summary>
    public partial class ListLinks : UserControl
    {
        public IPager<ArchiveLink> LinksPager
        {
            get => (IPager<ArchiveLink>)GetValue(LinksPagerProperty);
            set => SetValue(LinksPagerProperty, value);
        }
        public static readonly DependencyProperty LinksPagerProperty =
            DependencyProperty.Register(nameof(LinksPager), typeof(IPager<ArchiveLink>), typeof(ListLinks));


        public ListLinks()
        {
            InitializeComponent();
        }
    }
}
