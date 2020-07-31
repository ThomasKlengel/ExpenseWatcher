using System.Collections.ObjectModel;

namespace ExpanseWatcher
{
    public static class Globals
    {
        public static ObservableCollection<Payment> Payments { get; set; } = new ObservableCollection<Payment>();
    }
}
