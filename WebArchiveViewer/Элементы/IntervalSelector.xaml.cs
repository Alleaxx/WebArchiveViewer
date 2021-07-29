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
    /// Логика взаимодействия для IntervalSelector.xaml
    /// </summary>
    public partial class IntervalSelector : UserControl
    {
        public static DependencyProperty MinProperty;
        public static DependencyProperty MaxProperty;
        public static DependencyProperty FromProperty;
        public static DependencyProperty ToProperty;

        
        public static DependencyProperty InfoHeaderProperty;
        public static DependencyProperty ShowInfoProperty;
        
        public static DependencyProperty ColorInactiveProperty;
        public static DependencyProperty ColorActiveProperty;
        public static DependencyProperty ColorActiveBorderProperty;

        static IntervalSelector()
        {
            // Регистрация свойств зависимости
            MinProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(IntervalSelector), 
                new FrameworkPropertyMetadata(new PropertyChangedCallback(Smth_Changed)));
            MaxProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(IntervalSelector),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(Smth_Changed)));
            FromProperty = DependencyProperty.Register("From", typeof(double), typeof(IntervalSelector),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(Smth_Changed))); 
            ToProperty = DependencyProperty.Register("To", typeof(double), typeof(IntervalSelector),
                 new FrameworkPropertyMetadata(new PropertyChangedCallback(Smth_Changed)));
            
            ShowInfoProperty = DependencyProperty.Register("ShowInfo", typeof(bool), typeof(IntervalSelector));
            InfoHeaderProperty = DependencyProperty.Register("InfoHeader", typeof(string), typeof(IntervalSelector));

            
            ColorInactiveProperty = DependencyProperty.Register("ColorInactive", typeof(string), typeof(IntervalSelector),
                 new FrameworkPropertyMetadata("WhiteSmoke",new PropertyChangedCallback(Color_Changed)));
            ColorActiveProperty = DependencyProperty.Register("ColorActive", typeof(string), typeof(IntervalSelector),
                 new FrameworkPropertyMetadata("LightGreen",new PropertyChangedCallback(Color_Changed)));
            ColorActiveBorderProperty = DependencyProperty.Register("ColorActiveBorder", typeof(string), typeof(IntervalSelector),
                 new FrameworkPropertyMetadata("Green",new PropertyChangedCallback(Color_Changed)));
        }
        public IntervalSelector()
        {
            InitializeComponent();
            Minimum = 0;
            From = 25;
            To = 75;
            Maximum = 100;
            SizeChanged += IntervalSelector_Loaded;
        }

        private void IntervalSelector_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private static void Smth_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            IntervalSelector interval = (IntervalSelector)sender;
            interval.Update();

        }
        private static void Color_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            IntervalSelector interval = (IntervalSelector)sender;
            //interval.ColorActive = (string)e.NewValue;
        }

        private void Update()
        {
            ProgressNow.Margin = ProgressMargin;
            ProgressNow.Width = ProgressWidth;
        }

        public double Minimum
        {
              get { return (double)GetValue(MinProperty); }
              set { SetValue(MinProperty, value); }
        }
        public double Maximum
        {
              get { return (double)GetValue(MaxProperty); }
              set { SetValue(MaxProperty, value); }
        }
        public double To
        {
              get { return (double)GetValue(ToProperty); }
              set { SetValue(ToProperty, value); }
        }
        public double From
        {
              get { return (double)GetValue(FromProperty); }
              set { SetValue(FromProperty, value); }
        }
        public bool ShowInfo
        {
              get { return (bool)GetValue(ShowInfoProperty); }
              set { SetValue(ShowInfoProperty, value); }
        }
        public string InfoHeader
        {
              get { return (string)GetValue(InfoHeaderProperty); }
              set { SetValue(InfoHeaderProperty, value); }
        }

        public string ColorActive
        {
              get { return (string)GetValue(ColorActiveProperty); }
              set { SetValue(ColorActiveProperty, value); }
        }
        public string ColorInactive
        {
              get { return (string)GetValue(ColorInactiveProperty); }
              set { SetValue(ColorInactiveProperty, value); }
        }
        public string ColorActiveBorder
        {
              get { return (string)GetValue(ColorActiveBorderProperty); }
              set { SetValue(ColorActiveBorderProperty, value); }
        }

        public Thickness ProgressMargin =>  Maximum != 0 ? new Thickness(From / Maximum * Progress.ActualWidth,0,0,0) : new Thickness(0,0,0,0);
        public double ProgressWidth
        {
            get
            {
                double width = Maximum != 0 ? (Math.Abs(To - From)) / Maximum * Progress.ActualWidth : 0;
                if (width < 0)
                    return 0;
                return width;
            }
        }


        private void ProgressFull_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var test = e.GetPosition(Progress);
            double prs = test.X / Progress.ActualWidth;
            double val = prs * Maximum;
            if (val > To)
                To = val;
            else if (val < To && val > From)
            {
                if (e.ChangedButton == MouseButton.Left)
                    To = val;
                else
                    From = val;
            }
            else if (val < From)
                From = val;
        }
        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            From--;
        }
        private void RepeatButton_Click_1(object sender, RoutedEventArgs e)
        {
            To++;
        }
    }
}
