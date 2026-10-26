using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using ViewModel;

namespace ExpenseWatcher.ViewModels
{
    public class ExpenseOverviewVM: BaseViewModel
    {
        private string _filterShop;
        private string _filterPriceMin;
        private string _filterPriceMax;
        private string _filterInfo;
        private string _filterCategory;

        public ExpenseOverviewVM()
        {
            Payments = new ObservableCollection<Payment>();
            SortCommand = new RelayCommand(SortPayments);
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            Globals._mailClient.MailFinished += _mailClient_MailFinished;
            CsvReader.CsvFinished += _mailClient_MailFinished;
            ApplyFilters();
        }

        public ObservableCollection<Payment> Payments { get; set; }

        public RelayCommand ClearFiltersCommand { get; private set; }

        public void ClearFilters()
        {
            FilterShop = string.Empty;
            FilterPriceMin = string.Empty;
            FilterPriceMax = string.Empty;
            FilterInfo = string.Empty;
            FilterCategory = string.Empty;
            ApplyFilters();
        }

        public string FilterShop
        {
            get => _filterShop;
            set
            {
                if (_filterShop != value)
                {
                    _filterShop = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public string FilterPriceMin
        {
            get => _filterPriceMin;
            set
            {
                if (_filterPriceMin != value)
                {
                    _filterPriceMin = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public string FilterPriceMax
        {
            get => _filterPriceMax;
            set
            {
                if (_filterPriceMax != value)
                {
                    _filterPriceMax = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public string FilterInfo
        {
            get => _filterInfo;
            set
            {
                if (_filterInfo != value)
                {
                    _filterInfo = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public string FilterCategory
        {
            get => _filterCategory;
            set
            {
                if (_filterCategory != value)
                {
                    _filterCategory = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        #region event handlers
        private void _mailClient_MailFinished()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ApplyFilters();
            });
        }
        #endregion

        #region Commands
        public RelayCommand SortCommand { get; private set; }

        private void ApplyFilters()
        {
            var filteredPayments = Globals.Payments
                .Where(payment => MatchesFilter(payment))
                .ToList();

            Payments.Clear();
            foreach (var payment in filteredPayments)
            {
                Payments.Add(payment);
            }

            if (!string.IsNullOrWhiteSpace(lastSortedBy))
            {
                SortCollection<Payment>(Payments, lastSortedBy, lastSortAscending);
            }
        }

        private bool MatchesFilter(Payment payment)
        {
            if (payment == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(FilterShop) &&
                (payment.Shop == null || payment.Shop.IndexOf(FilterShop, StringComparison.OrdinalIgnoreCase) < 0))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(FilterInfo) &&
                (payment.Info == null || payment.Info.IndexOf(FilterInfo, StringComparison.OrdinalIgnoreCase) < 0))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(FilterCategory))
            {
                var categoryName = payment.CategoryName;
                if (categoryName == null || categoryName.IndexOf(FilterCategory, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            if (HasPriceFilter())
            {
                if (!MatchesPriceFilter(payment.Price, FilterPriceMin, FilterPriceMax))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasPriceFilter()
        {
            return !string.IsNullOrWhiteSpace(FilterPriceMin) || !string.IsNullOrWhiteSpace(FilterPriceMax);
        }

        private static bool MatchesPriceFilter(double priceValue, string minFilter, string maxFilter)
        {
            var minText = (minFilter ?? string.Empty).Trim();
            var maxText = (maxFilter ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(minText) && !string.IsNullOrWhiteSpace(maxText))
            {
                if (double.TryParse(minText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var minPrice) &&
                    double.TryParse(maxText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var maxPrice))
                {
                    return priceValue >= minPrice && priceValue <= maxPrice;
                }
            }

            if (!string.IsNullOrWhiteSpace(minText))
            {
                return MatchesContainsPrice(priceValue, minText);
            }

            if (!string.IsNullOrWhiteSpace(maxText))
            {
                return MatchesContainsPrice(priceValue, maxText);
            }

            return true;
        }

        private static bool MatchesContainsPrice(double priceValue, string filterValue)
        {
            var normalizedFilter = filterValue.Trim();
            if (string.IsNullOrWhiteSpace(normalizedFilter))
            {
                return true;
            }

            var priceText = Math.Abs(priceValue).ToString(CultureInfo.InvariantCulture).Replace(",", ".");

            var filterText = normalizedFilter
                .Replace(" ", string.Empty)
                .Replace(",", ".");

            if (filterText.StartsWith("-", StringComparison.Ordinal))
            {
                filterText = filterText.Substring(1);
            }

            if (!double.TryParse(filterText, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                return false;
            }

            return priceText.Contains(filterText, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The command to execute.
        /// </summary>
        /// <param name="o">should be a <see cref="GridViewColumnHeader"/> which has been clicked.</param>
        private void SortPayments(object o)
        {
            if (Payments.Count > 1)
            {
                // check if clicked item is a column header
                if (o.GetType() == typeof(GridViewColumnHeader))
                {
                    var header = o as GridViewColumnHeader;

                    var headerClicked = string.Empty;
                    // get the propery name to sort by
                    // if the binding is a binding...
                    if (header.Column.DisplayMemberBinding.GetType() == typeof(Binding))
                    { // ... set the header to the bound path
                        var content = header.Column.DisplayMemberBinding as Binding;
                        headerClicked = content.Path.Path;
                    }
                    else
                    { // ... otherwise it's amount (which is a multibinding)
                        headerClicked = "Amount";
                    }

                    // get the sort Direction
                    if (lastSortedBy == headerClicked)
                    {
                        lastSortAscending = !lastSortAscending;
                    }
                    else
                    {
                        lastSortAscending = true;
                    }

                    // Sort the shares
                    SortCollection<Payment>(Payments, headerClicked, lastSortAscending);

                    // set the last sorted by for next sort
                    lastSortedBy = headerClicked;
                }
            }
        }

        #region For Sorting
        /// <summary>
        /// used for determining the sortdirection when sorting the collection.
        /// </summary>
        private string lastSortedBy;

        /// <summary>
        /// whether the last sort direction was ascending or decending.
        /// </summary>
        private bool lastSortAscending = true;

        /// <summary>
        /// Sorts an <see cref="ObservableCollection{T}"/> by a given property name of the collection items
        /// Mainly used for GridViews.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the <see cref="ObservableCollection{T}"/>s items.</typeparam>
        /// <param name="origCollection">The original <see cref="ObservableCollection{T}"/> to sort.</param>
        /// <param name="sortBy">The name of a property of an item of the <see cref="ObservableCollection{T}"/>.</param>
        /// <param name="ascending">The sort direction (true=ascending, false=descending).</param>
        /// <returns>The sorted <see cref="ObservableCollection{T}"/>.</returns>
        private ObservableCollection<T> SortCollection<T>(ObservableCollection<T> origCollection, string sortBy, bool ascending)
        {
            #region actual sorting
            // create a copy of the orders
            var tempCollection = new T[origCollection.Count];
            origCollection.CopyTo(tempCollection, 0);

            // create an empty collection
            IOrderedEnumerable<T> sortedCollection = null;

            // rename the sortBy since any sort by Date refers to the BookingDate
            sortBy = sortBy == "Date" ? "DateOfPayment" : sortBy;
            // get the property to sort by
            System.Reflection.PropertyInfo property = typeof(T).GetProperty(sortBy);

            // sort by property descending or ascending
            if (!ascending) // desceding
            {
                sortedCollection = tempCollection.OrderByDescending((itemOfCollection) => { return property.GetValue(itemOfCollection); });
            }
            else // ascending
            {
                sortedCollection = tempCollection.OrderBy((itemOfCollection) => { return property.GetValue(itemOfCollection); });
            }

            // clear the old orders collection
            origCollection.Clear();

            // add the orders from the sorted collection
            foreach (var item in sortedCollection)
            {
                origCollection.Add(item);
            }
            #endregion

            return origCollection;
        }  
        #endregion
        #endregion
    }
}
