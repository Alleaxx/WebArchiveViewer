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
using System.Windows.Shapes;

namespace WebArchiveViewer
{
    public partial class RulesWindow : Window
    {
        public RulesWindow(RulesView view)
        {
            InitializeComponent();
            DataContext = view;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var snap = AppView.Ex.Archive.SnapshotView;
            snap.UpdateCategories(null);
            //ICommand command = snap.UpdateCategoriesCommand;

            //if(command.CanExecute(null))
            //    command.Execute(null);
        }
    }
}
