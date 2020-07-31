using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using ViewModel;

namespace ExpanseWatcher.ViewModels
{
    class MainWindowVM : BaseViewModel
    {
        public MainWindowVM()
        {
            DisplayPage = new Views.ExpenseOverview();
            DataBaseHelper.GetPaymentsFromDB().ForEach(pm => Globals.Payments.Add(pm));

            checkMailTimer = new Timer(1000 * 60 * 20);
            checkMailTimer.Elapsed += CheckMailTimer_Elapsed;
            checkMailTimer.Start();
        }

        private void CheckMailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(() =>
            {               
                MailRepository.ReadImap();                
            }).Wait();
            Globals.Payments.Clear();
            DataBaseHelper.GetPaymentsFromDB().ForEach(pm => Globals.Payments.Add(pm));
        }

        private Timer checkMailTimer;

        public Page DisplayPage { get; set; }

    }
}
