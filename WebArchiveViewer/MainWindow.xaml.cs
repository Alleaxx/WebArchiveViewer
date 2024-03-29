﻿using System;
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
    public partial class MainWindow : Window
    {
        private readonly ArchiveView ArchiveContext;
        public MainWindow()
        {
            InitializeComponent();
            ArchiveContext = new ArchiveView();
            DataContext = ArchiveContext;
        }

        private void EntryInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var elem = sender as FrameworkElement;
            System.Diagnostics.Process.Start(elem.Tag.ToString());
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ArchiveContext.UpdatePagerLinks();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var closeCommand = ArchiveContext.CloseSnapCommand;
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
