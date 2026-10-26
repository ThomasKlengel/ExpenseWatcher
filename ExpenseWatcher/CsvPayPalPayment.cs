using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseWatcher
{
    /// <summary>
    /// Data class for string the string values of a CSV file from PayPal. 
    /// </summary>
    internal class CsvPayPalPayment
    {
        /// <summary>
        /// The date of the payment, correlates to the <see cref="Payment.DateOfPayment"/>
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// The time of the payment, correlates to the <see cref="Payment.DateOfPayment"/>
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// The currency of the payment, usually all paments are in EUR.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The price of the payment, correlates to the <see cref="Payment.Price"/>
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// The recipuient of the payment, correlates to the <see cref="Payment.Shop"/>
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// The transaction id of the payment, correlates to the <see cref="Payment.TransactionCode"/>
        /// </summary>
        public string TransactionID { get; set; }

        /// <summary>
        /// The related id of the payment, correlates to a <see cref="TransactionID"/> of another <see cref="CsvPayPalPayment"/>.
        /// </summary>
        public string RelatedID { get; set; }
    }
}
