using Microsoft.Win32;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ViewModel;

namespace ExpenseWatcher.ViewModels
{
    /// <summary>
    /// A view model class for reading payments from a csv file.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class ReadCsvPageVM : BaseViewModel
    {
        /// <summary>
        /// All lines contained in a CSV.
        /// </summary>
        private List<string> allLines;

        /// <summary>
        /// The separators usually used in CSV files, used to select the right separator.
        /// </summary>
        private readonly string[] defaultSeperators = new[] { "\",\"", "\";\"", "\"\t\"" };

        /// <summary>
        /// Creates a new instance the <see cref="ReadCsvPageVM"/>.
        /// </summary>
        public ReadCsvPageVM()
        {
            ReadCsvPartCommand = new RelayCommand(ReadCsv);
            ConfirmCommand = new RelayCommand(Confirm, CanConfirm);
        }

        /// <summary>
        /// Gets or sets a list of all rows/lines in the CSV displayed to the user.
        /// </summary>
        public ObservableCollection<PayPalCsv> Rows { get; set; } = new ObservableCollection<PayPalCsv>();

        /// <summary>
        /// Gets or sets the headers for display in the GridView.
        /// </summary>
        public ObservableCollection<PayPalCsvHeader> Headers { get; set; } = new ObservableCollection<PayPalCsvHeader>();

        /// <summary>
        /// Gets or sets the first (3) lines of the CSV for display in UI.
        /// </summary>
        public string FirstLines { get; private set; }

        #region Commands
        /// <summary>
        /// Command for reading a csv file.
        /// </summary>
        public RelayCommand ReadCsvPartCommand { get; private set; }

        /// <summary>
        /// Opens and reads a CSV file.
        /// </summary>
        /// <param name="o">Any parameters that might be given to the function from the UI (none here).</param>
        private void ReadCsv(object o)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV|*.csv";
            if (ofd.ShowDialog() != true)
            {
                return;
            }

            allLines = File.ReadAllLines(ofd.FileName)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            FirstLines = string.Empty;
            Rows.Clear();
            Headers.Clear();

            if (TryImportBankCsv(allLines))
            {
                OnPropertyChanged(nameof(Headers));
                OnPropertyChanged(nameof(Rows));
                return;
            }

            if (!allLines.Any())
            {
                OnPropertyChanged(nameof(Headers));
                OnPropertyChanged(nameof(Rows));
                return;
            }

            // show first lines for information
            for (int i = 0; i < Math.Min(allLines.Count, 4); i++)
            {
                FirstLines += allLines[i] + "\r\n";
            }

            var separator = GetSeparator(allLines[0]);
            allLines[0].Split(separator).ToList().ForEach(s => Headers.Add(new PayPalCsvHeader() { HeaderFromFile = s.Trim('"') }));
            for (int i = 0; i < 5; i++)
            {
                Headers.Add(new PayPalCsvHeader() { HeaderFromFile = string.Empty });
            }

            foreach (var line in allLines.Skip(1))
            {
                var viewRow = new PayPalCsv();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                try
                {
                    var splits = line.Split(separator).ToList();
                    foreach (var s in splits)
                    {
                        viewRow.Items.Add(s.Trim('"'));
                    }
                    Rows.Add(viewRow);
                }
                catch (Exception ex)
                {
                    Logging.Log.Error($"Could not parse {line}. {ex}");
                }
            }

            OnPropertyChanged(nameof(Headers));
            OnPropertyChanged(nameof(Rows));
        }

        private bool TryImportBankCsv(List<string> lines)
        {
            if (lines == null || lines.Count < 5)
            {
                return false;
            }

            int headerIndex = lines.FindIndex(line => line.Contains("Buchungsdatum", StringComparison.OrdinalIgnoreCase));
            if (headerIndex < 0)
            {
                return false;
            }

            var header = SplitBankCsvLine(lines[headerIndex]);
            var normalizedHeaders = header.Select(NormalizeBankHeader).ToList();
            if (normalizedHeaders.Count == 0 || !normalizedHeaders.Any(h => h.Contains("betrag")))
            {
                return false;
            }

            var dateIndex = GetBankCsvColumnIndex(normalizedHeaders, "buchungsdatum", "wertstellung");
            var recipientIndex = GetBankCsvColumnIndex(normalizedHeaders, "zahlungsempf");
            var payerIndex = GetBankCsvColumnIndex(normalizedHeaders, "zahlungspflichtige", "zahlungspflichtige*r");
            var purposeIndex = GetBankCsvColumnIndex(normalizedHeaders, "verwendungszweck");
            var amountIndex = GetBankCsvColumnIndex(normalizedHeaders, "betrag");
            var statusIndex = GetBankCsvColumnIndex(normalizedHeaders, "status");
            var customerRefIndex = GetBankCsvColumnIndex(normalizedHeaders, "kundenreferenz");
            var mandatsRefIndex = GetBankCsvColumnIndex(normalizedHeaders, "mandatsreferenz");

            if (dateIndex < 0 || recipientIndex < 0 || purposeIndex < 0 || amountIndex < 0)
            {
                return false;
            }

            FirstLines = string.Join(Environment.NewLine, lines.Take(Math.Min(5, lines.Count)));

            List<Payment> payments = new List<Payment>();
            foreach (var line in lines.Skip(headerIndex + 1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var row = SplitBankCsvLine(line);
                if (row.Count < 5)
                {
                    continue;
                }

                var status = row.ElementAtOrDefault(statusIndex) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(status) && !status.Equals("Gebucht", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var amountText = row.ElementAtOrDefault(amountIndex) ?? string.Empty;
                if (!TryParseAmount(amountText, out double amount))
                {
                    continue;
                }

                var recipient = row.ElementAtOrDefault(recipientIndex) ?? string.Empty;
                var payer = row.ElementAtOrDefault(payerIndex) ?? string.Empty;
                var info = row.ElementAtOrDefault(purposeIndex) ?? string.Empty;
                var dateText = row.ElementAtOrDefault(dateIndex) ?? string.Empty;
                if (!TryParseBankDate(dateText, out DateTimeOffset date))
                {
                    continue;
                }

                var shopName = amount > 0 ? payer : recipient;
                Payment payment = new Payment(amount, shopName, date, string.Empty, info: info)
                {
                    Info = info,
                    Shop = string.IsNullOrWhiteSpace(shopName) ? info : shopName,
                };

                if (string.IsNullOrEmpty(payment.TransactionCode))
                {
                    payment.TransactionCode = $"BANK-{date:yyyyMMdd}-{Math.Abs(amount):0.00}-{Guid.NewGuid():N}";
                }

                if (IsExistingBankDuplicate(payment, Globals.Payments.ToList()))
                {
                    Logging.Log.Info($"Bank CSV payment skipped as duplicate: {payment.Shop} {payment.Price}€ {payment.DateOfPayment:yyyy-MM-dd}");
                    continue;
                }

                payments.Add(payment);
            }

            foreach (var payment in payments)
            {
                if (Globals.Payments.Any(curPay => curPay.Equals(payment)))
                {
                    continue;
                }

                while (Globals.Payments.Any(curPay => curPay.DateOfPayment == payment.DateOfPayment))
                {
                    payment.DateOfPayment = payment.DateOfPayment.AddMinutes(1);
                }

                Globals.Payments.Add(payment);
                DataBaseHelper.AddPaymentToDB(payment);
                Logging.Log.Info($"added bank CSV payment: {payment.Price}€, {payment.Shop}, {payment.DateOfPayment:yyyy-MM-dd HH:mm:ss}");
            }

            return payments.Count > 0;
        }

        private static bool IsExistingBankDuplicate(Payment candidate, List<Payment> existingPayments)
        {
            if (existingPayments == null || existingPayments.Count == 0)
            {
                return false;
            }

            var matchingPrice = existingPayments.Where(p => p.Price == candidate.Price);

            foreach (var existing in matchingPrice)
            {
                if (existing == null || string.IsNullOrWhiteSpace(existing.Shop))
                {
                    continue;
                }

                var dayDifference = Math.Abs((candidate.DateOfPayment - existing.DateOfPayment).TotalDays);
                if (dayDifference > 7)
                {
                    continue;
                }

                if (candidate.Info.ToLowerInvariant().EndsWith("ihr einkauf bei"))
                {
                    return true;
                }

                if (HasPayPalShop(candidate.Shop) || HasPayPalShop(existing.Shop))
                {
                    if (UsageMatchesShop(candidate.Info, existing.Shop) || UsageMatchesShop(existing.Info, candidate.Shop))
                    {
                        return true;
                    }
                }

                if (NormalizeComparableText(candidate.Shop) == NormalizeComparableText(existing.Shop))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasPayPalShop(string shop)
        {
            return !string.IsNullOrWhiteSpace(shop) && shop.Contains("PayPal", StringComparison.OrdinalIgnoreCase);
        }

        private static bool UsageMatchesShop(string usage, string shop)
        {
            if (string.IsNullOrWhiteSpace(usage) || string.IsNullOrWhiteSpace(shop))
            {
                return false;
            }

            var normalizedUsage = NormalizeComparableText(usage);
            var normalizedShop = NormalizeComparableText(shop);
            if (string.IsNullOrEmpty(normalizedUsage) || string.IsNullOrEmpty(normalizedShop))
            {
                return false;
            }

            if (normalizedUsage.Contains(normalizedShop, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (var token in GetNormalizedTokens(normalizedShop))
            {
                if (token.Length > 2 && normalizedUsage.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> GetNormalizedTokens(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<string>();
            }

            return Regex.Matches(value, "[a-z0-9]+", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(token => token.Length > 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string NormalizeComparableText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.ToLowerInvariant();
            normalized = normalized.Replace("&", string.Empty);
            normalized = normalized.Replace("+", string.Empty);
            normalized = Regex.Replace(normalized, "[^a-z0-9]", string.Empty);
            return normalized;
        }

        private static string NormalizeBankHeader(string header)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                return string.Empty;
            }

            return Regex.Replace(header.Trim(), "[^a-zA-Z0-9]", string.Empty).ToLowerInvariant();
        }

        private static int GetBankCsvColumnIndex(IReadOnlyList<string> headers, params string[] candidates)
        {
            for (int index = 0; index < headers.Count; index++)
            {
                foreach (var candidate in candidates)
                {
                    if (headers[index].Contains(candidate, StringComparison.OrdinalIgnoreCase))
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        private static List<string> SplitBankCsvLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return new List<string>();
            }

            return line.Split(';').Select(part => part.Trim().Trim('"')).ToList();
        }

        private static bool TryParseBankDate(string value, out DateTimeOffset date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var cleanValue = value.Trim();
            if (DateTimeOffset.TryParseExact(cleanValue, "dd.MM.yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }

            if (DateTimeOffset.TryParseExact(cleanValue, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }

            if (DateTime.TryParse(cleanValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                date = new DateTimeOffset(parsedDate, TimeSpan.Zero);
                return true;
            }

            return false;
        }

        private static bool TryParseAmount(string value, out double amount)
        {
            amount = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var cleanValue = value.Trim();
            cleanValue = cleanValue.Replace("\u00A0", string.Empty);
            cleanValue = cleanValue.Replace("'", string.Empty);
            cleanValue = cleanValue.Replace(" ", string.Empty);
            cleanValue = cleanValue.Replace("+", string.Empty);

            // Handles values like: --6,58 or -6,58 or 6,58
            if (cleanValue.StartsWith("--", StringComparison.Ordinal))
            {
                cleanValue = "-" + cleanValue.Substring(2);
            }

            cleanValue = cleanValue.Replace(".", string.Empty).Replace(",", ".");
            if (double.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
            {
                return true;
            }

            return false;
        }

        private static string GetSeparator(string line)
        {
            string separator = "\",\"";
            foreach (var sep in new[] { "\",\"", "\";\"", "\"\t\"" })
            {
                if (line.Split(sep).Length > 5)
                {
                    separator = sep;
                    break;
                }
            }

            return separator;
        }

        /// <summary>
        /// Command that confirms the selection of the coulumns of the payments.
        /// </summary>
        public RelayCommand ConfirmCommand { get; private set; }

        /// <summary>
        /// Confirms the selected columns to create new payments.
        /// Creates the payments and adds them to the database if they do not exist yet.
        /// </summary>
        /// <param name="o">Any parameters that might be given to the function from the UI (none here).</param>
        private void Confirm(object o)
        {
            // create a dictionary which contains the right column to each payment data.
            int i = 0;
            Dictionary<PayPalHeaders, int> columns = new Dictionary<PayPalHeaders, int>();
            foreach (var header in this.Headers)
            {
                PayPalHeaders h = (PayPalHeaders)header.SetHeader;
                if (h != PayPalHeaders.None)
                {
                    columns.Add(h, i);
                }
                i++;
            }

            // create new string-only payment data for each line
            var pays = Rows.Select(row =>
            {
                CsvPayPalPayment payment = new CsvPayPalPayment();
                foreach (var c in columns)
                {
                    switch (c.Key)
                    {
                        case PayPalHeaders.Date:
                            payment.Date = row.Items[c.Value];
                            break;
                        case PayPalHeaders.Time:
                            payment.Time = row.Items[c.Value];
                            break;
                        case PayPalHeaders.Amount:
                            payment.Amount = row.Items[c.Value];
                            break;
                        case PayPalHeaders.Currency:
                            payment.Currency = row.Items[c.Value];
                            break;
                        case PayPalHeaders.Recipient:
                            payment.Recipient = row.Items[c.Value];
                            break;
                        case PayPalHeaders.TransactionID:
                            payment.TransactionID = row.Items[c.Value];
                            break;
                        case PayPalHeaders.ReceivedTransaction:
                            payment.RelatedID = row.Items[c.Value];
                            break;
                    }
                }
                return payment;
            });

            var transactions = pays.Select(p => p.TransactionID);

            // list for all payments that should be added to the database
            // removes all the transactions from the bank for each payment
            var realTransactions = pays.Where(p => !transactions.Any(t => t == p.RelatedID)).Where(p => !string.IsNullOrEmpty(p.Recipient));
            var realDeclinedTransactions = pays.Where(p => !transactions.Any(t => t == p.RelatedID)).Where(p => string.IsNullOrEmpty(p.Recipient));
            foreach (var t in realDeclinedTransactions)
            {
                Logging.Log.Warn($"The transaction from\t{t.Date} {t.Time}" +
                    $" with an amount of {t.Amount}{t.Currency}," +
                    $" ID '{t.TransactionID}', relatedID '{t.RelatedID}' will not be added" +
                    $" due to missing recipient '{t.Recipient}'");
            }

            // create payments from the string data
            List<Payment> payments = new List<Payment>();
            realTransactions.ToList().ForEach(p =>
            {
                double.TryParse(p.Amount.Replace(".", string.Empty).Replace(",", "."), out double price);
                DateTimeOffset.TryParse($"{p.Date} {p.Time}", out DateTimeOffset date);
                payments.Add(new Payment(price * -1, p.Recipient, date, p.TransactionID));
            });

            // add new payments to database
            foreach (var payment in payments)
            {
                if (Globals.Payments.Any(curPay => curPay.Equals(payment)))
                {
                    Logging.Log.Info($"payment is already in database: {payment.Price}€, {payment.Shop}, {payment.DateOfPayment:yyyy-MM-dd HH:mm:ss}");
                    continue;
                }

                while (Globals.Payments.Any(curPay => curPay.DateOfPayment == payment.DateOfPayment))
                {
                    payment.DateOfPayment = payment.DateOfPayment.AddMinutes(1);
                }

                Globals.Payments.Add(payment);
                DataBaseHelper.AddPaymentToDB(payment);
                Logging.Log.Info("new payment was successfully added to database.");
            }

            Rows.Clear();
            Headers.Clear();
            FirstLines = string.Empty;
            allLines = null;
        }

        /// <summary>
        /// Checks if the selection of payment columns is correctly set.
        /// </summary>
        /// <param name="o">Any parameters that might be given to the function from the UI (none here).</param>
        /// <returns>True if the selection is complete, false otherwise.</returns>
        private bool CanConfirm(object o)
        {
            // Create dictionary with all headers, initially the amount they occur is zero.
            Dictionary<PayPalHeaders, int> headerAmounts = new Dictionary<PayPalHeaders, int>();
            foreach (var element in Enum.GetValues<PayPalHeaders>())
            {
                if (element != PayPalHeaders.None)
                {
                    headerAmounts.Add(element, 0);
                }
            }

            // Increase the header count depending on how they are set in the UI.
            foreach (var header in this.Headers)
            {
                PayPalHeaders h = (PayPalHeaders)header.SetHeader;
                if (h != PayPalHeaders.None)
                {
                    headerAmounts[h] = headerAmounts[h] + 1;
                }
            }

            // Every header should occur exactly once.
            foreach (var key in headerAmounts.Keys)
            {
                if (headerAmounts[key] != 1)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
