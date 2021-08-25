using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveViewer
{
    public class AppView
    {
        private static readonly AppView ex = new AppView();
        public static AppView Ex => ex;


        public ArchiveView Archive { get; private set; }

        private AppView()
        {
            Archive = new ArchiveView();
        }
    }}
