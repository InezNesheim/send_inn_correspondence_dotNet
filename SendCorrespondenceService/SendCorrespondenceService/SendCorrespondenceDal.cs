using System;
using System.Configuration;
using System.Diagnostics;
using SendCorrespondenceService.ICorrespondenceAgencyExternalBasic;
using SendCorrespondenceService.Utils;

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
        public static void CreateAndSendCorrespondence(string archiveReference, string reportee)
        {
           InsertCorrespondenceV2 correspondence;

            try
            {
                correspondence = CreateCorrespondence(archiveReference, reportee);
            }
            catch (Exception exception)
            {                
                Logger.Log($"Failed to initialize correspondence {exception.Message}");
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
            try
            {
                using (CorrespondenceAgencyExternalBasicClient client = new CorrespondenceAgencyExternalBasicClient())
                {
                    var receipt = client.InsertCorrespondenceBasicV2(systemUserName, systemPassword, systemUserCode, externalShipmentReferece, correspondence);
                    if (receipt.ReceiptStatusCode == ReceiptStatusEnum.OK)
                    {
                        Logger.Log($"Correspondence sent successfully for reportee: {correspondence.Reportee} and reference: {correspondence.ArchiveReference}", false, EventLogEntryType.Information);
                    }
                    else
                    {
                        Logger.Log($"Failed to send correspondence for reportee: {correspondence.Reportee} and reference: {correspondence.ArchiveReference}");
                        throw new Exception($"Error while sending correspondence: {receipt.ReceiptStatusCode}, {receipt.ReceiptText}");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Log($"Failed to send correspondence for reportee: {correspondence.Reportee} and reference: {correspondence.ArchiveReference}, message: {exception.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initializes a new Correspondence with example values. Values are defined in SendInCorrespondence.config file.      
        /// </summary>
        /// <returns></returns>
        public static InsertCorrespondenceV2 CreateCorrespondence(string archiveReference, string reportee)
        {
            InsertCorrespondenceV2 correspondence = new InsertCorrespondenceV2();
            correspondence.ServiceCode = ConfigurationManager.AppSettings["serviceCode"];
            correspondence.ServiceEdition = ConfigurationManager.AppSettings["serviceEdition"];
            correspondence.Reportee = reportee;
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

