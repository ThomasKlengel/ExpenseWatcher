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
                Dictionary<string, double> data =
                    new Dictionary<string, double>() {};
                data.Add("Essen", 3.3);
                data.Add("Trinken", 1);
                data.Add("Tische", 12.21);
                var col = new SeriesCollection();
                foreach (var entry in data)
                {
                    var ps = new PieSeries();
                    ps.Title = entry.Key;
                    ps.Values = new ChartValues<Double>() {entry.Value } ;
                    ps.LabelPoint = chartPoint =>
                    string.Format($"{entry.Key} {chartPoint.Y} ({chartPoint.Participation:P})");
                    ps.DataLabels = true;
                    col.Add(ps);
                }


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
