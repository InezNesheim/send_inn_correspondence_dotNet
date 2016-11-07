using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;

namespace SendCorrespondenceService
{
    public partial class SendInCorrespondenceService : ServiceBase
    {

        private FileProcesser fp;

        public SendInCorrespondenceService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            fp = new FileProcesser(ConfigurationManager.AppSettings["FromPath"]);
            fp.Watch();
        }

        protected override void OnStop()
        {
        }
    }
}
