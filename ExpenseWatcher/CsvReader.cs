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
                        var lineSplits = line.Replace("\"", "").Split(',');
                        if (lineSplits[4]== "Allgemeine Autorisierung")
                        {
                            continue;
                        }
                        var dateSplits = lineSplits[0].Split('.');
                        var timeSplits = lineSplits[1].Split(':');
                        DateTimeOffset dt = new DateTimeOffset(
                            int.Parse(dateSplits[2]),
                            int.Parse(dateSplits[1]),
                            int.Parse(dateSplits[0]),
                            int.Parse(timeSplits[2]),
                            int.Parse(timeSplits[1]),
                            int.Parse(timeSplits[0]),
                            new TimeSpan(0)
                            );
                        var price = double.Parse($"{lineSplits[7]},{lineSplits[8]}");
                        if (price>0)
                        {
                            continue;
                        }

                        var payment = new Payment(-price,lineSplits[3],dt, lineSplits[15]);
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
            foreach (var payment in newPayments)
            {
                if (currentPayments.Any(curPay => curPay.Equals(payment)))
                {
                    Logging.Log.Info($"payment is already in database: {payment.Price}€, {payment.Shop}, {payment.DateOfPayment.ToString("yyyy-MM-dd HH:mm:ss")}");
                    continue;
                }
                DataBaseHelper.AddPaymentToDB(payment);
            }

            RaiseCsvFinished();
        }

    }
}
