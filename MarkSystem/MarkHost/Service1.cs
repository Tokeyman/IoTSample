using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using MarkWcf;
using System.ServiceModel;

namespace MarkHost
{
    public partial class MarHost : ServiceBase
    {
        public MarHost()
        {
            InitializeComponent();
        }

        private ServiceHost _host = new ServiceHost(typeof(MarkService));
        protected override void OnStart(string[] args)
        {
            _host.Open();
        }

        protected override void OnStop()
        {
            _host.Close();
        }
    }
}
