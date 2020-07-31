using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExpanseWatcher.Views
{
    /// <summary>
    /// Interaktionslogik für NameReplacements.xaml
    /// </summary>
    public partial class NameReplacements : Page
    {
        public NameReplacements()
        {
            InitializeComponent();

            this.DataContext = new NRViewModel();
        }
    }

    public class NRViewModel
    {
        public ObservableCollection<Replacement> Replacements { get; set; }

        public NRViewModel()
        {
            Replacements = new ObservableCollection<Replacement>();
            Replacements.Add(new Replacement("Netto Markendiscount", "Netto"));
            Replacements.Add(new Replacement("Kaufland GmbH", "Kaufland"));
            Replacements.Add(new Replacement("Aldi Gmbh und Co", "Aldi"));

        }

    }

    public class Replacement: INotifyPropertyChanged
    {
        private string _original;
        public string Original
        {
            get { return _original; }
            set
            {
                if (_original != value)
                {
                    _original = value;
                    RaisePropertyChange();
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
                    RaisePropertyChange();
                }
            }
        }

        public Replacement() { }

        public Replacement(string orig, string repl)
        {
            Original = orig;
            Replaced = repl;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChange([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
