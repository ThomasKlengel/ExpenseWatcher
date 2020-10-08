using System;

namespace ExpanseWatcher
{
    /// <summary>
    /// A class representing a payment via PayPal
    /// </summary>
    public class Payment
    {
        #region Properties
        /// <summary>
        /// The price that was payed
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// The shop the price was payed to
        /// </summary>
        public string Shop { get; set; }

        /// <summary>
        /// The code of the transaction
        /// </summary>
        public string TransactionCode { get; set; }
        
        /// <summary>
        /// The authorization code when payed via cellphone
        /// </summary>
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// The date of the payment
        /// </summary>
        public DateTimeOffset DateOfPayment { get; set; }
        #endregion

        /// <summary>
        /// Compares two payments by their <see cref="Price"/>, <see cref="Shop"/> and <see cref="DateOfPayment"/>
        /// </summary>
        /// <param name="obj">The payment to compare</param>
        /// <returns>TRUE if <see cref="Price"/>, <see cref="Shop"/> and <see cref="DateOfPayment"/> are the same</returns>
        public override bool Equals(object obj)
        {
            if (obj is Payment p)
            {
                if (p.Price == this.Price && p.Shop==this.Shop && p.DateOfPayment == this.DateOfPayment)
                {
                    return true;
                }
            }
            return false;
        }

        #region Constructors
        public Payment() { }

        /// <summary>
        /// Constructor for a <see cref="Payment"/>
        /// </summary>
        /// <param name="price">The price that was payed</param>
        /// <param name="shop">The shop the price was payed to</param>
        /// <param name="date">The date of the payment</param>
        /// <param name="transaction">The transaction code</param>
        /// <param name="authorization">The authorization code (can be empty)</param>
        public Payment(double price, string shop, DateTimeOffset date, string transaction, string authorization="")
        {
            Price = price;
            Shop = shop;
            DateOfPayment = date;
            TransactionCode = transaction;
            AuthorizationCode = authorization;
        }

        /// <summary>
        /// Constructor for a <see cref="Payment"/>
        /// </summary>
        /// <param name="price">The price that was payed</param>
        /// <param name="shop">The shop the price was payed to</param>
        /// <param name="transaction">The transaction code</param>
        public Payment(double price, string shop, string transaction) : this(price, shop, DateTimeOffset.UtcNow, transaction) { } 
        #endregion
    }
}
