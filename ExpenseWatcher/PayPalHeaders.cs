using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseWatcher
{
    /// <summary>
    /// Enum continaing values for allowed headers of a csv file
    /// </summary>
    public enum PayPalHeaders
    {
        /// <summary>
        /// No header, columns with this setting will not be used to create a <see cref="Payment"/>.
        /// </summary>
        None,

        /// <summary>
        /// The date header. This column will in addition with <see cref="Time"/> be useed to generate the <see cref="DateTimeOffset"/> of a <see cref="Payment"/>.
        /// </summary>
        Date,

        /// <summary>
        /// The time header. This column will in addition with <see cref="Date"/> be useed to generate the <see cref="DateTimeOffset"/> of a <see cref="Payment"/>.
        /// </summary>
        Time,

        /// <summary>
        /// The amount header. This column will be useed to generate the <see cref="Payment.Price"/>.
        /// </summary>
        Amount,

        /// <summary>
        /// The currency header. This column is not really necessary but may come in handy later when we use different currencies.
        /// </summary>
        Currency,

        /// <summary>
        /// The recipient header. This column will be useed to generate the <see cref="Payment.Shop"/>.
        /// </summary>
        Recipient,

        /// <summary>
        /// The transaction id header. This column will be useed to generate the <see cref="Payment.TransactionCode"/>.
        /// </summary>
        TransactionID,

        /// <summary>
        /// The related transaction id header.
        /// This column will be useed to clear out the <see cref="Payment"/>s were not interseted in.
        /// This is the case for <see cref="Payment"/>s from the bank to paypal.
        /// These <see cref="Payment"/>s have a related transaction id which matches the
        /// transaction id of the real <see cref="Payment"/>s to the shop.
        /// </summary>
        ReceivedTransaction,
    }
}
