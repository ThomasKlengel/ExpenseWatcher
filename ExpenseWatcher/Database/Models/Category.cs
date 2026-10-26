using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using sqla = SqliteAccessForExpenseWatcher;

namespace ExpenseWatcher
{
    /// <summary>
    /// class representing a category of <see cref="Payment"/>s (e.g. groceries, car)
    /// </summary>
    public class Category
    {
        #region Properties
        /// <summary>
        /// The name of the category
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// A list of shops thats <see cref="Payment"/>s are contained in the category
        /// </summary>
        public ObservableCollection<string> AttachedShops { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// A list of shops thats <see cref="Payment"/>s are contained in the category in Json 
        /// </summary>
        public string AttachedShopsJson
        {
            get
            {
                return JsonConvert.SerializeObject(AttachedShops);
            }
            set
            {
                if (value != null)
                {
                    AttachedShops = JsonConvert.DeserializeObject<ObservableCollection<string>>(value);
                }
            }
        }
        #endregion

        #region Consturctors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Category() { }

        /// <summary>
        /// Consrtuctor
        /// </summary>
        /// <param name="name">The name of the category</param>
        public Category(string name) : this(name, new List<string>()) { }

        /// <summary>
        /// Consrtuctor
        /// </summary>
        /// <param name="name">The name of the category</param>
        /// <param name="shops">A list of shops that are contained in the category</param>
        public Category(string name, List<string> shops)
        {
            Name = name;
            var obs = new ObservableCollection<string>();
            foreach (var shop in shops)
            {
                obs.Add(shop);
            }
            AttachedShops = obs;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the category</param>
        /// <param name="attachedShopsJson">A list of shops that are contained in the category in Json</param>
        public Category(string name, string attachedShopsJson)
        {
            Name = name;
            AttachedShops = JsonConvert.DeserializeObject<ObservableCollection<string>>(attachedShopsJson);
        }
        #endregion

        /// <summary>
        /// Converts a <see cref="Category"/> to a <see cref="sqla.Category"/> data base object.
        /// </summary>
        /// <returns>The converted <see cref="sqla.Category"/> DBO.</returns>
        public sqla.Category ConvertToDbo()
        {
            return new sqla.Category()
            {
                AttachedShopsJson = this.AttachedShopsJson,
                Name = this.Name,
            };
        }

        /// <summary>
        /// Creates a new instance of a <see cref="Category"/> from a <see cref="sqla.Category"/> data base object.
        /// </summary>
        /// <param name="categoryDBO">The <see cref="sqla.Category"/> that is used to create the creat the <see cref="Category"/>.</param>
        /// <returns>The resulting <see cref="Category"/>.</returns>
        public static Category CreateFromDbo(sqla.Category categoryDBO)
        {
            return new Category()
            {
                AttachedShopsJson = categoryDBO.AttachedShopsJson,
                Name = categoryDBO.Name,
            };
        }
    }
}
