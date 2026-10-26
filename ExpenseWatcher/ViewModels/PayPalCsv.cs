using System.Collections.ObjectModel;
using ViewModel;

namespace ExpenseWatcher.ViewModels
{
    public class PayPalCsv: BaseViewModel
    {
        public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>();
    }
}
