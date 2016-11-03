using IntegrationAMelding.AltinnAMelding;
using IntegrationAMelding.Authentication;
using IntegrationAMelding.Correspondence;
using IntegrationAMelding.CorrespondenceListForReportee;
using IntegrationAMelding.Receipt;

using System;
using System.Configuration;
using System.Linq;
using ReceiptStatusEnum = IntegrationAMelding.Correspondence.ReceiptStatusEnum;

namespace IntegrationAMelding
{
    public class Program
    {
        static void Main()
        {
            var pins = new[]
            {
                "ajhhs", "piksd", "iuyhs", "asdfg", "rtefs", "loj7s", "mmmyp", "juksa", "fizck", "qicks", "98ujs",
                "mnbvs", "qwers", "polze", "ztang", "alt1n", "zcatt", "kjasd", "23as3", "lkiju", "4564s", "zxhfg",
                "alsks", "ooiks", "likme", "kaffe", "arbei", "00kks", "mjhgg", "ziste"
            };

            //create and submit FormTask
            int receiptId = SubmitFormTask();
            Console.WriteLine($"Formtask created and sent. ReceiptId: {receiptId}");

            //get archive reference
            string archiveReference = GetArchiveReference(receiptId);
            Console.WriteLine($"Archive reference: {archiveReference}");

            int pinIndex = GetPinIndexForAuthentication();
            Console.WriteLine($"Pin index for authentication: {pinIndex}");

            var pin = pins[pinIndex - 1];
            Console.WriteLine($"Pin for authentication: {pin}");

            //get correspondence
            ReporteeElementBEV2 correspondence = GetCorrespondence(pin, archiveReference);
            Console.WriteLine($"Correspondence: {correspondence.ArchiveReference}");

            //archive correspondence
            ArchiveCorrespondence(pin, correspondence);
            Console.WriteLine("Correspondence archived.");
        }

        private static int SubmitFormTask()
        {
            using (var client = new IntermediaryInboundExternalBasicClient())
            {
                var fts = new FormTaskShipmentBE
                {
                    ExternalShipmentReference = "uniqueReference" + DateTime.Now.ToLongTimeString(),
                    Reportee = ConfigurationManager.AppSettings["orgNo"]
                };

                var ft = new FormTask
                {
                    ServiceCode = ConfigurationManager.AppSettings["serviceCode"],
                    ServiceEdition = int.Parse(ConfigurationManager.AppSettings["serviceEdition"])
                };

                var form = new Form
                {
                    DataFormatId = ConfigurationManager.AppSettings["DataFormatId"],
                    DataFormatVersion = int.Parse(ConfigurationManager.AppSettings["DataFormatVersion"]),
                    FormData = ConfigurationManager.AppSettings["formData"],
                    EndUserSystemReference = "uniqueReferenceee" + DateTime.Now.ToLongTimeString(),
                    Completed = true
                };

                var formList = new Form[1];
                formList[0] = form;
                ft.Forms = formList;
                fts.FormTasks = ft;

                var receipt = client.SubmitFormTaskBasic(ConfigurationManager.AppSettings["systemUserName"],
                    ConfigurationManager.AppSettings["systemPassword"], null, null, null, null, fts);
                return receipt.ReceiptId;
            }
        }

        private static string GetArchiveReference(int receiptId)
        {
            try
            {
                using (var client = new ReceiptExternalBasicClient())
                {
                    var receiptSearch = new ReceiptSearchExternal
                    {
                        ReceiptId = receiptId,
                        References = new Receipt.ReferenceList()
                    };

                    var receipt = client.GetReceiptBasic(ConfigurationManager.AppSettings["systemUserName"],
                        ConfigurationManager.AppSettings["systemPassword"], receiptSearch);

                    foreach (var reference in receipt.References)
                    {
                        if (reference.ReferenceTypeName == Receipt.ReferenceType.ArchiveReference)
                            return reference.ReferenceValue;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to get archive reference: {exception.Message}");
                throw;
            }

            return string.Empty;
        }

        private static int GetPinIndexForAuthentication()
        {
            try
            {
                using (var client = new SystemAuthenticationExternalClient())
                {
                    AuthenticationChallengeRequestBE challengeRequest = new AuthenticationChallengeRequestBE
                    {
                        SystemUserName = ConfigurationManager.AppSettings["systemUserName"],
                        UserSSN = ConfigurationManager.AppSettings["ssn"],
                        AuthMethod = "AltinnPin"
                    };

                    var challenge = client.GetAuthenticationChallenge(challengeRequest);
                    if (challenge.Status == ChallengeRequestResult.Ok)
                    {
                        var digits = challenge.Message.SkipWhile(c => !char.IsDigit(c))
                            .TakeWhile(char.IsDigit)
                            .ToArray();

                        var str = new string(digits);
                        return int.Parse(str);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to get pin index for authentication: {exception.Message}");
                throw;
            }

            return -1;
        }

        private static ReporteeElementBEV2 GetCorrespondence(string pin, string archieveReference)
        {
            try
            {
                using (var client = new ReporteeElementListExternalBasicClient())
                {
                    ExternalSearchBEV2 searchList = new ExternalSearchBEV2
                    {
                        Reportee = ConfigurationManager.AppSettings["orgNo"],
                        ToDate = DateTime.Now
                    };


                    var correspondenceList = client.GetReporteeElementListBasicV2(
                        ConfigurationManager.AppSettings["systemUserName"],
                        ConfigurationManager.AppSettings["systemPassword"],
                        ConfigurationManager.AppSettings["ssn"],
                        ConfigurationManager.AppSettings["systemPassword"],
                        pin, "AltinnPin",
                        searchList,
                        int.Parse(ConfigurationManager.AppSettings["languageCode"]));

                    return correspondenceList.FirstOrDefault(c => c.ArchiveReference == archieveReference);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to get correspondence: {exception.Message}");
                throw;
            }
        }

        private static void ArchiveCorrespondence(string pin, ReporteeElementBEV2 correspondence)
        {
            try
            {
                using (var client = new CorrespondenceExternalBasicClient())
                {
                    //var correspondenceForEUS = client.GetCorrespondenceForEndUserSystemsBasicV2(
                    //    ConfigurationManager.AppSettings["systemUserName"],
                    //    ConfigurationManager.AppSettings["systemPassword"],
                    //    ConfigurationManager.AppSettings["ssn"],
                    //    ConfigurationManager.AppSettings["systemPassword"],
                    //    pin, "AltinnPin",
                    //    correspondence.SEReporteeElementID,
                    //    int.Parse(ConfigurationManager.AppSettings["languageCode"]));

                    var receipt = client.ArchiveCorrespondenceForEndUserSystemBasic(
                        ConfigurationManager.AppSettings["systemUserName"],
                        ConfigurationManager.AppSettings["systemPassword"],
                        ConfigurationManager.AppSettings["ssn"],
                        ConfigurationManager.AppSettings["systemPassword"],
                        pin, "AltinnPin",
                        correspondence.SEReporteeElementID);

                    if (receipt.ReceiptStatusCode != ReceiptStatusEnum.OK)
                    {
                        throw new InvalidOperationException($"{receipt.ReceiptStatusCode} : {receipt.ReceiptText}");
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to get correspondence: {exception.Message}");
                throw;
            }
        }
    }
}
