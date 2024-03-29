﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WebArchiveViewer
{
    public class StatusCodeColorConverter : IValueConverter
    {
        private string UndefinedColor { get; set; }
        private IEnumerable<StatusCodeColor> CodeColors { get; set; }

        public StatusCodeColorConverter()
        {
            UndefinedColor = "black";
            CodeColors = new StatusCodeColor[]
            {
                new StatusCodeColor(200, "#69ab3c"),
                new StatusCodeColor(200, 400, "#ab873c"),
                new StatusCodeColor(400, 1000, "#ab3c3c")
            };
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse($"{value}", out int code))
            {
                StatusCodeColor matchStatus = CodeColors.FirstOrDefault(c => code > c.From && code <= c.To);
                if (matchStatus != null)
                {
                    return matchStatus.Color;
                }
            }
            return UndefinedColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
    class StatusCodeColor
    {
        public readonly int From;
        public readonly int To;
        public readonly string Color;

        public StatusCodeColor(int value, string color) : this(value, value, color)
        {

        }
        public StatusCodeColor(int from, int to, string color)
        {
            From = from;
            To = to;
            Color = color;
        }
    }

}
