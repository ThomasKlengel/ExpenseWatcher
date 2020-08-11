using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Controls;

namespace ExpanseWatcher.ViewModels
{
    class ChartsPageVM : BaseViewModel
    {

        public ChartsPageVM()
        {

        }

        public SeriesCollection Series
        {
            get
            {
                var col = new SeriesCollection();
                // go through each category
                List<Payment> payInCat = new List<Payment>();
                foreach (var cat in Globals.Categories)
                {
                    var series = new PieSeries();
                    series.Title = cat.Name;
                    // sum up the amount of payments to the category
                    var sumPrice = 0.0;
                    foreach (var shop in cat.AttachedShops)
                    {
                        Globals.Payments.Where(p => p.Shop == shop).ToList().ForEach(p => { sumPrice += p.Price; payInCat.Add(p); });
                    }
                    // add data to series collection
                    series.Values = new ChartValues<Double>() { sumPrice };
                    series.LabelPoint = chartPoint =>
                    string.Format($"{cat.Name} {chartPoint.Y}€ ({chartPoint.Participation:P})");
                    series.DataLabels = true;
                    col.Add(series);
                }


                // sum up the amount of payments not in any category
                var sum = 0.0;
                Globals.Payments.Where(p => !payInCat.Contains(p)).ToList().ForEach(p => sum += p.Price);
                // add data to series collection
                var ps = new PieSeries();
                ps.Title = "Other";
                ps.Values = new ChartValues<Double>() { sum };
                ps.LabelPoint = chartPoint =>
                string.Format($"{"Other"} {chartPoint.Y}€ ({chartPoint.Participation:P})");
                ps.DataLabels = true;
                col.Add(ps);

                return col;
            }
        }

        public SeriesCollection Series2
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
                            Globals.Payments.Where(p => p.Shop == shop 
                                && p.DateOfPayment>DateTimeOffset.Now.AddDays((-i-1)*7)
                                && p.DateOfPayment < DateTimeOffset.Now.AddDays(-i*7))
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


                //// sum up the amount of payments not in any category
                //var sum = 0.0;
                //Globals.Payments.Where(p => !payInCat.Contains(p)).ToList().ForEach(p => sum += p.Price);
                //// add data to series collection
                //var ps = new PieSeries();
                //ps.Title = "Other";
                //ps.Values = new ChartValues<Double>() { sum };
                //ps.LabelPoint = chartPoint =>
                //string.Format($"{"Other"} {chartPoint.Y}€ ({chartPoint.Participation:P})");
                //ps.DataLabels = true;
                //col.Add(ps);

                return col;
            }
        }
    }

    public class Data
    {
        public Data() { }

        public string Name { get; set; }
        public double Value { get; set; }

    }
}
