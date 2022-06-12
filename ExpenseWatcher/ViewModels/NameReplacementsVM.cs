using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModel;

namespace ExpenseWatcher.ViewModels
{
    /// <summary>
    /// class representing the view model for a list of company names that are to be replaced by shorter names
    /// </summary>
    class NameReplacementsVM : BaseViewModel
    {
        /// <summary>
        /// The list of replacements
        /// </summary>
        public ObservableCollection<ReplacementVM> Replacements { get; set; }

        /// <summary>
        /// The commmand that writes the replacements to the database
        /// </summary>
        public RelayCommand ApplyCommand { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public NameReplacementsVM()
        {
            Replacements = new ObservableCollection<ReplacementVM>();
            // get current replacements from list and fill the collection
            foreach (var rep in ExpenseWatcher.DataBaseHelper.GetReplacementsFromDB())
            {
                if (!Replacements.Any(repl => repl.Original == rep.Original))
                {
                    Replacements.Add(rep);
                }
            }
            DataBaseHelper.GetPaymentsFromDB().Select(pay => pay.Shop).Distinct().ToList()
                .ForEach((pm) =>
                {
                    if (!Replacements.Any(repl => repl.Original == pm))
                    {
                        Replacements.Add(new ReplacementVM(pm, pm));
                    }
                });
            ApplyCommand = new RelayCommand(Apply);

        }

        /// <summary>
        /// Writes all replacements to the database
        /// </summary>
        /// <param name="o">a dummy object</param>
        private void Apply(object o)
        {
            foreach (var replacement in Replacements)
            {
                ExpenseWatcher.DataBaseHelper.AddReplacementToDB(replacement);
            }
        }
    }
}
