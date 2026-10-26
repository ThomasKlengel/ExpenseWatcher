using ExpenseWatcher.ViewModels;
//using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using sqla = SqliteAccessForExpenseWatcher;

namespace ExpenseWatcher
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

        #region Payments
        /// <summary>
        /// Adds a <see cref="Payment"/> to the database,
        /// </summary>
        /// <param name="payment">The <see cref="Payment"/> to add.</param>
        /// <param name="path">The path to the database to insert the <see cref="Payment"/>into.</param>
        /// <returns>1 if successful, -1 if an error occured.</returns>
        public static short AddPaymentToDB(Payment payment)
        {
            Logging.Log.Info($"adding pyament to database: {payment.Price}€, {payment.Shop}, {payment.DateOfPayment.ToString("yyyy-MM-dd HH:mm:ss")}");
            try
            {
                using (var db = new sqla.DataBaseContext())
                {
                    db.Add(payment.ConvertToDbo());
                    db.SaveChanges();
                }

                return 1;
            }
            catch (Exception ex)
            {
                Logging.Log.Error("AddPaymentToDB : " + ex);
                return -1;
            }
        }

        ///// <summary>
        ///// Adds a <see cref="Payment"/> to the database,
        ///// </summary>
        ///// <param name="payment">The <see cref="Payment"/> to add.</param>
        ///// <param name="path">The path to the database to insert the <see cref="Payment"/>into.</param>
        ///// <returns>1 if successful, -1 if an error occured.</returns>
        //public static short UpdatePaymentInDB(Payment payment)
        //{
        //    Logging.Log.Info($"updating pyament to database: {payment.Price}€, {payment.Shop}, {payment.DateOfPayment.ToString("yyyy-MM-dd HH:mm:ss")}");
        //    try
        //    {
        //        using (var db = new sqla.DataBaseContext())
        //        {
        //            db.Update(payment.ConvertToDbo());
        //            db.SaveChanges();
        //        }

        //        return 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logging.Log.Error("AddPaymentToDB : " + ex);
        //        return -1;
        //    }
        //}

        /// <summary>
        /// Gets all paments from the databse, ordered descending on date of payment
        /// </summary>
        /// <param name="path">The path to the database</param>
        /// <returns></returns>
        public static List<Payment> GetPaymentsFromDB()
        {
            List<sqla.Payment> dbPayments = new List<sqla.Payment>();
            try
            {
                using (var db = new sqla.DataBaseContext())
                {
                    dbPayments = db.Payments.ToList();
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error("GetPaymentsFromDB : " + ex + "\r\n" + ex.InnerException);
            }

            List<Payment> payments = new List<Payment>();
            dbPayments.ForEach(dbo => payments.Add(Payment.CreateFromDbo(dbo)));

            return payments;
        }
        #endregion

        //#region Replacements
        ///// <summary>
        ///// Adds a replacement to the database
        ///// </summary>
        ///// <param name="replacement">The <see cref="ReplacementVM"/> to add to the database</param>
        ///// <param name="path">The path to the database</param>
        ///// <returns></returns>
        //public static short AddReplacementToDB(ReplacementVM replacement, string path = DEFAULTPATH)
        //{
        //    try
        //    {   // connect to the database
        //        //using (SQLiteConnection con = new SQLiteConnection(path))
        //        //{
        //        //    // get the required tables of the database
        //        //    con.CreateTable<ReplacementVM>();
        //        //    var existingReplacement = con.Find<ReplacementVM>(replacement.Original);
        //        //    if (existingReplacement == null)
        //        //    {
        //        //        con.Insert(replacement);
        //        //    }
        //        //    else
        //        //    {
        //        //        existingReplacement.Replaced = replacement.Replaced;
        //        //        con.RunInTransaction(() =>
        //        //        {
        //        //            con.Update(existingReplacement);
        //        //        });
        //        //    }

        //        //}

        //        return 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log("AddReplacementToDB : " + ex);
        //        return -1;
        //    }
        //}

        //public static short SaveReplacementsToDB(string path = DEFAULTPATH)
        //{
        //    try
        //    {   // connect to the database
        //        //using (SQLiteConnection con = new SQLiteConnection(path))
        //        //{
        //        //    // get the required tables of the database                    
        //        //    con.DropTable<ReplacementVM>();
        //        //}
        //        //foreach (var rep in Globals.Replacements)
        //        //{
        //        //    AddReplacementToDB(rep);
        //        //}

        //        return 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log("SaveReplacementsToDB : " + ex);
        //        return -1;
        //    }

        //}

        ///// <summary>
        ///// Gets all replacements from the database
        ///// </summary>
        ///// <param name="path">The path to the database</param>
        ///// <returns></returns>
        //public static List<ReplacementVM> GetReplacementsFromDB(string path = DEFAULTPATH)
        //{
        //    try
        //    {   // connect to the database
        //        //using (SQLiteConnection con = new SQLiteConnection(path))
        //        //{
        //        //    // get the required tables of the database
        //        //    con.CreateTable<ReplacementVM>();
        //        //    // return the table as list, orderd by the ShareName
        //        //    return con.Table<ReplacementVM>().ToList().OrderBy((rep) => { return rep.Original; }).ToList();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log("GetReplacementsFromDB : " + ex);
        //        return new List<ReplacementVM>();
        //    }

        //    return new List<ReplacementVM>();
        //}
        //#endregion

        #region Categories
        /// <summary>
        /// Adds all <see cref="Globals.Categories"/> to the database.
        /// </summary>
        /// <returns>-1 on Error, 1 on success.</returns>
        public static short SaveCategoriesToDB()
        {
            try
            {
                using (var db = new sqla.DataBaseContext())
                {
                    var categories = db.Categories.ToList();
                    db.RemoveRange(categories.ToArray());
                    db.SaveChanges();
                    foreach (var category in Globals.Categories)
                    {
                        db.Add(category.ConvertToDbo());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error("AddSettingsToDB : " + ex);
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// Adds a category to the database
        /// </summary>
        /// <param name="item">The <see cref="Category"/> to add to the database</param>
        /// <param name="path">The path to the database</param>
        /// <returns></returns>
        public static short AddCategoryToDB(Category category)
        {
            try
            {
                using (var db = new sqla.DataBaseContext())
                {
                    db.Add(category.ConvertToDbo());
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error("AddCategoryToDB : " + ex);
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// Gets all replacements from the database
        /// </summary>
        /// <param name="path">The path to the database</param>
        /// <returns></returns>
        public static List<Category> GetCategoriesFromDB()
        {
            List<sqla.Category> dbCategories = new List<sqla.Category>();
            try
            {
                using (var db = new sqla.DataBaseContext())
                {
                    dbCategories = db.Categories.ToList();
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error("GetCategoriesFromDB : " + ex + "\r\n" + ex.InnerException);
            }

            List<Category> categories = new List<Category>();
            dbCategories.ForEach(dbo => categories.Add(Category.CreateFromDbo(dbo)));

            return categories;
        }
        #endregion

        #region Settings
        /// <summary>
        /// Gets all replacements from the database
        /// </summary>
        /// <param name="path">The path to the database</param>
        /// <returns></returns>
        public static List<Setting> GetSettingsFromDB(string path = DEFAULTPATH)
        {
            List<sqla.Setting> dbSettings = new List<sqla.Setting>();
            try
            {
                using (var db = new sqla.DataBaseContext())
                {
                    dbSettings = db.Settings.ToList();
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error("GetSettingsFromDB : " + ex + "\r\n" + ex.InnerException);
            }

            List<Setting> settings = new List<Setting>();
            dbSettings.ForEach(dbo => settings.Add(Setting.CreateFromDbo(dbo)));

            return settings;
        }

        /// <summary>
        /// Adds all <see cref="Globals.Settings"/> to the database.
        /// </summary>
        /// <returns>-1 on Error, 1 on success.</returns>
        public static short SaveSettingsToDB()
        {
            try
            {
                using (var db = new sqla.DataBaseContext())
                {
                    var settings = db.Settings.ToList();
                    db.RemoveRange(settings.ToArray());
                    db.SaveChanges();
                    foreach (var setting in Globals.Settings)
                    {
                        db.Add(setting.ConvertToDbo());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error("AddSettingsToDB : " + ex);
                return -1;
            }

            return 1;
        }
        #endregion
    }
}
