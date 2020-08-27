using System;
using System.Collections.Generic;
using System.Linq;
using ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using LiveCharts.Defaults;

namespace ExpanseWatcher.ViewModels
{
    class ChartsPageVM : BaseViewModel
    {

        public ChartsPageVM()
        {            
            Start_StartDate = Globals.Payments.OrderBy(p => p.DateOfPayment).FirstOrDefault().DateOfPayment.DateTime;
            Start_EndDate = Globals.Payments.OrderBy(p => p.DateOfPayment).LastOrDefault().DateOfPayment.DateTime;
            End_StartDate = Globals.Payments.OrderBy(p => p.DateOfPayment).FirstOrDefault().DateOfPayment.DateTime;
            End_EndDate = Globals.Payments.OrderBy(p => p.DateOfPayment).LastOrDefault().DateOfPayment.DateTime;
            Start_SelectedDate = Globals.Payments.OrderBy(p => p.DateOfPayment).FirstOrDefault().DateOfPayment.DateTime;
            End_SelectedDate = Globals.Payments.OrderBy(p => p.DateOfPayment).LastOrDefault().DateOfPayment.DateTime;

            SelectedChart = Charts.First();
        }

        public ObservableCollection<string> Charts { get; set; } = new ObservableCollection<string>() { "Pie/Kuchen", "StackedLine", "StackedArea" };

        private string _selectedChart;
        public string SelectedChart
        {
            get { return _selectedChart; }
            set
            {
                if (_selectedChart != value)
                {
                    _selectedChart = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(PieVisible));
                    NotifyPropertyChanged(nameof(StackedLineVisible));
                    NotifyPropertyChanged(nameof(StackedAreaVisible));
                    NotifyPropertyChanged(nameof(Series));
                }
            }
        }

        private DateTime _start_StartDate;
        public DateTime Start_StartDate
        {
            get { return _start_StartDate; }
            set
            {
                if (_start_StartDate != value)
                {
                    _start_StartDate = value;
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(PieSeries));
                    NotifyPropertyChanged(nameof(StackedAreaSeries));
                    NotifyPropertyChanged(nameof(StackedLineSeries));
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
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(PieSeries));
                    NotifyPropertyChanged(nameof(StackedAreaSeries));
                    NotifyPropertyChanged(nameof(StackedLineSeries));
                }
            }
        }

        private List<Payment> localPayments 
        {
            get
            {
                return Globals.Payments
                    .Where(p => p.DateOfPayment >= Start_SelectedDate && p.DateOfPayment <= End_SelectedDate)
                    .ToList();
            }
        }
        private List<Category> localCategories
        {
            get
            {
                List<Category> categories = new List<Category>();
                foreach (var p in localPayments)
                {
                    var category = Globals.Categories.Where(c => c.AttachedShops.Contains(p.Shop)).FirstOrDefault();
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
                return categories;
            }
        }

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
                    // sum up the amount of payments to the category
                    var sumPrice = 0.0;
                    foreach (var shop in cat.AttachedShops)
                    {
                        localPayments.Where(p => p.Shop == shop).ToList().ForEach(p => { sumPrice += p.Price;});
                    }
                    // add data to series collection
                    series.Values = new ChartValues<Double>() { sumPrice };
                    series.LabelPoint = chartPoint =>
                    string.Format($"{cat.Name} {chartPoint.Y}€ ({chartPoint.Participation:P})");
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
                // go through each category
                List<Payment> payInCat = new List<Payment>();
                foreach (var cat in Globals.Categories)
                {
                    var series = new StackedColumnSeries();
                    series.Title = cat.Name;
                    // sum up the amount of payments to the category
                    List<object> sumPrices = new List<object>();
                    for (int i = 12; i >= 0; i--)
                    {
                        var sumX = 0.0;
                        foreach (var shop in cat.AttachedShops)
                        {
                            localPayments.Where(p => p.Shop == shop
                                && p.DateOfPayment > DateTimeOffset.Now.AddDays((-i - 1) * 7)
                                && p.DateOfPayment < DateTimeOffset.Now.AddDays(-i * 7))
                                .ToList().ForEach(p => { sumX += p.Price; });
                        }
                        sumPrices.Add(sumX);
                    }
                    // add data to series collection
                    series.Values = new ChartValues<Double>();
                    series.Values.AddRange(sumPrices);
                    series.LabelPoint = chartPoint =>
                    string.Format($"{chartPoint.Y}€");
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

                    // put all pyaments of the category into the list
                    foreach (var shop in cat.AttachedShops)
                    {
                        localPayments
                            .Where(p => p.Shop == shop)
                            .OrderBy(p => p.DateOfPayment)
                            .ToList().ForEach(p =>
                            {
                                var dt = new DateTime(p.DateOfPayment.Ticks);
                                sumPrices.Where(sum => sum.DateTime == dt)
                                .FirstOrDefault().Value = p.Price;
                                //payInCat.Add(p);
                            });
                    }

                    // sum up the price with the previous one, so we get an ascending sum
                    for (int i = 1; i < sumPrices.Count; i++)
                    {
                        sumPrices[i].Value += sumPrices[i - 1].Value;
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
                return val => val.ToString("N0") + "€";
            }
        }

    }
}
