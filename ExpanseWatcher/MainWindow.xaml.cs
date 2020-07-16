using ActiveUp.Net.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace ExpanseWatcher
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            
            ReadImap();
        }

        /// <summary>
        /// Reads the credentials to log into gmail from a file.
        /// </summary>
        /// <param name="user">The user name to log in with.</param>
        /// <param name="password">The password to use.</param>
        private void GetCredentials(out string user, out string password)
        {
            var doc = XDocument.Load("Credentials.xml");

            user = doc.Root.Element("User").Attribute("value").Value;
            password = doc.Root.Element("Password").Attribute("value").Value;
        }

        public void ReadImap()
        {
            var currentPayments = DataBaseHelper.GetPaymentsFromDB();
            var date = currentPayments.Last().DateOfPayment;

            GetCredentials(out string user, out string pw);

            var mailRepository = new MailRepository(
                                    "imap.gmail.com",
                                    993,
                                    true,
                                    user,
                                    pw
                                );

            var emailList = mailRepository.GetMailsSince("PayPal", new DateTime(date.Year,date.Month,date.Day));
            var newPayments = new List<Payment>();

            var regexStrings = new List<string>();
            regexStrings.Add("Sie haben eine (Zahlung|Bestellung) über \\W{0,1}(\\d+[,.]\\d{2})\\sEUR an (.*) (genehmigt|gesendet|autorisiert)");

            foreach (Message email in emailList)
            {
                Match match = null;
                var success = false;
                foreach (var reg in regexStrings)
                {
                    // create regex for relevant text - group[0]=complete text, group[2]=amount, group[3]=shop
                    Regex paymentRegex = new Regex(reg);

                    // search for the match
                    match = paymentRegex.Match(email.BodyHtml.Text);
                    // if there is no match.. continue
                    if (!match.Success)
                    {
                        Debug.WriteLine(email.Subject);
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
                var priceText = match.Groups[2].Value;
                var shop = match.Groups[3].Value;
                double.TryParse(priceText, out double price);

                // put data into a class
                newPayments.Add(new Payment(price, shop, new DateTimeOffset(email.Date.Ticks, new TimeSpan(0))));               
            }

            foreach (var payment in newPayments)
            {
                if (currentPayments.Any(curPay=> curPay.Equals(payment)))
                {
                    continue;
                }
                DataBaseHelper.AddPaymentToDB(payment);
            }
        }
    }
}
