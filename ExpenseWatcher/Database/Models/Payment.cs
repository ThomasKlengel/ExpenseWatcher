using System;
using System.Linq;
using sqla = SqliteAccessForExpenseWatcher;

namespace ExpenseWatcher
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

        public string Info { get; set; }

        public string CategoryName
        {
            get
            {
                if (Globals.Categories == null)
                {
                    return string.Empty;
                }

                var category = Globals.Categories.FirstOrDefault(c => c.MatchesInfo(this) || c.MatchesShop(this));
                return category?.Name ?? "Others";
            }
        }
        #endregion

        /// <summary>
        /// Compares two payments by their <see cref="TransactionCode"/>
        /// </summary>
        /// <param name="obj">The payment to compare</param>
        /// <returns>TRUE if <see cref="TransactionCode"/>s are the same</returns>
        public override bool Equals(object obj)
        {
            if (obj is Payment p)
            {
                if (!string.IsNullOrEmpty(this.TransactionCode) && !string.IsNullOrEmpty(p.TransactionCode))
                {
                    return string.Equals(this.TransactionCode, p.TransactionCode, StringComparison.Ordinal);
                }

                return Math.Abs(this.Price - p.Price) < 0.01
                    && this.DateOfPayment.Equals(p.DateOfPayment)
                    && string.Equals(this.Shop ?? string.Empty, p.Shop ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(this.Info ?? string.Empty, p.Info ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (!string.IsNullOrEmpty(this.TransactionCode))
            {
                return StringComparer.Ordinal.GetHashCode(this.TransactionCode);
            }

            var shopHash = this.Shop == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(this.Shop);
            var infoHash = this.Info == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(this.Info);
            var dateHash = this.DateOfPayment.Ticks.GetHashCode();
            var priceHash = this.Price.GetHashCode();
            return HashCode.Combine(shopHash, infoHash, dateHash, priceHash);
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
        /// <param name="info">Any additional information for the payment</param>
        public Payment(double price, string shop, DateTimeOffset date, string transaction, string authorization = "", string info="")
        {
            Price = price;
            Shop = shop;
            DateOfPayment = date;
            TransactionCode = transaction;
            AuthorizationCode = authorization;
            Info = info;
        }

        /// <summary>
        /// Constructor for a <see cref="Payment"/>
        /// </summary>
        /// <param name="price">The price that was payed</param>
        /// <param name="shop">The shop the price was payed to</param>
        /// <param name="transaction">The transaction code</param>
        public Payment(double price, string shop, string transaction) : this(price, shop, DateTimeOffset.UtcNow, transaction) { }
        #endregion

        /// <summary>
        /// Converts a <see cref="Payment"/> to a <see cref="sqla.Payment"/> data base object.
        /// </summary>
        /// <param name="payment">The <see cref="Payment" to convert./></param>
        /// <returns>The converted <see cref="sqla.Payment"/> DBO.</returns>
        public sqla.Payment ConvertToDbo()
        {
            return new sqla.Payment()
            {
                AuthorizationCode = this.AuthorizationCode,
                TransactionCode = this.TransactionCode,
                Shop = this.Shop,
                DateOfPayment = this.DateOfPayment,
                Price = this.Price,
                Info=this.Info,
            };
        }

        /// <summary>
        /// Creates a new instance of a <see cref="Payment"/> from a <see cref="sqla.Payment"/> data base object.
        /// </summary>
        /// <param name="paymentDBO">The <see cref="sqla.Payment"/> that is used to create the creat the <see cref="Payment"/>.</param>
        /// <returns>The resulting <see cref="Payment"/>.</returns>
        public static Payment CreateFromDbo (sqla.Payment paymentDBO)
        {
            return new Payment()
            {
                AuthorizationCode = paymentDBO.AuthorizationCode,
                TransactionCode = paymentDBO.TransactionCode,
                Shop = paymentDBO.Shop,
                DateOfPayment = paymentDBO.DateOfPayment,
                Price = paymentDBO.Price,
                Info = paymentDBO.Info,
            };
        }
    }
}
