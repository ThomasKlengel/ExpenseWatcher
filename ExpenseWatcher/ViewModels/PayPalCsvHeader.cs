using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModel;

namespace ExpenseWatcher.ViewModels
{
    /// <summary>
    /// Header class for a coulmn header read from a csv file.
    /// </summary>
    public class PayPalCsvHeader : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the header name from the csv file.
        /// </summary>
        public string HeaderFromFile { get; set; }

        /// <summary>
        /// Gets or sets the header from an enum of available heades <see cref="PayPalHeaders"/>.
        /// This is bound to a combo boxes selected item, its a bit shitty....
        /// </summary>
        public int SetHeader { get; set; } = 0;
    }
}
