using SQLite;
using ViewModel;

namespace ExpanseWatcher.ViewModels
{
    public class ReplacementVM: BaseViewModel
    {


        private string _original;
        [PrimaryKey]
        public string Original
        {
            get { return _original; }
            set
            {
                if (_original != value)
                {
                    _original = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _replaced;
        public string Replaced
        {
            get { return _replaced; }
            set
            {
                if (_replaced != value)
                {
                    _replaced = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ReplacementVM() { }

        public ReplacementVM(string orig, string repl)
        {
            Original = orig;
            Replaced = repl;
        }

    }
}
