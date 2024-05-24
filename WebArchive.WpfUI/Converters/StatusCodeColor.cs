using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer.WpfUI.Converters
{
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
