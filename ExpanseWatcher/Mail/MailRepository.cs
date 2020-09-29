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
        public MessageCollection GetAllMails(string mailBox)
        {
            Logging.Log.Info($"Reading all mails");
            return GetMails(mailBox, "ALL");
        }

        /// <summary>
        /// Gets all  unread mails from a mailbox
        /// </summary>
        /// <param name="mailBox">The name of the mailbox (or folder)</param>
        /// <returns></returns>
        public MessageCollection GetUnreadMails(string mailBox)
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
        public MessageCollection GetMailsSince(string mailBox, DateTime date)
        {       
            var searchPhrase = "SINCE " + date.ToString("dd-MMM-yyyy", new CultureInfo("en-US"))+
                " FROM service@paypal.de";
            Logging.Log.Info($"Reading mails: SearchPhrase:'{searchPhrase}'");
            return GetMails(mailBox, searchPhrase);
        }

        /// <summary>
        /// Gets all mail from a mailbox that match the criteria
        /// </summary>
        /// <param name="mailBox">The name of the mailbox (or folder)</param>
        /// <param name="searchPhrase">The criteria to search for</param>
        /// <returns></returns>
        public MessageCollection GetMails(string mailBox, string searchPhrase)
        {
            try
            {
                Mailbox mails = Client.SelectMailbox(mailBox);
                // get the message IDs
                var messageIds = mails.Search(searchPhrase);
                Logging.Log.Info($"found {messageIds.Length-1} messages matching '{searchPhrase}'");
                // get the messages
                MessageCollection messages = mails.SearchParse(searchPhrase);
                // set the message ID of the messages 
                // this is a bit stupid, but the message collection contains everything but the ID :(
                for (int i = 0; i < messages.Count; i++)
                {
                    messages[i].Id = messageIds[i];
                }
                Logging.Log.Info($"finished reading mails: SearchPhrase: {searchPhrase}");
                Logging.Log.Info($"Found {messages.Count} new mails");

                return messages;
            }
            catch (Exception ex)
            {
                Logging.Log.Error($"{ex.Message}:{ex.StackTrace}");
                return new MessageCollection();
            }            
        }

        /// <summary>
        /// Deletes a set of E-Mails by their messageID from a given mailbox
        /// </summary>
        /// <param name="messageIds">The IDs of the messages to delete</param>
        /// <param name="mailBox">The mailbox to delete the messages from</param>
        public void DeletMails(List<int> messageIds, string mailBox)
        {
            if (messageIds.Count<1)
            {
                return;
            }
            // get the mailbox
            Mailbox mails = Client.SelectMailbox(mailBox);

            // create a collection of flags to set for the emails
            var flags = new FlagCollection();
            // set only the deleted flag
            flags.Add(new Flag("DELETED"));

            Logging.Log.Info($"Deleting {messageIds.Count} mails");
            // set the flag for each email in the messageIds
            try
            {
                foreach (var id in messageIds)
                {
                    mails.SetFlags(id, flags);
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error($"{ex.Message}:{ex.StackTrace}");
            }

            Logging.Log.Info($"Finished deleting {messageIds.Count} mails");

            // this deletes the messages
        }

    }
}
