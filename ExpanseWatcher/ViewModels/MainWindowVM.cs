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
            DisplayPage = new Views.ExpenseOverviewPage();

            // get payments until now from database
            DataBaseHelper.GetPaymentsFromDB().ForEach(pm => Globals.Payments.Add(pm));

            // initialize update timer
            checkMailTimer = new Timer(1000 * 60 * 20);
            checkMailTimer.Elapsed += CheckMailTimer_Elapsed;
            checkMailTimer.Start();

            OverviewCommand = new RelayCommand(ShowOverview);
            ReplacementsCommand = new RelayCommand(ShowReplacements);
            CategoriesCommand = new RelayCommand(ShowCategories);
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

        private Page _displayPage;
        /// <summary>
        /// The page that is currently displayed in the main frame of the main window
        /// </summary>
        public Page DisplayPage
        {
            get
            {
                return _displayPage;
            }
            set
            {
                if (_displayPage!=value)
                {
                    _displayPage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RelayCommand OverviewCommand { get; private set; }
        public RelayCommand ReplacementsCommand { get; private set; }
        public RelayCommand CategoriesCommand { get; private set; }

        private void ShowOverview(object o) { DisplayPage = new Views.ExpenseOverviewPage(); }
        private void ShowReplacements(object o) { DisplayPage = new Views.NameReplacementsPage(); }
        private void ShowCategories(object o) { DisplayPage = new Views.CategoriesPage(); }

    }
}
