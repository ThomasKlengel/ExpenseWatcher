using ActiveUp.Net.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

/// <remarks>
/// https://doc.4d.com/4Dv16/4D-Internet-Commands/16/IMAP-Search.301-3069816.en.html
/// https://tools.ietf.org/html/rfc3501#page-51
/// </remarks>

namespace ExpanseWatcher
{
    public class MailClient
    {
        #region Events
        /// <summary>
        /// event handler for the MailFinished event
        /// </summary>
        public delegate void ReadingMailFinishedEventHandler();

        /// <summary>
        /// An event that occurs when Emails have been read
        /// </summary>
        public event ReadingMailFinishedEventHandler MailFinished;

        /// <summary>
        /// Encapsulating method for raising the MailFinished event
        /// </summary>
        public void RaiseMailFinished()
        {
            MailFinished?.Invoke();
        }
        #endregion

        private const string DEFAULT_FOLDER = "InBox";

        /// <summary>
        /// Reads the credentials to log into gmail from a file.
        /// </summary>
        /// <param name="user">The user name to log in with.</param>
        /// <param name="password">The password to use.</param>
        public static void GetCredentials(out string user, out string password)
        {
            var doc = XDocument.Load("Credentials.xml");

            user = doc.Root.Element("User").Attribute("value").Value;
            password = doc.Root.Element("Password").Attribute("value").Value;
        }

        public void ReadImap()
        {
            var currentPayments = DataBaseHelper.GetPaymentsFromDB();
            DateTimeOffset date = (currentPayments?.Count > 0)
                ? currentPayments.Last().DateOfPayment.AddDays(-1)
                : new DateTimeOffset(DateTime.Today.AddYears(-10));

            GetCredentials(out string user, out string pw);

            var mailRepository = new MailRepository(
                                    "imap.gmail.com",
                                    993,
                                    true,
                                    user,
                                    pw
                                );

            var payPalFolder = Globals.Settings.FirstOrDefault(s => s.Name == Globals.PAYPAL_FOLDER_SETTING).Value;
            payPalFolder = payPalFolder == string.Empty ? DEFAULT_FOLDER : payPalFolder;
            // Get the mails from the Inbox and paypalfolder
            var emailList = mailRepository.GetMailsSince(payPalFolder, new DateTime(date.Year, date.Month, date.Day));
            var emailList2 = mailRepository.GetMailsSince("INBOX", new DateTime(date.Year, date.Month, date.Day));
            // put the ID of the Inobx into the collection
            emailList.ForEach(mail =>
            {
                if (emailList2.Any(m2 => m2.TimeReceived == mail.TimeReceived))
                {
                    mail.InBoxID = emailList2.First(m2 => m2.TimeReceived == mail.TimeReceived).InBoxID;
                }
            });

            var newPayments = new List<Payment>();

            var regexStrings = new List<string>();
            regexStrings.Add("Sie\\shaben\\s((eine\\s(Zahlung|Bestellung)\\süber\\s)|)(\\W{0,1}|.*;)(\\d+[,.]\\d{2})[\\s\\S]EUR\\san (.*) (genehmigt|gesendet|autorisiert)");

            List<EMail> messagesToDelete = new List<EMail>();
            foreach (var email in emailList)
            {
                Match match = null;
                var success = false;
                foreach (var reg in regexStrings)
                {
                    // create regex for relevant text - group[0]=complete text, group[2]=amount, group[3]=shop
                    Regex paymentRegex = new Regex(reg);

                    // search for the match
                    match = paymentRegex.Match(email.Body);
                    // if there is no match.. continue
                    if (!match.Success)
                    {
                        //Debug.WriteLine(email.Subject);
                        continue;
                    }
                    success = true;
                    break;
                }

                // continue to next email if the text didnt match any of the regular expressions
                if (!success)
                {
                    continue;
                }

                // otherwise fetch data
                var priceText = match.Groups[5].Value;
                var shop = match.Groups[6].Value;
                double.TryParse(priceText, out double price);

                // get transaction and authorization
                Regex transaktion = new Regex("Transaktionscode:{0,1}\\s*(\\w{17})");
                Regex autorisierung = new Regex("Autorisierungscode:{0,1}\\s*(\\w{6})");
                var tmatch = transaktion.Match(email.Body);
                string trans = "";
                if (tmatch.Success)
                {
                    trans = tmatch.Groups[1].Value;
                }
                else
                {
                    continue;
                }
                var amatch = autorisierung.Match(email.Body);
                string auth = "";
                if (amatch.Success)
                {
                    auth = amatch.Groups[1].Value;
                }

                // put data into a class
                newPayments.Add(new Payment(price, shop, new DateTimeOffset(email.TimeReceived.Ticks, new TimeSpan(0)), trans, auth));
                messagesToDelete.Add(email);
            }

            Logging.Log.Info($"found {newPayments.Count} new payments via PayPal");
            foreach (var payment in newPayments)
            {
                if (currentPayments.Any(curPay => curPay.Equals(payment)))
                {
                    Logging.Log.Info($"payment is already in database: {payment.Price}€, {payment.Shop}, {payment.DateOfPayment.ToString("yyyy-MM-dd HH:mm:ss")}");
                    continue;
                }
                while (currentPayments.Any(curPay => curPay.DateOfPayment == payment.DateOfPayment))
                {
                    payment.DateOfPayment = payment.DateOfPayment.AddMinutes(1);
                }
                currentPayments.Add(payment);
                DataBaseHelper.AddPaymentToDB(payment);
            }

            RaiseMailFinished();

            // delete the messages from the gmail folders
            mailRepository.DeleteMails(messagesToDelete, "INBOX");
            if (payPalFolder.ToUpper() != "INBOX")
            {
                mailRepository.DeleteMails(messagesToDelete, payPalFolder);
            }

        }
    }
}
