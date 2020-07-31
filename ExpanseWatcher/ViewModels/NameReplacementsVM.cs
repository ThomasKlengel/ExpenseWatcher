using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModel.Base;

namespace ExpanseWatcher.ViewModels
{
    class NameReplacementsVM : BaseViewModel
    {
        public ObservableCollection<ReplacementVM> Replacements { get; set; }

        public RelayCommand ApplyCommand { get; set; }

        public NameReplacementsVM()
        {
            Replacements = new ObservableCollection<ReplacementVM>();
            Replacements.Add(new ReplacementVM("Netto Markendiscount", "Netto"));
            Replacements.Add(new ReplacementVM("Kaufland GmbH", "Kaufland"));
            Replacements.Add(new ReplacementVM("Aldi Gmbh und Co", "Aldi"));
            foreach (var rep in ExpanseWatcher.DataBaseHelper.GetReplacementsFromDB())
            {
                if (!Replacements.Any(repl => repl.Original == rep.Original))
                {
                    Replacements.Add(rep);
                }
            }
            ApplyCommand = new RelayCommand(Apply);

        }

        private void Apply(object o)
        {
            foreach (var replacement in Replacements)
            {
                ExpanseWatcher.DataBaseHelper.AddReplacementToDB(replacement);
            }
        }
    }
}
