using System.Windows.Controls;
namespace ExpanseWatcher.Views
{
    /// <summary>
    /// Interaktionslogik für NameReplacements.xaml
    /// </summary>
    public partial class ExpenseOverviewPage : Page
    {
        public ExpenseOverviewPage()
        {
            InitializeComponent();
            this.DataContext = Globals.Payments;
        }
    }

}
