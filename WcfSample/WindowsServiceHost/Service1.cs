using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using WcfService;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceHost
{
    public partial class LR : ServiceBase
    {
        public LR()
        {
            InitializeComponent();
        }


        private ServiceHost _host = new ServiceHost(typeof(MyService));

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
