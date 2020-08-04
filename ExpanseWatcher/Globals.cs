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

        public static List<Shop> Shops { get; set; } = new List<Shop>();

        public static ObservableCollection<ReplacementVM> Replacements { get; set; } = new ObservableCollection<ReplacementVM>();

        public static ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>() {
        new Category("testCat1", new List<string>(){"Netto", "ALDI", "Lidl" }),
        new Category("testCat2", new List<string>(){"Conrad", "MediaMarkt", "SAturn" }),
        };
    }
}
