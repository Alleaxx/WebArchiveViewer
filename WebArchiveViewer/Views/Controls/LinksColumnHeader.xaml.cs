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

namespace WebArchiveViewer.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для LinksColumnHeader.xaml
    /// </summary>
    public partial class LinksColumnHeader : UserControl
    {
        public ListViewColumn ColumnInfo
        {
            get => (ListViewColumn)GetValue(ColumnInfoProperty);
            set => SetValue(ColumnInfoProperty, value);
        }
        public static readonly DependencyProperty ColumnInfoProperty =
            DependencyProperty.Register(nameof(ColumnInfo), typeof(ListViewColumn), typeof(LinksColumnHeader));


        public LinksColumnHeader()
        {
            InitializeComponent();

            //Чтобы ссылки на имена элементов работали в контекстном меню
            NameScope.SetNameScope(contextMenu, NameScope.GetNameScope(this));
        }
    }
}
