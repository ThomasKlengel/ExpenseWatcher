using ActiveUp.Net.Mail;
using System;
using System.Collections.Generic;
using System.Globalization;

/// <remarks>
/// https://doc.4d.com/4Dv16/4D-Internet-Commands/16/IMAP-Search.301-3069816.en.html
/// https://tools.ietf.org/html/rfc3501#page-51
/// </remarks>

namespace ExpanseWatcher
{
    /// <summary>
    /// Class handling access to a IMAP Mailserver
    /// </summary>
    /// 
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
        /// Gets the ImapClient of this instance
        /// </summary>
        protected Imap4Client Client
        {
            get { return client ?? (client = new Imap4Client()); }
        }

        /// <summary>
        /// Gets all mails from a mailbox
        /// </summary>
        /// <param name="mailBox">The name of the mailbox (or folder)</param>
        /// <returns></returns>
        public List<EMail> GetAllMails(string mailBox)
        {
            Logging.Log.Info($"Reading all mails");
            return GetMails(mailBox, "ALL");
        }

        /// <summary>
        /// Gets all  unread mails from a mailbox
        /// </summary>
        /// <param name="mailBox">The name of the mailbox (or folder)</param>
        /// <returns></returns>
        public List<EMail> GetUnreadMails(string mailBox)
        {
            Logging.Log.Info($"Reading unread mails");
            return GetMails(mailBox, "UNSEEN");
        }

        /// <summary>
        /// Gets all mails from a mailbox since a certain date
        /// </summary>
        /// <param name="mailBox">The name of the mailbox (or folder)</param>
        /// <param name="date">The required date</param>
        /// <returns></returns>
        public List<EMail> GetMailsSince(string mailBox, DateTime date)
        {
            var searchPhrase = "SINCE " + date.ToString("dd-MMM-yyyy", new CultureInfo("en-US")) +
                " FROM service@paypal.de";
            Logging.Log.Info($"Reading mails: SearchPhrase:'{searchPhrase}'");
            return GetMails(mailBox, searchPhrase);
        }

        /// <summary>
        /// Gets all mail from a mailbox that match the criteria
        /// </summary>
        /// <param name="mailBoxName">The name of the mailbox (or folder)</param>
        /// <param name="searchPhrase">The criteria to search for</param>
        /// <returns></returns>
        public List<EMail> GetMails(string mailBoxName, string searchPhrase)
        {
            try
            {
                Mailbox mailBox = Client.SelectMailbox(mailBoxName);


                // get the message IDs
                var messageIds = mailBox.Search(searchPhrase);
                Logging.Log.Info($"found {messageIds.Length - 1} messages matching '{searchPhrase}'");
                // get the messages
                MessageCollection messages = mailBox.SearchParse(searchPhrase);
                // set the message ID of the messages 
                // this is a bit stupid, but the message collection contains everything but the ID :(
                List<EMail> mails = new List<EMail>();
                for (int i = 0; i < messages.Count; i++)
                {
                    mails.Add(
                        new EMail()
                        {
                            Body = messages[i].BodyHtml.TextStripped,
                            InBoxID = mailBoxName.ToUpperInvariant() == "INBOX" ? messageIds[i] : -1,
                            PayPalFolderID = mailBoxName.ToUpperInvariant() == "INBOX" ? -1 : messageIds[i],
                            Subject = messages[i].Subject,
                            TimeReceived = messages[i].Date
                        });
                }
                Logging.Log.Info($"finished reading mails: SearchPhrase: {searchPhrase}");
                Logging.Log.Info($"Found {messages.Count} new mails");

                return mails;
            }
            catch (Exception ex)
            {
                Logging.Log.Error($"{ex.Message}:{ex.StackTrace}");
                return new List<EMail>();
            }
        }

        /// <summary>
        /// Deletes a set of E-Mails by their messageID from a given mailbox
        /// </summary>
        /// <param name="messages">The IDs of the messages to delete</param>
        /// <param name="mailBoxName">The mailbox to delete the messages from</param>
        public void DeleteMails(List<EMail> messages, string mailBoxName)
        {
            if (messages.Count < 1)
            {
                return;
            }
            // get the mailbox
            Mailbox mailBox = Client.SelectMailbox(mailBoxName);

            // create a collection of flags to set for the emails
            var flags = new FlagCollection();
            // set only the deleted flag
            flags.Add(new Flag("DELETED"));

            
            // set the flag for each email in the messageIds
            try
            {
                Logging.Log.Info($"Deleting {messages.Count} mails from '{mailBoxName}'");
                if (mailBoxName == "INBOX")
                {
                    foreach (var mail in messages)
                    {
                        mailBox.SetFlags(mail.InBoxID, flags);
                    }
                }
                else
                {
                    foreach (var mail in messages)
                    {
                        mailBox.SetFlags(mail.PayPalFolderID, flags);
                    }
                }
                Logging.Log.Info($"Finished deleting {messages.Count} mails from '{mailBoxName}'");
            }
            catch (Exception ex)
            {
                Logging.Log.Error($"{ex.Message}:{ex.StackTrace}");
            }           

            Client.Expunge();
        }
    }
}
