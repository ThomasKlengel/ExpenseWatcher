using ActiveUp.Net.Mail;

namespace ExpanseWatcher2
{
    public class MailRepository
    {
        private Imap4Client client;

        public MailRepository(string mailServer, int port, bool ssl, string login, string password)
        {
            if (ssl)
                Client.ConnectSsl(mailServer, port);
            else
                Client.Connect(mailServer, port);
            Client.Login(login, password);
        }

        public MessageCollection GetAllMails(string mailBox)
        {
            return GetMails(mailBox, "ALL");
        }

        public MessageCollection GetUnreadMails(string mailBox)
        {
            return GetMails(mailBox, "UNSEEN");
        }

        protected Imap4Client Client
        {
            get { return client ?? (client = new Imap4Client()); }
        }

        public MessageCollection GetMails(string mailBox, string searchPhrase)
        {
            Mailbox mails = Client.SelectMailbox(mailBox);
            MessageCollection messages = mails.SearchParse(searchPhrase);
            return messages;
        }
    }
}
