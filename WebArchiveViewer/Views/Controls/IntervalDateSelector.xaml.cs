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
    public partial class IntervalDateSelector : UserControl
    {
        public static DependencyProperty MinDateProperty;
        public static DependencyProperty MaxDateProperty;
        public static DependencyProperty FromDateProperty;
        public static DependencyProperty ToDateProperty;

        
        static IntervalDateSelector()
        {
            // Регистрация свойств зависимости
            MinDateProperty = DependencyProperty.Register("MinDate", typeof(DateTime), typeof(IntervalDateSelector), 
                new FrameworkPropertyMetadata(new DateTime(2012, 1, 1),new PropertyChangedCallback(Smth_Changed)));
            MaxDateProperty = DependencyProperty.Register("MaxDate", typeof(DateTime), typeof(IntervalDateSelector),
                new FrameworkPropertyMetadata(new DateTime(2013, 1, 1),new PropertyChangedCallback(Smth_Changed)));
            FromDateProperty = DependencyProperty.Register("FromDate", typeof(DateTime), typeof(IntervalDateSelector),
                new FrameworkPropertyMetadata(new DateTime(2012, 3, 1),new PropertyChangedCallback(Smth_Changed))); 
            ToDateProperty = DependencyProperty.Register("ToDate", typeof(DateTime), typeof(IntervalDateSelector),
                 new FrameworkPropertyMetadata(new DateTime(2012, 9, 1),new PropertyChangedCallback(Smth_Changed)));        
        }
        public IntervalDateSelector()
        {
            InitializeComponent();
            SizeChanged += IntervalSelector_Loaded;
        }
        private void IntervalSelector_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private static void Smth_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            IntervalDateSelector interval = (IntervalDateSelector)sender;
            interval.Update();

        }

        private void Update()
        {
            ProgressNow.Margin = ProgressMargin;
            ProgressNow.Width = ProgressWidth;
        }
        
        public DateTime MinDate
        {
              get { return (DateTime)GetValue(MinDateProperty); }
              set { SetValue(MinDateProperty, value); }
        }
        public DateTime MaxDate
        {
              get { return (DateTime)GetValue(MaxDateProperty); }
              set { SetValue(MaxDateProperty, value); }
        }
        public DateTime ToDate
        {
              get { return (DateTime)GetValue(ToDateProperty); }
              set { SetValue(ToDateProperty, value); }
        }
        public DateTime FromDate
        {
              get { return (DateTime)GetValue(FromDateProperty); }
              set { SetValue(FromDateProperty, value); }
        }

        private double Minimum => 0;
        private double Maximum => (MaxDate - MinDate).TotalHours;
        private double To
        {
            get => (ToDate - MinDate).TotalHours;
            set
            {
                ToDate = MinDate.AddHours(value);
            }
        }
        private double From
        {
            get => (FromDate - MinDate).TotalHours;
            set
            {
                FromDate = MinDate.AddHours(value);
            }
        }

        private Thickness ProgressMargin =>  Maximum != 0 ? new Thickness(From / Maximum * ProgressFull.ActualWidth, 0, 0, 0) : new Thickness(0, 0, 0, 0);
        private double ProgressWidth
        {
            get
            {
                double width = Maximum != 0 ? (Math.Abs(To - From)) / Maximum * ProgressFull.ActualWidth : 0;
                if (width < 0)
                    return 0;
                return width;
            }
        }



        private void ProgressFull_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var test = e.GetPosition(ProgressFull);
            double prs = test.X / ProgressFull.ActualWidth;
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
            ToDate.AddDays(-1);
        }
        private void RepeatButton_Click_1(object sender, RoutedEventArgs e)
        {
            ToDate.AddDays(1);
        }
    }
}
