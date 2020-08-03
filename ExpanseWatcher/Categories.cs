using SQLite;
using System.Collections.Generic;

namespace ExpanseWatcher
{
    /// <summary>
    /// class representing a category of <see cref="Payment"/>s (e.g. groceries, car)
    /// </summary>
    class Category
    {
        #region Properties
        /// <summary>
        /// The name of the category
        /// </summary>
        [PrimaryKey]
        public string Name { get; set; }

        /// <summary>
        /// A list of shops thats <see cref="Payment"/>s are contained in the category
        /// </summary>
        public List<string> AttachedShops { get; set; } = new List<string>(); 
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
            AttachedShops = shops;
        } 
        #endregion
    }
}
