using System;
using SendInCorrespondence;
using System.Configuration;
using SendInCorrespondence.ICorrespondenceAgencyExternalBasic;


namespace SendInCorrespondenceConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            InsertCorrespondenceV2 correspondence = SendInCorrespondenceDal.CreateCorrespondence();
            SendInCorrespondenceDal.InsertCorrespondence(
                ConfigurationManager.AppSettings["systemUserName"],
                ConfigurationManager.AppSettings["systemPassword"],
                ConfigurationManager.AppSettings["systemUserCode"],
                Guid.NewGuid().ToString(),
                correspondence);               
        }        
    }
}
