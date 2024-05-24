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

using WebArchiveViewer.ViewModels;

namespace WebArchiveViewer.Views.Controls
{
    public partial class IntervalSliderLinks : UserControl
    {
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(IntervalSliderLinks), new UIPropertyMetadata(0d));

        public double LowerValue
        {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }
        }
        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register("LowerValue", typeof(double), typeof(IntervalSliderLinks), new UIPropertyMetadata(0d));

        public double UpperValue
        {
            get { return (double)GetValue(UpperValueProperty); }
            set { SetValue(UpperValueProperty, value); }
        }
        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register("UpperValue", typeof(double), typeof(IntervalSliderLinks), new UIPropertyMetadata(0d));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(IntervalSliderLinks), new UIPropertyMetadata(1d));

        public SnapshotView LinksSource
        {
            get { return (SnapshotView)GetValue(LinksSourceProperty); }
            set {
                var old = LinksSource;
                SetValue(LinksSourceProperty, value);
                OnPropertyChanged(new DependencyPropertyChangedEventArgs(LinksSourceProperty, value, old));
            }
        }
        public static readonly DependencyProperty LinksSourceProperty =
            DependencyProperty.Register(nameof(LinksSource), typeof(SnapshotView), typeof(IntervalSliderLinks));
        
        public SnapshotDateStatistics Statistics
        {
            get => (SnapshotDateStatistics)GetValue(StatisticsProperty);
            set => SetValue(StatisticsProperty, value);
        }
        public static readonly DependencyProperty StatisticsProperty =
            DependencyProperty.Register(nameof(Statistics), typeof(SnapshotDateStatistics), typeof(IntervalSliderLinks));


        public IntervalSliderLinks()
        {
            InitializeComponent();
            Loaded += IntervalSlider_Loaded;
        }

        void IntervalSlider_Loaded(object sender, RoutedEventArgs e)
        {
            LowerSlider.ValueChanged += LowerSlider_ValueChanged;
            UpperSlider.ValueChanged += UpperSlider_ValueChanged;
        }

        private void LowerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpperSlider.Value = Math.Max(UpperSlider.Value, LowerSlider.Value);
        }
        private void UpperSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LowerSlider.Value = Math.Min(UpperSlider.Value, LowerSlider.Value);
        }


    }



    //Ширина = суммарное количество дней / количество доступных пикселей по ширине
    //Высота = количество ссылок на дату / максимальное количество ссылок на дату * количество доступных пикселей по высоте
    
}
