using ExpanseWatcher.ViewModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpanseWatcher
{
    /// <summary>
    /// A helper class for SQlite database handling.
    /// </summary>
    public static class DataBaseHelper
    {
        /// <summary>
        /// the usual path to the database.
        /// </summary>
        private const string DEFAULTPATH = "Payments.DB";

        /// <summary>
        /// Adds a <see cref="Payment"/> to the database,
        /// </summary>
        /// <param name="payment">The <see cref="Payment"/> to add.</param>
        /// <param name="path">The path to the database to insert the <see cref="Payment"/>into.</param>
        /// <returns>1 if successful, -1 if an error occured.</returns>
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

        /// <summary>
        /// Gets all paments from the databse
        /// </summary>
        /// <param name="path">The path to the database</param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds a replacement to the database
        /// </summary>
        /// <param name="replacement">The <see cref="ReplacementVM"/> to add to the database</param>
        /// <param name="path">The path to the database</param>
        /// <returns></returns>
        public static short AddReplacementToDB(ReplacementVM replacement, string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<ReplacementVM>();
                    var existingReplacement = con.Find<ReplacementVM>(replacement.Original);
                    if (existingReplacement == null)
                    {
                        con.Insert(replacement);
                    }
                    else
                    {
                        existingReplacement.Replaced = replacement.Replaced;
                        con.RunInTransaction(() =>
                        {
                            con.Update(existingReplacement);
                        });
                    }

                }

                return 1;
            }
            catch (Exception ex)
            {
                Logger.Log("AddPaymentToDB : " + ex);
                return -1;
            }
        }

        /// <summary>
        /// Gets all replacements from the database
        /// </summary>
        /// <param name="path">The path to the database</param>
        /// <returns></returns>
        public static List<ReplacementVM> GetReplacementsFromDB(string path = DEFAULTPATH)
        {
            try
            {   // connect to the database
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    // get the required tables of the database
                    con.CreateTable<ReplacementVM>();
                    // return the table as list, orderd by the ShareName
                    return con.Table<ReplacementVM>().ToList().OrderBy((rep) => { return rep.Original; }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("GetReplacementsFromDB : " + ex);
                return null;
            }
        }

    }
}
