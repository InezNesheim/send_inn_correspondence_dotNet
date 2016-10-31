using System;
using SendInCorrespondence.ICorrespondenceAgencyExternalBasic;
using System.Configuration;

namespace SendInCorrespondence
{
    public class SendInCorrespondenceDal
    {
        public static void InsertCorrespondence(string systemUserName, string systemPassword, string systemUserCode,
            string externalShipmentReferece, InsertCorrespondenceV2 correspondence)
        {
            ReceiptExternal receipt;

            try
            {
                using (CorrespondenceAgencyExternalBasicClient client = new CorrespondenceAgencyExternalBasicClient())
                {
                    receipt = client.InsertCorrespondenceBasicV2(systemUserName, systemPassword, systemUserCode, externalShipmentReferece, correspondence);
                    if (receipt.ReceiptStatusCode == ReceiptStatusEnum.OK)
                        return;

                    throw new Exception($"Error while sending correspondence: {receipt.ReceiptStatusCode}, {receipt.ReceiptText}");
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static InsertCorrespondenceV2  CreateCorrespondence()
        {
            InsertCorrespondenceV2 correspondence = new InsertCorrespondenceV2();
            correspondence.ServiceCode = ConfigurationManager.AppSettings["serviceCode"];
            correspondence.ServiceEdition = ConfigurationManager.AppSettings["serviceEdition"];
            correspondence.Reportee = ConfigurationManager.AppSettings["orgNo"];
            correspondence.VisibleDateTime = DateTime.Parse(ConfigurationManager.AppSettings["visibleDateTime"]);
            correspondence.AllowSystemDeleteDateTime = DateTime.Parse(ConfigurationManager.AppSettings["allowSystemDeleteDateTime"]);
            correspondence.DueDateTime = DateTime.Parse(ConfigurationManager.AppSettings["dueDateTime"]);            

            correspondence.Content = new ExternalContentV2
                {
                    LanguageCode = ConfigurationManager.AppSettings["languageCode"],
                    MessageTitle = "Title",
                    MessageSummary = "Summary text",
                    MessageBody = "Body text",
                }
            ;

            correspondence.Notifications = new NotificationBEList
            {
                new Notification1
                {
                    FromAddress = "no-reply@altinn.no",
                    ShipmentDateTime = DateTime.Parse(ConfigurationManager.AppSettings["shipmentDateTime"]),
                    LanguageCode = ConfigurationManager.AppSettings["languageCode"],
                    NotificationType = ConfigurationManager.AppSettings["notificationTemplate"],
                    ReceiverEndPoints = new ReceiverEndPointBEList
                    {
                        new ReceiverEndPoint
                        {
                            TransportType = TransportType.Email,
                            ReceiverAddress = ConfigurationManager.AppSettings["epost"],
                        },
                        new ReceiverEndPoint
                        {
                            TransportType = TransportType.SMS,
                            ReceiverAddress = ConfigurationManager.AppSettings["mobilePhone"],
                        }
                    }
                }
            };            

            return correspondence;
        }        
    }
}
