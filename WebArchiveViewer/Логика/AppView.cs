using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    class AppView
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

        public ArchiveReceiveView Receiving { get; private set; }
        public ArchiveView Archive { get; private set; }

        private AppView()
        {
            Archive = new ArchiveView();
            Receiving = new ArchiveReceiveView();
        }
    }}
