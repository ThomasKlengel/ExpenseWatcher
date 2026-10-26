using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using MailKit;
using MailKit.Search;
using MailKit.Net.Imap;

/// <remarks>
/// https://doc.4d.com/4Dv16/4D-Internet-Commands/16/IMAP-Search.301-3069816.en.html
/// https://tools.ietf.org/html/rfc3501#page-51
/// </remarks>

namespace ExpenseWatcher.Mail
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

        /// <summary>
        /// The default folder to read fromn gmail.
        /// </summary>
        private const string DEFAULT_FOLDER = "InBox";

        /// <summary>
        /// A list of regexes that an email might match to generate a valid payment.
        /// </summary>
        private Dictionary<Regex,bool> PaylPaylRegexes = new Dictionary<Regex, bool>()
        {

            { new Regex(@"Sie\shaben\s(eine\s(Zahlung|Bestellung)\süber\s)?(\W?|.*;)(?<price>\d+[,.]\d{2})[\s\S]*EUR\san\s(?<shop>.*)\s(genehmigt|gesendet|autorisiert|gezahlt)",RegexOptions.Multiline|RegexOptions.IgnoreCase),false },
            { new Regex(@"(Sie|Du)\s(haben|hast)\s?(\W?|.*;)(?<price>\d+[,.]\d{2})\s?(.*)?(?<currency>EUR|USD)\san\s(?<shop>.*)\s(genehmigt|gesendet|autorisiert|gezahlt)",RegexOptions.Multiline|RegexOptions.IgnoreCase),false },
            { new Regex(@"^\s*(?<shop>[\w\s\.]*)\s?hat\s(Ihnen|Dir)\s?(\seine\sRückzahlung\sin\sHöhe\svon)?\s?(?<price>\d+[,.]\d{2}).*(?<currency>EUR|USD)\s(gesendet|gezahlt)",RegexOptions.Multiline|RegexOptions.IgnoreCase),true },
        };        

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
              
        /// <summary>
        /// Gets the emails from Gmail.
        /// </summary>
        /// <param name="folder">the gmail folder name we are interested in.</param>
        /// <param name="user">The user to login to a gmial account.</param>
        /// <param name="pw">The password for the user.</param>
        /// <param name="date">The date the Emails should be older then.</param>
        /// <returns>A list of emails that match the criteria.</returns>
        private static async Task<IEnumerable<MailInfo>> GetMails_MaiLKit(string folder, string user, string pw, DateTimeOffset date)
        {
            return Task.Run(() =>
            {
                using (var imap = new ImapClient())
                {
                    imap.Connect("imap.gmail.com", 993, true);
                    imap.Authenticate(user, pw);
                    HashSet<MailInfo> mails_m = new HashSet<MailInfo>();

                    List<IMailFolder> folders = new List<IMailFolder>();
                    foreach (var ns in imap.PersonalNamespaces)
                    {
                        folders.AddRange(imap.GetFolders(ns).ToList());
                    }

                    IEnumerable<IMailFolder> selectedFolders =
                    folders.Any(f => f.FullName.Contains(folder))
                    ? folders.Where(f => f.FullName.Contains(folder))
                    : folders.Where(f => f.FullName.Contains("Alle"));



                    //var query = SearchQuery.DeliveredAfter(new DateTime(date.Year, date.Month, date.Day))
                    //            .And(SearchQuery.FromContains("service@paypal.de"));

                    var query = SearchQuery.DeliveredAfter(new DateTime(2024, 1, 1))
                                .And(SearchQuery.FromContains("service@paypal.de"));

                    foreach (var folder in selectedFolders)
                    {
                        folder.Open(FolderAccess.ReadOnly);

                        foreach (var uid in folder.Search(query))
                        {
                            var message = folder.GetMessage(uid);
                            mails_m.Add(new MailInfo(message, uid,folder));
                        }
                    }

                    imap.Disconnect(true);
                    return mails_m;
                }               
                
            })
            .Result;
        }

        /// <summary>
        /// Permanently deletes the given emails from the gmail account.
        /// </summary>
        /// <param name="mails">The emails to delete.</param>
        private void DeleteMails(List<MailInfo> mails)
        {
            GetCredentials(out string user, out string pw);
            using (var imap = new ImapClient())
            {
                imap.Connect("imap.gmail.com", 993, true);
                imap.Authenticate(user, pw);

                List<IMailFolder> folders = new List<IMailFolder>();
                foreach (var ns in imap.PersonalNamespaces)
                {
                    folders.AddRange(imap.GetFolders(ns).ToList());
                }
                var trashFolder = imap.GetFolder(SpecialFolder.Trash);                

                foreach (var mail in mails)
                {
                    try
                    {
                        var folder = folders.Where(f => f.FullName == mail.ContainingFolder.FullName).First();
                        folder.Open(FolderAccess.ReadWrite);
                        var movedUid = folder.MoveTo(mail.UID, trashFolder).Value;
                        folder.Close(true);
                        trashFolder.Open(FolderAccess.ReadWrite);
                        trashFolder.AddFlags(movedUid, MailKit.MessageFlags.Deleted, true);
                        trashFolder.Expunge(new[] { movedUid });
                        trashFolder.Close(true);
                    }
                    catch (Exception ex)
                    {
                        Logging.Log.Warn($"{mail.ContainingFolder.Name}\r\n" + ex.Message);
                    }
                }

                imap.Disconnect(true);
            }
        }

        /// <summary>
        /// Tries to create a new payment from an email.
        /// </summary>
        /// <param name="mail">The email to create a payment from.</param>
        /// <param name="payment">The payment created. (null iof the creation failed.)</param>
        /// <returns>True if the creation was successfull, false otherwise.</returns>
        private bool MakePaymentFromEmail(MailInfo mail, out Payment payment)
        {
            payment = null;

            Match match = null;
            var success = false;
            bool isIncome = false;
            foreach (var paymentRegex in PaylPaylRegexes)
            {
                // search for the match
                match = paymentRegex.Key.Match(mail.PayPalPlainText);
                // if there is no match.. continue
                if (!match.Success)
                {
                    continue;
                }
                isIncome = paymentRegex.Value;
                success = true;
                break;
            }

            // continue to next email if the text didnt match any of the regular expressions
            if (!success)
            {
                Logging.Log.Warn($"no success matching:\r\n{mail.PayPalPlainText}");
                return false;
            }

            // otherwise fetch data

            var priceText = match.Groups["price"].Value.Replace(",", ".");
            if (match.Groups["currency"].Value != "EUR")
            {
                Regex euroCurrency = new Regex(@"Ausgangswährung:[\s]*\W?(?<price>\d+[,.]\d{2})\sEUR", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var euroMatch = euroCurrency.Match(mail.PayPalPlainText);
                if (euroMatch.Success)
                {
                    priceText = euroMatch.Groups["price"].Value.Replace(",", ".");
                }
            }
            var shop = match.Groups["shop"].Value;
            double.TryParse(priceText, out double price);
            if (!isIncome)
            {
                price = -Math.Abs(price);
            }
            else
            {
                price = Math.Abs(price);
            }

            // get transaction and authorization
            Regex transaktion = new Regex(@"Transaktionscode:?\s*(\w{17})", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Regex autorisierung = new Regex(@"Autorisierungscode:?\s*(\w{6})", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var tmatch = transaktion.Match(mail.PayPalPlainText);
            string trans = string.Empty;
            if (tmatch.Success)
            {
                trans = tmatch.Groups[1].Value;
            }
            else
            {
                return false;
            }

            var amatch = autorisierung.Match(mail.PayPalPlainText);
            string auth = string.Empty;
            if (amatch.Success)
            {
                auth = amatch.Groups[1].Value;
            }

            var escapedShop = Regex.Escape(shop.Trim());
            var infoRegex = new Regex($@"(Mitteilung)\s(?:an|von)\s{escapedShop}\s?:?\s*(?<info>.*)\s*(\r\n)?(Transaktionsdetails|(Bist du zufrieden)?)?\s?", RegexOptions.Multiline|RegexOptions.IgnoreCase);
            var iMatch = infoRegex.Match(mail.PayPalPlainText);
            string info = string.Empty;
            if (iMatch.Success)
            {
                info = iMatch.Groups["info"].Value.Replace("\r","").Replace("\n", "").Trim();
            }

            payment = new Payment(price, shop, new DateTimeOffset(mail.Mail.Date.LocalDateTime.Ticks, new TimeSpan(0)), trans, auth,info);
            return true;
        }

        /// <summary>
        /// Reads gmail for new messages from paypal and generates new payments ot of them.
        /// </summary>
        public void ReadPayPal()
        {
            var currentPayments = DataBaseHelper.GetPaymentsFromDB();
            DateTimeOffset date = (currentPayments?.Count > 0)
                ? currentPayments.Last().DateOfPayment.AddDays(-1)
                : new DateTimeOffset(DateTime.Today.AddYears(-10));

            GetCredentials(out string user, out string pw);

            var payPalFolder = Globals.Settings.FirstOrDefault(s => s.Name == Globals.PAYPAL_FOLDER_SETTING).Value;
            payPalFolder = payPalFolder == string.Empty ? DEFAULT_FOLDER : payPalFolder;

            // Get the mails from the paypalfolder
            var mails = GetMails_MaiLKit(payPalFolder, user, pw, date).Result;
            Logging.Log.Info($"Read {mails.Count()} Emails from PayPal folder");
            if (!mails.Any())
            {
                RaiseMailFinished();
                return;
            }

            List<MailInfo> noMatch = new List<MailInfo>();
            List<(MailInfo mail,Payment pay)> newPayments = new List<(MailInfo mail ,Payment pay)>();
            // parse the emails to payments
            foreach (var email in mails)
            {
                if (MakePaymentFromEmail(email, out Payment payment))
                {
                    // put data into a class
                    newPayments.Add((email,payment));
                }
                else
                {
                    noMatch.Add(email);
                }
            }

            Logging.Log.Info($"could not match {noMatch.Count} Emails");
            Logging.Log.Info($"found {newPayments.Count} new payments via PayPal");
            List<MailInfo> messagesToDelete = new List<MailInfo>();    
            // add the payments to the database
            foreach (var payment in newPayments)
            {
                if (currentPayments.Any(curPay => curPay.Equals(payment.pay)))
                {
                    Logging.Log.Info($"payment is already in database: {payment.pay.Price}€, {payment.pay.Shop}, {payment.pay.DateOfPayment.ToString("yyyy-MM-dd HH:mm:ss")}");
                    continue;
                }
                while (currentPayments.Any(curPay => curPay.DateOfPayment == payment.pay.DateOfPayment))
                {
                    payment.pay.DateOfPayment = payment.pay.DateOfPayment.AddMinutes(1);
                }
                var res = DataBaseHelper.AddPaymentToDB(payment.pay);
                if (res == 1)
                {
                   messagesToDelete.Add(payment.mail);
                }
            }

            RaiseMailFinished();

            // delete the messages from the gmail folders
            DeleteMails(messagesToDelete);
        }
    }
}
