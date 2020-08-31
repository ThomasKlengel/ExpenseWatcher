using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpanseWatcher.ViewModels
{
    public class ExpenseOverviewVM: ViewModel.BaseViewModel
    {
        public ExpenseOverviewVM()
        {
            Globals.MainWindowVM.DateChanged += MainWindowVM_DateChanged;
        }

        private void MainWindowVM_DateChanged()
        {
            NotifyPropertyChanged(nameof(LocalPayments));
        }

        public ObservableCollection<Payment> _localPayments = new ObservableCollection<Payment>();
        public ObservableCollection<Payment> LocalPayments
        {
            get
            {
                _localPayments.Clear();
                Globals.Payments
                    .Where(p => p.DateOfPayment >= Globals.MainWindowVM.Start_SelectedDate && p.DateOfPayment <= Globals.MainWindowVM.End_SelectedDate)
                    .ToList().ForEach(p => _localPayments.Add(p));
                return _localPayments;
            }
        }
    }
}
