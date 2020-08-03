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
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowVM()
        {
            // initial page is overview
            DisplayPage = new Views.ExpenseOverview();

            // get payments until now from database
            DataBaseHelper.GetPaymentsFromDB().ForEach(pm => Globals.Payments.Add(pm));

            // initialize update timer
            checkMailTimer = new Timer(1000 * 60 * 20);
            checkMailTimer.Elapsed += CheckMailTimer_Elapsed;
            checkMailTimer.Start();
        }

        /// <summary>
        /// Handler for when the update timer elapses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckMailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // read mails in seperate task
            Task.Run(() =>
            {               
                MailRepository.ReadImap();                
            }).Wait();

            // refresh payments in view
            Globals.Payments.Clear();
            DataBaseHelper.GetPaymentsFromDB().ForEach(pm => Globals.Payments.Add(pm));
        }

        private Timer checkMailTimer;


        /// <summary>
        /// The page that is currently displayed in the main frame of the main window
        /// </summary>
        public Page DisplayPage { get; set; }

    }
}
