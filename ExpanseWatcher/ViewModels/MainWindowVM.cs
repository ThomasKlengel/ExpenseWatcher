using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using ViewModel;

namespace ExpanseWatcher.ViewModels
{
    public class MainWindowVM : BaseViewModel
    {
        MailClient _mailClient = new MailClient();
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

            // get settings from database
            DataBaseHelper.GetSettingsFromDB().ForEach(set => Globals.Settings.Add(set));

            // initialize default settings
            if (!Globals.Settings.Any(s => s.Name == Globals.PAYPAL_FOLDER_SETTING))
            {
                Globals.Settings.Add(new Setting(Globals.PAYPAL_FOLDER_SETTING, "PayPal"));
            }

            // initialize update timer
            checkMailTimer = new Timer(1000 * 60 * 20);
            checkMailTimer.Elapsed += CheckMailTimer_Elapsed;
            checkMailTimer.Start();

            // set commands
            OverviewCommand = new RelayCommand(ShowOverview);
            ReplacementsCommand = new RelayCommand(ShowReplacements);
            CategoriesCommand = new RelayCommand(ShowCategories);
            ChartsCommand = new RelayCommand(ShowCharts);
            SettingsCommand = new RelayCommand(ShowSettings);

            if (Globals.Payments.Any())
            {
                // set dates
                Start_StartDate = Globals.Payments.OrderBy(p => p.DateOfPayment).FirstOrDefault().DateOfPayment.DateTime;
                Start_EndDate = Globals.Payments.OrderBy(p => p.DateOfPayment).LastOrDefault().DateOfPayment.DateTime;
                End_StartDate = Globals.Payments.OrderBy(p => p.DateOfPayment).FirstOrDefault().DateOfPayment.DateTime;
                End_EndDate = Globals.Payments.OrderBy(p => p.DateOfPayment).LastOrDefault().DateOfPayment.DateTime;
                Start_SelectedDate = Globals.Payments.OrderBy(p => p.DateOfPayment).FirstOrDefault().DateOfPayment.DateTime;
                End_SelectedDate = Globals.Payments.OrderBy(p => p.DateOfPayment).LastOrDefault().DateOfPayment.DateTime;
            }
            else
            {
                Start_StartDate = DateTime.Now.AddYears(-1);
                Start_EndDate = DateTime.Now.AddYears(-1);
                End_StartDate = DateTime.Now;
                End_EndDate = DateTime.Now;
                Start_SelectedDate = DateTime.Now.AddDays(-1);
                End_SelectedDate = DateTime.Now;
            }

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
            App.Current.Dispatcher.Invoke(() =>
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
                if (_displayPage != value)
                {
                    _displayPage = value;
                    NotifyPropertyChanged();
                }
            }
        }


        private DateTime _start_StartDate;
        public DateTime Start_StartDate
        {
            get { return _start_StartDate; }
            set
            {
                if (_start_StartDate != value)
                {
                    _start_StartDate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private DateTime _start_EndDate;
        public DateTime Start_EndDate
        {
            get { return _start_EndDate; }
            set
            {
                if (_start_EndDate != value)
                {
                    _start_EndDate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private DateTime _end_StartDate;
        public DateTime End_StartDate
        {
            get { return _end_StartDate; }
            set
            {
                if (_end_StartDate != value)
                {
                    _end_StartDate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private DateTime _end_EndDate;
        public DateTime End_EndDate
        {
            get { return _end_EndDate; }
            set
            {
                if (_end_EndDate != value)
                {
                    _end_EndDate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private DateTime _start_SelectedDate;
        public DateTime Start_SelectedDate
        {
            get { return _start_SelectedDate; }
            set
            {
                if (_start_SelectedDate != value)
                {
                    _start_SelectedDate = value;
                    NotifyPropertyChanged();
                    RaiseDateChanged();
                }
            }
        }

        private DateTime _end_SelectedDate;
        public DateTime End_SelectedDate
        {
            get { return _end_SelectedDate; }
            set
            {
                if (_end_SelectedDate != value)
                {
                    _end_SelectedDate = value.AddHours(23).AddMinutes(59).AddSeconds(59);
                    NotifyPropertyChanged();
                    RaiseDateChanged();
                }
            }
        }

        public List<Payment> localPayments
        {
            get
            {
                return Globals.Payments
                    .Where(p => p.DateOfPayment >= Start_SelectedDate && p.DateOfPayment <= End_SelectedDate)
                    .ToList();
            }
        }

        private Visibility _dateVisibility = Visibility.Visible;
        public Visibility DateVisibility
        {
            get { return _dateVisibility; }
            set
            {
                if (_dateVisibility != value)
                {
                    _dateVisibility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RelayCommand OverviewCommand { get; private set; }
        public RelayCommand ReplacementsCommand { get; private set; }
        public RelayCommand CategoriesCommand { get; private set; }
        public RelayCommand ChartsCommand { get; private set; }
        public RelayCommand SettingsCommand { get; private set; }

        private void ShowOverview(object o) { DisplayPage = new Views.ExpenseOverviewPage(); Save(); DateVisibility = Visibility.Visible; }
        private void ShowReplacements(object o) { DisplayPage = new Views.NameReplacementsPage(); Save(); DateVisibility = Visibility.Collapsed; }
        private void ShowCategories(object o) { DisplayPage = new Views.CategoriesPage(); Save(); DateVisibility = Visibility.Collapsed; }
        private void ShowCharts(object o) { DisplayPage = new Views.ChartsPage(); Save(); DateVisibility = Visibility.Visible; }
        private void ShowSettings(object o) { DisplayPage = new Views.SettingsPage(); Save(); DateVisibility = Visibility.Collapsed; }

        private void Save()
        {
            Task.Run(() =>
            {
                DataBaseHelper.SaveReplacementsToDB();
                DataBaseHelper.SaveCategoriesToDB();
                DataBaseHelper.SaveSettingsToDB();
            });
        }

        public event DateChangedEventHandler DateChanged;

        public void RaiseDateChanged()
        {
            if (DateChanged != null)
            {
                DateChanged();
            }
        }
    }

    public delegate void DateChangedEventHandler();
}
