using Newtonsoft.Json;
using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ExpanseWatcher
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
        [PrimaryKey]
        public string Name { get; set; }

        [Ignore]
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
    }
}
