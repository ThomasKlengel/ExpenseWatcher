using System;
using System.Collections.Generic;
using System.Linq;
using ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using LiveCharts.Defaults;
using System.ComponentModel;

namespace ExpenseWatcher.ViewModels
{
    class ChartsPageVM : BaseViewModel
    {
        #region Constructors
        public ChartsPageVM()
        {
            if (Globals.Payments.Count > 0)
            {
                var minDate = Globals.Payments.OrderBy(p => p.DateOfPayment).FirstOrDefault().DateOfPayment.DateTime;
                var maxDate = Globals.Payments.OrderBy(p => p.DateOfPayment).LastOrDefault().DateOfPayment.DateTime;

                Start_StartDate = minDate;
                Start_EndDate = maxDate;
                End_StartDate = minDate;
                End_EndDate = maxDate;
                Start_SelectedDate = DateTime.Now.Date.AddMonths(-3);
                End_SelectedDate = DateTime.Now.Date;
            }

            SelectedChart = Charts.First();
        }
        #endregion

        #region Properties
        public ObservableCollection<string> Charts { get; set; } = new ObservableCollection<string>() { "Pie/Kuchen", "StackedLine", "StackedArea" };

        public ObservableCollection<Payment> SelectedCategoryPayments { get; private set; } = new ObservableCollection<Payment>();

        private string _selectedChart;
        public string SelectedChart
        {
            get { return _selectedChart; }
            set
            {
                if (_selectedChart != value)
                {
                    _selectedChart = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PieVisible));
                    OnPropertyChanged(nameof(StackedLineVisible));
                    OnPropertyChanged(nameof(StackedAreaVisible));
                    OnPropertyChanged(nameof(Series));
                }
            }
        }

        private string _selectedPieCategory;
        public string SelectedPieCategory
        {
            get => _selectedPieCategory;
            set
            {
                if (_selectedPieCategory != value)
                {
                    _selectedPieCategory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasSelectedPieCategory));
                    UpdateSelectedCategoryPayments();
                }
            }
        }

        public bool HasSelectedPieCategory => !string.IsNullOrWhiteSpace(SelectedPieCategory);

        public void SelectPieCategory(string categoryName)
        {
            SelectedPieCategory = categoryName;
        }

        private void UpdateSelectedCategoryPayments()
        {
            SelectedCategoryPayments.Clear();

            if (string.IsNullOrWhiteSpace(SelectedPieCategory))
            {
                return;
            }

            if (string.Equals(SelectedPieCategory, "Others", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var payment in localPayments)
                {
                    if (payment.Price >= 0)
                    {
                        continue;
                    }

                    var resolvedCategory = GetCategoryForPayment(payment);
                    if (resolvedCategory == null || string.Equals(resolvedCategory.Name, "Others", StringComparison.OrdinalIgnoreCase))
                    {
                        SelectedCategoryPayments.Add(payment);
                    }
                }
                return;
            }

            var selectedCategory = Globals.Categories.FirstOrDefault(c => string.Equals(c.Name, SelectedPieCategory, StringComparison.OrdinalIgnoreCase));
            if (selectedCategory == null)
            {
                return;
            }

            foreach (var payment in localPayments)
            {
                if (payment.Price >= 0)
                {
                    continue;
                }

                if (IsPaymentInCategory(payment, selectedCategory))
                {
                    SelectedCategoryPayments.Add(payment);
                }
            }
        }

        public void ClearPieCategorySelection()
        {
            SelectedPieCategory = null;
        }

        #region DatePicker
        private DateTime _start_StartDate;
        public DateTime Start_StartDate
        {
            get { return _start_StartDate; }
            set
            {
                if (_start_StartDate != value)
                {
                    _start_StartDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _start_EndDate;
        public DateTime Start_EndDate
        {
            get { return _start_EndDate; }
            set
            {
                if (_start_EndDate != value)
                {
                    _start_EndDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _end_StartDate;
        public DateTime End_StartDate
        {
            get { return _end_StartDate; }
            set
            {
                if (_end_StartDate != value)
                {
                    _end_StartDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _end_EndDate;
        public DateTime End_EndDate
        {
            get { return _end_EndDate; }
            set
            {
                if (_end_EndDate != value)
                {
                    _end_EndDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _start_SelectedDate;
        public DateTime Start_SelectedDate
        {
            get { return _start_SelectedDate; }
            set
            {
                if (_start_SelectedDate != value)
                {
                    _start_SelectedDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PieSeries));
                    OnPropertyChanged(nameof(StackedAreaSeries));
                    OnPropertyChanged(nameof(StackedLineSeries));
                }
            }
        }

        private DateTime _end_SelectedDate;
        public DateTime End_SelectedDate
        {
            get { return _end_SelectedDate; }
            set
            {
                if (_end_SelectedDate != value)
                {
                    _end_SelectedDate = value.AddHours(23).AddMinutes(59).AddSeconds(59);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PieSeries));
                    OnPropertyChanged(nameof(StackedAreaSeries));
                    OnPropertyChanged(nameof(StackedLineSeries));
                }
            }
        }
        #endregion

        private List<Payment> localPayments
        {
            get
            {
                return Globals.Payments
                    .Where(p => p.DateOfPayment >= Start_SelectedDate && p.DateOfPayment <= End_SelectedDate)
                    .OrderBy(p => p.DateOfPayment)
                    .ToList();
            }
        }
        private Category GetCategoryForPayment(Payment payment)
        {
            if (payment == null)
            {
                return null;
            }

            var infoCategory = Globals.Categories.FirstOrDefault(c => c.MatchesInfo(payment));
            if (infoCategory != null)
            {
                return infoCategory;
            }

            return Globals.Categories.FirstOrDefault(c => c.MatchesShop(payment));
        }

        private bool IsPaymentInCategory(Payment payment, Category category)
        {
            if (payment == null || category == null)
            {
                return false;
            }

            if (category.MatchesInfo(payment))
            {
                return true;
            }

            return category.MatchesShop(payment);
        }

        private List<Category> localCategories
        {
            get
            {
                List<Category> categories = new List<Category>();
                foreach (var p in localPayments)
                {
                    if (p.Price > 0)
                    {
                        continue;
                    }

                    var category = GetCategoryForPayment(p);
                    if (category == null)
                    {
                        category = new Category("Others");
                    }

                    if (!categories.Any(c => c.Name == category.Name))
                    {
                        categories.Add(category);
                    }

                    if (category.Name == "Others")
                    {
                        var othersCategory = categories.Find(c => c.Name == "Others");
                        if (!othersCategory.AttachedShops.Contains(p.Shop))
                        {
                            othersCategory.AttachedShops.Add(p.Shop);
                        }
                    }
                }

                return categories
                    .Where(c => !string.Equals(c.Name, "Einkommen", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        #region Chart Visibility
        public Visibility PieVisible
        {
            get { return SelectedChart == "Pie/Kuchen" ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility StackedLineVisible
        {
            get { return SelectedChart == "StackedLine" ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility StackedAreaVisible
        {
            get { return SelectedChart == "StackedArea" ? Visibility.Visible : Visibility.Collapsed; }
        }
        #endregion

        public SeriesCollection PieSeries
        {
            get
            {
                var col = new SeriesCollection();
                // go through each category
                foreach (var cat in localCategories)
                {
                    var series = new PieSeries();
                    series.Title = cat.Name;
                    var sumPrice = localPayments
                        .Where(p => IsPaymentInCategory(p, cat))
                        .Where(p => p.Price < 0)
                        .Sum(p => Math.Abs(p.Price));
                    series.Values = new ChartValues<Double>() { sumPrice };
                    series.LabelPoint = chartPoint =>
                    string.Format($"{cat.Name} {chartPoint.Y:0.##}€ ({chartPoint.Participation:P})");
                    series.DataLabels = true;
                    col.Add(series);
                }

                return col;
            }
        }

        public SeriesCollection StackedLineSeries
        {
            get
            {
                var col = new SeriesCollection();
                List<Payment> payInCat = new List<Payment>();
                // order categories by amount of payments in this category
                var orderedCategories = new List<(Category category, int payCounter)>();
                localCategories.ForEach(c =>
                {
                    var payments = localPayments.Count(p => IsPaymentInCategory(p, c));
                    orderedCategories.Add((c, payments));
                });
                orderedCategories = orderedCategories.OrderByDescending(oc => oc.payCounter).ToList();
                foreach (var cat in orderedCategories.Select(oc => oc.category))
                {
                    var series = new StackedColumnSeries();
                    series.Title = cat.Name;
                    List<object> sumPrices = new List<object>();
                    for (int i = 12; i >= 0; i--)
                    {
                        var sumX = localPayments
                            .Where(p => IsPaymentInCategory(p, cat))
                            .Where(p => p.Price < 0)
                            .Where(p => p.DateOfPayment > DateTimeOffset.Now.AddDays((-i - 1) * 7))
                            .Where(p => p.DateOfPayment < DateTimeOffset.Now.AddDays(-i * 7))
                            .Sum(p => Math.Abs(p.Price));
                        sumPrices.Add(sumX);
                    }
                    series.Values = new ChartValues<Double>();
                    series.Values.AddRange(sumPrices);
                    series.LabelPoint = chartPoint =>
                    string.Format($"{chartPoint.Y:0.##}€");
                    series.DataLabels = true;
                    col.Add(series);
                }
                return col;
            }
        }

        public SeriesCollection StackedAreaSeries
        {
            get
            {
                var col = new SeriesCollection();

                // go through each category
                foreach (var cat in localCategories)
                {
                    var series = new StackedAreaSeries();
                    series.Title = cat.Name;
                    series.LineSmoothness = 0;

                    // initialzie list
                    List<DateTimePoint> sumPrices = new List<DateTimePoint>();
                    localPayments.ForEach(p => sumPrices.Add(new DateTimePoint(new DateTime(p.DateOfPayment.Ticks), 0)));
                    sumPrices.Add(new DateTimePoint(DateTime.Today.AddDays(2), 0));

                    localPayments
                        .Where(p => IsPaymentInCategory(p, cat))
                        .OrderBy(p => p.DateOfPayment)
                        .Where(p => p.Price < 0)
                        .ToList()
                        .ForEach(p =>
                        {
                            var dt = new DateTime(p.DateOfPayment.Ticks);
                            var match = sumPrices.FirstOrDefault(sum => sum.DateTime == dt);
                            if (match != null)
                            {
                                match.Value = Math.Abs(p.Price);
                            }
                        });

                    // sum up the price with the previous one, so we get an ascending sum
                    
                    for (int i = 1; i < sumPrices.Count; i++)
                    {
                        sumPrices[i].Value += Math.Abs(sumPrices[i - 1].Value);
                    }

                    // add data to series collection
                    series.Values = new ChartValues<DateTimePoint>();
                    series.Values.AddRange(sumPrices);
                    col.Add(series);
                }

                return col;
            }
        }

        /// <summary>
        /// A formatter for the x-axis values of the chart
        /// </summary>
        public Func<double, string> XDateFormat
        {
            get
            {
                return val => new DateTime((long)val).ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// A formatter for the y-axis values of the chart
        /// </summary>
        public Func<double, string> YEuroFormat
        {
            get
            {
                return val => val.ToString("0.##") + "€";
            }
        }
        #endregion
    }
}