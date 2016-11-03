using System;
using System.Configuration;
using SendCorrespondenceService.ICorrespondenceAgencyExternalBasic;


namespace SendCorrespondenceService
{
    /// <summary>
    /// Dal class for initializing and sending in a correspondence via Altinn webservice.
    /// </summary>
    public class SendCorrespondenceDal
    {
        /// <summary>
        /// Initializes and sends a correspondence
        /// </summary>
        public static void CreateAndSendCorrespondence(string archiveReference)
        {
           InsertCorrespondenceV2 correspondence;

            try
            {
                correspondence = CreateCorrespondence(archiveReference);
            }
            catch (Exception)
            {
                //TODO:: log error - failed to initialize correspondence
                throw;
            }

            SendCorrespondence(ConfigurationManager.AppSettings["systemUserName"], ConfigurationManager.AppSettings["systemPassword"], ConfigurationManager.AppSettings["systemUserCode"], Guid.NewGuid().ToString(), correspondence);
        }

        /// <summary>
        /// Uses the ICorrespondenceAgencyExternalBasic web service for sending a correspondence
        /// </summary>
        /// <param name="systemUserName">User name for the system</param>
        /// <param name="systemPassword">System password</param>
        /// <param name="systemUserCode">User code for the system</param>
        /// <param name="externalShipmentReferece">Unique value to identify the shipment</param>
        /// <param name="correspondence">Correspondence to send</param>
        public static void SendCorrespondence(string systemUserName, string systemPassword, string systemUserCode, string externalShipmentReferece, InsertCorrespondenceV2 correspondence)
        {
            ReceiptExternal receipt;

            try
            {
                using (CorrespondenceAgencyExternalBasicClient client = new CorrespondenceAgencyExternalBasicClient())
                {
                    receipt = client.InsertCorrespondenceBasicV2(systemUserName, systemPassword, systemUserCode, externalShipmentReferece, correspondence);
                    if (receipt.ReceiptStatusCode == ReceiptStatusEnum.OK)
                    {
                        //TODO:: log success and add business logic here
                    }
                    else
                    {
                        //TODO:: log error and add business logic here
                        throw new Exception($"Error while sending correspondence: {receipt.ReceiptStatusCode.ToString()}, {receipt.ReceiptText}");
                    }
                }
            }
            catch (Exception)
            {
                //TODO:: log error and add business logic here
                throw;
            }
        }

        /// <summary>
        /// Initializes a new Correspondence with example values. Values are defined in SendInCorrespondence.config file.      
        /// </summary>
        /// <returns></returns>
        public static InsertCorrespondenceV2 CreateCorrespondence(string archiveReference)
        {
            InsertCorrespondenceV2 correspondence = new InsertCorrespondenceV2();
            correspondence.ServiceCode = ConfigurationManager.AppSettings["serviceCode"];
            correspondence.ServiceEdition = ConfigurationManager.AppSettings["serviceEdition"];
            correspondence.Reportee = ConfigurationManager.AppSettings["orgNo"];
            correspondence.VisibleDateTime = DateTime.Parse(ConfigurationManager.AppSettings["visibleDateTime"]);
            correspondence.AllowSystemDeleteDateTime = DateTime.Parse(ConfigurationManager.AppSettings["allowSystemDeleteDateTime"]);
            correspondence.DueDateTime = DateTime.Parse(ConfigurationManager.AppSettings["dueDateTime"]);
            correspondence.ArchiveReference = archiveReference;

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
                    FromAddress = ConfigurationManager.AppSettings["fromAddress"],
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

