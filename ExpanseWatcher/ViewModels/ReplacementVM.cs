using SQLite;
using ViewModel;

namespace ExpanseWatcher.ViewModels
{
    /// <summary>
    /// class representing the view model for a single company name that is to be replaced by a shorter name
    /// </summary>
    public class ReplacementVM: BaseViewModel
    {
        /// <summary>
        /// The original name that is to be replaced
        /// </summary>
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

        /// <summary>
        /// The replaced name
        /// </summary>
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

        /// <summary>
        /// default constructor
        /// </summary>
        public ReplacementVM() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="orig">The orignal name that is to be replaced</param>
        /// <param name="repl">The replacement</param>
        public ReplacementVM(string orig, string repl)
        {
            Original = orig;
            Replaced = repl;
        }

    }
}
