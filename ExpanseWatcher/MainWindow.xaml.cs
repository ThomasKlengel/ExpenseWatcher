using ActiveUp.Net.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;

namespace ExpanseWatcher2
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

        public void ReadImap()
        {
            var mailRepository = new MailRepository(
                                    "imap.gmail.com",
                                    993,
                                    true,
                                    "",
                                    ""
                                );

            var emailList = mailRepository.GetAllMails("PayPal");
            var payments = new List<Payment>();

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

                if (!success)
                {
                    continue;
                }
                // otherwise output data
                var PaymentText = match.Value;
                var priceText = match.Groups[2].Value;
                var shop = match.Groups[3].Value;
                double.TryParse(priceText, out double price);

                Debug.WriteLine($"From: {email.From}\r\nSubject: {email.Subject}\r\nMessage:{PaymentText}");
                if (email.Attachments.Count > 0)
                {
                    Debug.WriteLine("Attachements:");
                    foreach (MimePart attachment in email.Attachments)
                    {
                        Debug.WriteLine("Attachment: {0} {1}", attachment.ContentName, attachment.ContentType.MimeType);
                    }
                }

                payments.Add(new Payment(price, shop, new DateTimeOffset(email.Date.Ticks, new TimeSpan(0))));

            }
        }
    }

    class Payment
    {
        public double Price { get;  }

        public string Shop { get;  }

        public DateTimeOffset DateOfPayment { get;  }

        public Payment (double price, string shop, DateTimeOffset date)
        {
            Price = price;
            Shop = shop;
            DateOfPayment = date;
        }

        public Payment(double price, string shop) : this(price, shop, DateTimeOffset.UtcNow) { }


    }
}
