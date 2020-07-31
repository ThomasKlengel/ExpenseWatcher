using ActiveUp.Net.Mail;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;



/// <remarks>
/// https://doc.4d.com/4Dv16/4D-Internet-Commands/16/IMAP-Search.301-3069816.en.html
/// https://tools.ietf.org/html/rfc3501#page-51
/// </remarks>

namespace ExpanseWatcher
{
    /// <summary>
    /// Class handling access to a IMAP Mailserver
    /// </summary>
    public class MailRepository
    {
        private Imap4Client client;

        /// <summary>
        /// Constuctor for a Mail Repository
        /// </summary>
        /// <param name="mailServer">The mail server e.g. "imap.gmail.com",</param>
        /// <param name="port">The port of the server e.g. 993</param>
        /// <param name="ssl">if SSL is to be used</param>
        /// <param name="login">The user login/name</param>
        /// <param name="password">The password for the user</param>
        public MailRepository(string mailServer, int port, bool ssl, string login, string password)
        {
            if (ssl)
                Client.ConnectSsl(mailServer, port);
            else
                Client.Connect(mailServer, port);
            Client.Login(login, password);
        }

        /// <summary>
        /// Gets all mails from a mailbox
        /// </summary>
        /// <param name="mailBox">The name of the mailbox (or folder)</param>
        /// <returns></returns>
        public MessageCollection GetAllMails(string mailBox)
        {
            return GetMails(mailBox, "ALL");
        }

        /// <summary>
        /// Gets all  unread mails from a mailbox
        /// </summary>
        /// <param name="mailBox">The name of the mailbox (or folder)</param>
        /// <returns></returns>
        public MessageCollection GetUnreadMails(string mailBox)
        {
            return GetMails(mailBox, "UNSEEN");
        }

        /// <summary>
        /// Gets all mails from a mailbox since a certain date
        /// </summary>
        /// <param name="mailBox">The name of the mailbox (or folder)</param>
        /// <param name="date">The required date</param>
        /// <returns></returns>
        public MessageCollection GetMailsSince(string mailBox, DateTime date)
        {
            var searchPhrase = "SINCE " + date.ToString("dd-MMM-yyyy", new CultureInfo("en-US"));

            return GetMails(mailBox, searchPhrase);
        }

        /// <summary>
        /// Gets the ImapClient of this instance
        /// </summary>
        protected Imap4Client Client
        {
            get { return client ?? (client = new Imap4Client()); }
        }

        /// <summary>
        /// Gets all mail from a mailbox that match the criteria
        /// </summary>
        /// <param name="mailBox">The name of the mailbox (or folder)</param>
        /// <param name="searchPhrase">The criteria to search for</param>
        /// <returns></returns>
        public MessageCollection GetMails(string mailBox, string searchPhrase)
        {
            Mailbox mails = Client.SelectMailbox(mailBox);
            MessageCollection messages = mails.SearchParse(searchPhrase);
            return messages;
        }

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

        public static void ReadImap()
        {
            var currentPayments = DataBaseHelper.GetPaymentsFromDB();
            DateTimeOffset date = (currentPayments?.Count > 0)
                ? currentPayments.Last().DateOfPayment
                : new DateTimeOffset(DateTime.Today.AddYears(-10));

            GetCredentials(out string user, out string pw);

            var mailRepository = new MailRepository(
                                    "imap.gmail.com",
                                    993,
                                    true,
                                    user,
                                    pw
                                );

            var emailList = mailRepository.GetMailsSince("PayPal", new DateTime(date.Year, date.Month, date.Day));
            var newPayments = new List<Payment>();

            var regexStrings = new List<string>();
            regexStrings.Add("Sie\\shaben\\s((eine\\s(Zahlung|Bestellung)\\süber\\s)|)(\\W{0,1}|.*;)(\\d+[,.]\\d{2})[\\s\\S]EUR\\san (.*) (genehmigt|gesendet|autorisiert)");

            foreach (Message email in emailList)
            {
                Match match = null;
                var success = false;
                foreach (var reg in regexStrings)
                {
                    // create regex for relevant text - group[0]=complete text, group[2]=amount, group[3]=shop
                    Regex paymentRegex = new Regex(reg);

                    // search for the match
                    match = paymentRegex.Match(email.BodyHtml.TextStripped);
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
                Regex transaktion = new Regex("Transaktionscode:\\s*([\\r\\n]|)\\s*(\\w{17})");
                Regex autorisierung = new Regex("Autorisierungscode:\\s*([\\r\\n]|)\\s*(\\w{6})");
                var tmatch = transaktion.Match(email.BodyHtml.TextStripped);
                string trans = "";
                if (tmatch.Success)
                {
                    trans = tmatch.Groups[2].Value;
                }
                else
                {
                    continue;
                }
                var amatch = autorisierung.Match(email.BodyHtml.TextStripped);
                string auth = "";
                if (amatch.Success)
                {
                    auth = amatch.Groups[2].Value;
                }

                // put data into a class
                newPayments.Add(new Payment(price, shop, new DateTimeOffset(email.Date.Ticks, new TimeSpan(0)), trans, auth));
            }

            foreach (var payment in newPayments)
            {
                if (currentPayments.Any(curPay => curPay.Equals(payment)))
                {
                    continue;
                }
                DataBaseHelper.AddPaymentToDB(payment);
            }
        }
    }
}
