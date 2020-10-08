using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpanseWatcher
{
    public static class CsvReader
    {
        #region Events
        /// <summary>
        /// event handler for the MailFinished event
        /// </summary>
        public delegate void ReadingCsvFinishedEventHandler();

        /// <summary>
        /// An event that occurs when Emails have been read
        /// </summary>
        public static event ReadingCsvFinishedEventHandler CsvFinished;

        /// <summary>
        /// Encapsulating method for raising the MailFinished event
        /// </summary>
        public static void RaiseCsvFinished()
        {
            CsvFinished?.Invoke();
        }
        #endregion

        // Element         Spalte
        // Datum            1
        // Uhrzeit          2
        // Shop             4
        // Preis            8,9
        // Transaktionscode 16

        public static void ReadFromFile()
        {
            List<Payment> newPayments = new List<Payment>();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            if (ofd.ShowDialog() == true)
            {
                var lines = File.ReadAllLines(ofd.FileName);
                lines[0] = "";

                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    try
                    {
                        var lineSplits = line.Split(new string[] { "\",\"" }, StringSplitOptions.None);
                        if (lineSplits[4].StartsWith("Allgemeine"))
                        {
                            continue;
                        }
                        var dateSplits = lineSplits[0].Replace("\"", "").Split('.');
                        var timeSplits = lineSplits[1].Split(':');
                        var year = int.Parse(dateSplits[2]);
                        var month = int.Parse(dateSplits[1]);
                        var day = int.Parse(dateSplits[0]);
                        var hour = int.Parse(timeSplits[0]);
                        var minute = int.Parse(timeSplits[1]);
                        var second = int.Parse(timeSplits[2]);

                        var dt = new DateTimeOffset(
                        year, month, day, hour, minute, second,
                        new TimeSpan(0)
                        );

                        var price = double.Parse(lineSplits[7]);
                        if (price > 0)
                        {
                            continue;
                        }

                        var payment = new Payment(-price, lineSplits[3], dt, lineSplits[12]);
                        newPayments.Add(payment);

                    }
                    catch (Exception ex)
                    {
                        Logging.Log.Error($"Could not Parse {line} to payment. {ex}");
                        continue;
                    }
                }
            }
            else { return; }

            var currentPayments = DataBaseHelper.GetPaymentsFromDB();
            Logging.Log.Info($"found {newPayments.Count} payments in {ofd.FileName}");
            int i = 0;
            foreach (var payment in newPayments)
            {
                if (currentPayments.Any(curPay => curPay.Equals(payment)))
                {
                    Logging.Log.Info($"payment is already in database: {payment.Price}€, {payment.Shop}, {payment.DateOfPayment.ToString("yyyy-MM-dd HH:mm:ss")}");
                    continue;
                }
                DataBaseHelper.AddPaymentToDB(payment);
                i++;
            }
            Logging.Log.Info($"added {i} new payments to database");

            RaiseCsvFinished();
        }

    }
}
