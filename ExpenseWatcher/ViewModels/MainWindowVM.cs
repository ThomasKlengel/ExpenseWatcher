using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using ViewModel;

namespace ExpenseWatcher.ViewModels
{
    //TODO: add class summaries
    class MainWindowVM : BaseViewModel
    {
        #region Fields        
        private Timer checkMailTimer;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowVM()
        {
            Logging.Log.Info("initializing...");

            Globals._mailClient.MailFinished += _mailClient_MailFinished;
            CsvReader.CsvFinished += _mailClient_MailFinished;
            // initial page is overview
            DisplayPage = new Views.ExpenseOverviewPage();

            // get payments until now from database
            Logging.Log.Info("getting payments from database");
            DataBaseHelper.GetPaymentsFromDB().ForEach(pm => Globals.Payments.Add(pm));
            Logging.Log.Info($"found {Globals.Payments.Count} payments");

            // get replacements until now from database
            DataBaseHelper.GetReplacementsFromDB().ForEach(rep => Globals.Replacements.Add(rep));

            // get categories until now from database
            Logging.Log.Info("getting categories from database");
            DataBaseHelper.GetCategoriesFromDB().ForEach(cat => Globals.Categories.Add(cat));
            Logging.Log.Info($"found {Globals.Categories.Count} categories");

            // get settings from database
            DataBaseHelper.GetSettingsFromDB().ForEach(set => Globals.Settings.Add(set));

            // initialize default settings
            if (!Globals.Settings.Any(s => s.Name == Globals.PAYPAL_FOLDER_SETTING))
            {
                Globals.Settings.Add(new Setting(Globals.PAYPAL_FOLDER_SETTING, "InBox"));
            }
            Logging.Log.Info($"PayPal folder is {Globals.Settings.Where(s=>s.Name==Globals.PAYPAL_FOLDER_SETTING).First().Value}");

            // initialize update timer
            checkMailTimer = new Timer(1000 * 60 * 20);
            checkMailTimer.Elapsed += CheckMailTimer_Elapsed;
            checkMailTimer.Start();

            OverviewCommand = new RelayCommand(ShowOverview);
            ReplacementsCommand = new RelayCommand(ShowReplacements);
            CategoriesCommand = new RelayCommand(ShowCategories);
            ChartsCommand = new RelayCommand(ShowCharts);
            SettingsCommand = new RelayCommand(ShowSettings);
            ReadCsvCommand = new RelayCommand(ReadCsv);

            Task.Run(() =>
            {
                Globals._mailClient.ReadImap();
            });
        } 
        #endregion

        #region Properties
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
                if (_displayPage != value)
                {
                    _displayPage = value;
                    NotifyPropertyChanged();
                }
            }
        } 
        #endregion

        #region Commands
        public RelayCommand OverviewCommand { get; private set; }
        public RelayCommand ReplacementsCommand { get; private set; }
        public RelayCommand CategoriesCommand { get; private set; }
        public RelayCommand ChartsCommand { get; private set; }
        public RelayCommand SettingsCommand { get; private set; }
        public RelayCommand ReadCsvCommand { get; private set; }

        private void ShowOverview(object o) { DisplayPage = new Views.ExpenseOverviewPage(); Save(); }
        private void ShowReplacements(object o) { DisplayPage = new Views.NameReplacementsPage(); Save(); }
        private void ShowCategories(object o) { DisplayPage = new Views.CategoriesPage(); Save(); }
        private void ShowCharts(object o) { DisplayPage = new Views.ChartsPage(); Save(); }
        private void ShowSettings(object o) { DisplayPage = new Views.SettingsPage(); Save(); }

        private void ReadCsv (object o) { DisplayPage = new Views.ReadCsvPage();/*CsvReader.ReadFromFile();*/ }

        #endregion

        #region private Methods
        private void Save()
        {
            Task.Run(() =>
            {
                DataBaseHelper.SaveReplacementsToDB();
                DataBaseHelper.SaveCategoriesToDB();
                DataBaseHelper.SaveSettingsToDB();
            });
        } 
        #endregion

        #region event handlers
        private void _mailClient_MailFinished()
        {
            // refresh payments in view
            App.Current.Dispatcher.Invoke(() =>
            {
                Globals.Payments.Clear();
                // get payments until now from database
                DataBaseHelper.GetPaymentsFromDB().ToList().ForEach(pm => Globals.Payments.Add(pm));
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
                Globals._mailClient.ReadImap();
            });
        } 
        #endregion
    }
}
