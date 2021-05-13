using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using ViewModel;

namespace ExpanseWatcher.ViewModels
{
    public class ExpenseOverviewVM: BaseViewModel
    {      
        public ExpenseOverviewVM()
        {
            Payments = Globals.Payments;
            SortCommand = new RelayCommand(SortPayments);
            Globals._mailClient.MailFinished += _mailClient_MailFinished;
            CsvReader.CsvFinished += _mailClient_MailFinished;
        }
                
        public ObservableCollection<Payment> Payments { get; set; }

        #region event handlers
        private void _mailClient_MailFinished()
        {
            // refresh payments in view
            App.Current.Dispatcher.Invoke(() =>
            {
                lastSortAscending = false;
                // Sort the shares
                Payments = SortCollection<Payment>(Globals.Payments, "DateOfPayment", lastSortAscending);
            });
        }
        #endregion

        #region Commands
        public RelayCommand SortCommand { get; private set; }

        /// <summary>
        /// The command to execute.
        /// </summary>
        /// <param name="o">should be a <see cref="GridViewColumnHeader"/> which has been clicked.</param>
        private void SortPayments(object o)
        {
            if (Payments.Count > 1)
            {
                // check if clicked item is a column header
                if (o.GetType() == typeof(GridViewColumnHeader))
                {
                    var header = o as GridViewColumnHeader;

                    var headerClicked = string.Empty;
                    // get the propery name to sort by
                    // if the binding is a binding...
                    if (header.Column.DisplayMemberBinding.GetType() == typeof(Binding))
                    { // ... set the header to the bound path
                        var content = header.Column.DisplayMemberBinding as Binding;
                        headerClicked = content.Path.Path;
                    }
                    else
                    { // ... otherwise it's amount (which is a multibinding)
                        headerClicked = "Amount";
                    }

                    // get the sort Direction
                    if (lastSortedBy == headerClicked)
                    {
                        lastSortAscending = !lastSortAscending;
                    }
                    else
                    {
                        lastSortAscending = true;
                    }

                    // Sort the shares
                    Payments = SortCollection<Payment>(Globals.Payments, headerClicked, lastSortAscending);

                    // set the last sorted by for next sort
                    lastSortedBy = headerClicked;
                }
            }
        }

        #region For Sorting
        /// <summary>
        /// used for determining the sortdirection when sorting the collection.
        /// </summary>
        private string lastSortedBy;

        /// <summary>
        /// whether the last sort direction was ascending or decending.
        /// </summary>
        private bool lastSortAscending = true;

        /// <summary>
        /// Sorts an <see cref="ObservableCollection{T}"/> by a given property name of the collection items
        /// Mainly used for GridViews.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the <see cref="ObservableCollection{T}"/>s items.</typeparam>
        /// <param name="origCollection">The original <see cref="ObservableCollection{T}"/> to sort.</param>
        /// <param name="sortBy">The name of a property of an item of the <see cref="ObservableCollection{T}"/>.</param>
        /// <param name="ascending">The sort direction (true=ascending, false=descending).</param>
        /// <returns>The sorted <see cref="ObservableCollection{T}"/>.</returns>
        private ObservableCollection<T> SortCollection<T>(ObservableCollection<T> origCollection, string sortBy, bool ascending)
        {
            #region actual sorting
            // create a copy of the orders
            var tempCollection = new T[origCollection.Count];
            origCollection.CopyTo(tempCollection, 0);

            // create an empty collection
            IOrderedEnumerable<T> sortedCollection = null;

            // rename the sortBy since any sort by Date refers to the BookingDate
            sortBy = sortBy == "Date" ? "DateOfPayment" : sortBy;
            // get the property to sort by
            System.Reflection.PropertyInfo property = typeof(T).GetProperty(sortBy);

            // sort by property descending or ascending
            if (!ascending) // desceding
            {
                sortedCollection = tempCollection.OrderByDescending((itemOfCollection) => { return property.GetValue(itemOfCollection); });
            }
            else // ascending
            {
                sortedCollection = tempCollection.OrderBy((itemOfCollection) => { return property.GetValue(itemOfCollection); });
            }

            // clear the old orders collection
            origCollection.Clear();

            // add the orders from the sorted collection
            foreach (var item in sortedCollection)
            {
                origCollection.Add(item);
            }
            #endregion

            return origCollection;
        }  
        #endregion
        #endregion
    }
}
