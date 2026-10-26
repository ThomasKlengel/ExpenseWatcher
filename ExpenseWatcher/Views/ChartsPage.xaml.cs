using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace ExpenseWatcher.Views
{
    /// <summary>
    /// Interaktionslogik für ChartsPage.xaml
    /// </summary>
    public partial class ChartsPage : Page
    {
        public ChartsPage()
        {
            InitializeComponent();
        }

        private void PieChart_DataClick(object sender, ChartPoint chartPoint)
        {
            if (chartPoint?.SeriesView is PieSeries pieSeries)
            {
                var vm = DataContext as ExpenseWatcher.ViewModels.ChartsPageVM;
                if (vm != null)
                {
                    vm.SelectPieCategory(pieSeries.Title);
                }
            }
        }
    }
}
