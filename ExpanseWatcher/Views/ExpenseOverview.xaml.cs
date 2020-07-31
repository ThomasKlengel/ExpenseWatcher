using System.Windows.Controls;
namespace ExpanseWatcher.Views
{
    /// <summary>
    /// Interaktionslogik für NameReplacements.xaml
    /// </summary>
    public partial class ExpenseOverview : Page
    {
        public ExpenseOverview()
        {
            InitializeComponent();
            this.DataContext = Globals.Payments;
        }
    }

}
