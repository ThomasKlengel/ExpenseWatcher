using System;

namespace ExpanseWatcher
{
    /// <summary>
    /// A class representing a payment via PayPal
    /// </summary>
    public class Payment
    {
        #region Properties
        public double Price { get; set; }

        public string Shop { get; set; }

        public string TransactionCode { get; set; }
        
        public string AuthorizationCode { get; set; }

        public DateTimeOffset DateOfPayment { get; set; }
        #endregion

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

        public Payment(double price, string shop, DateTimeOffset date, string transaction, string authorization="")
        {
            Price = price;
            Shop = shop;
            DateOfPayment = date;
            TransactionCode = transaction;
            AuthorizationCode = authorization;
        }

        public Payment(double price, string shop, string transaction) : this(price, shop, DateTimeOffset.UtcNow, transaction) { } 
        #endregion
    }
}
