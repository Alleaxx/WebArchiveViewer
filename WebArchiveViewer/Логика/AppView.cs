using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public class AppView
    {
        public static AppView Ex
        {
            get
            {
                if(ex == null)
                    ex = new AppView();
                return ex;
            }
        }
        private static AppView ex;

        public ArchiveView Archive { get; private set; }

        private AppView()
        {
            Archive = new ArchiveView();
        }
    }}
