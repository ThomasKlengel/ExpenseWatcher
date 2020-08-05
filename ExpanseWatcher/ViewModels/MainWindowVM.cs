using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        MailClient _mailClient=new MailClient();
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowVM()
        {
            _mailClient.MailFinished += _mailClient_MailFinished;
            // initial page is overview
            DisplayPage = new Views.ExpenseOverviewPage();

            // get payments until now from database
            DataBaseHelper.GetPaymentsFromDB().ForEach(pm => Globals.Payments.Add(pm));

            // get replacements until now from database
            DataBaseHelper.GetReplacementsFromDB().ForEach(rep => Globals.Replacements.Add(rep));

            // get categories until now from database
            DataBaseHelper.GetCategoriesFromDB().ForEach(cat => Globals.Categories.Add(cat));

            // initialize update timer
            checkMailTimer = new Timer(1000 * 60 * 20);
            checkMailTimer.Elapsed += CheckMailTimer_Elapsed;
            checkMailTimer.Start();

            OverviewCommand = new RelayCommand(ShowOverview);
            ReplacementsCommand = new RelayCommand(ShowReplacements);
            CategoriesCommand = new RelayCommand(ShowCategories);

            Task.Run(() =>
            {
                _mailClient.ReadImap();
            });
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
                _mailClient.ReadImap();
            });                       
        }

        private void _mailClient_MailFinished()
        {
            // refresh payments in view
            App.Current.Dispatcher.Invoke(()=> 
            {
                Globals.Payments.Clear();
                // get payments until now from database
                DataBaseHelper.GetPaymentsFromDB().ForEach(pm => Globals.Payments.Add(pm));
            });
            
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

        private void ShowOverview(object o) { DisplayPage = new Views.ExpenseOverviewPage(); Save(); }
        private void ShowReplacements(object o) { DisplayPage = new Views.NameReplacementsPage(); Save(); }
        private void ShowCategories(object o) { DisplayPage = new Views.CategoriesPage(); Save(); }
        
        private void Save ()
        {
            Task.Run(() =>
            {
                DataBaseHelper.SaveReplacementsToDB();
                DataBaseHelper.SaveCategoriesToDB();
            });
        }
    }
}
