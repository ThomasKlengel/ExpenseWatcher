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

        public Payment(double price, string shop, DateTimeOffset date)
        {
            Price = price;
            Shop = shop;
            DateOfPayment = date;
        }

        public Payment(double price, string shop) : this(price, shop, DateTimeOffset.UtcNow) { } 
        #endregion
    }
}
