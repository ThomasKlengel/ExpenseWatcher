using ExpanseWatcher.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ExpanseWatcher
{
    /// <summary>
    /// static class representing application wide accessible data
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// A list of payments that is currently managed by this application
        /// </summary>
        public static ObservableCollection<Payment> Payments { get; set; } = new ObservableCollection<Payment>();

        public static ObservableCollection<ReplacementVM> Replacements { get; set; } = new ObservableCollection<ReplacementVM>();

        public static ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        public static ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();

        public const string PAYPAL_FOLDER_SETTING = "PayPalFolder";

        public static MailClient _mailClient = new MailClient();
    }
}
