using System;

namespace ExpanseWatcher
{
    /// <summary>
    /// Class representing the core components of an Email needed for this app
    /// </summary>
    public class EMail
    {
        /// <summary>
        /// The ID of the e-mail within the PayPal folder
        /// </summary>
        public int PayPalFolderID { get; set; }
        /// <summary>
        /// The ID of the e-mail within the INBOX
        /// </summary>
        public int InBoxID { get; set; }

        /// <summary>
        /// The subject of the e-mail
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// The text of the e-mail
        /// </summary>
        public string Body { get; set;}

        /// <summary>
        /// The Dtae and Time at which the e-mail was received
        /// </summary>
        public DateTime TimeReceived { get; set; }
    }
}
