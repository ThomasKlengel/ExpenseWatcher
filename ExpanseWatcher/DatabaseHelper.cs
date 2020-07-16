using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpanseWatcher
{
    /// <summary>
    /// A helper class for database handling.
    /// </summary>
    public static class DataBaseHelper
    {
        /// <summary>
        /// the usual path to the database.
        /// </summary>
        private const string DEFAULTPATH = "Payments.DB";

        /// <summary>
        /// Adds a <see cref="Share"/> to the database,
        /// also adds a new <see cref="ShareValue"/> for today to the database.
        /// </summary>
        /// <param name="share">The <see cref="Share"/> to add.</param>
        /// <param name="path">The path to the database to insert the <see cref="Share"/>into.</param>
        /// <returns>1 if successful, 0 if a share matching the ISIN already exists, -1 if an error occured.</returns>
        public static short AddPaymentToDB(Payment payment, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Payment>();
                    con.Insert(payment);
                }

                return 1;
            }
            catch (Exception ex)
            {
                Logger.Log("AddPaymentToDB : " + ex);
                return -1;
            }
        }

        public static List<Payment> GetPaymentsFromDB(string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<Payment>();
                    // return the table as list, orderd by the ShareName
                    return con.Table<Payment>().ToList().OrderBy((pay) => { return pay.DateOfPayment; }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("GetPaymentsFromDB : " + ex);
                return null;
            }
        }

    }
}
