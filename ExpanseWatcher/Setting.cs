using SQLite;

namespace ExpanseWatcher
{
    /// <summary>
    /// class representing a category of <see cref="Payment"/>s (e.g. groceries, car)
    /// </summary>
    public class Setting
    {
        #region Properties
        /// <summary>
        /// The name of the setting
        /// </summary>
        [PrimaryKey]
        public string Name { get; set; }

        /// <summary>
        /// The value of the setting
        /// </summary>
        public string Value { get; set; }
        #endregion

        #region Consturctors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Setting() { }

        /// <summary>
        /// Consrtuctor
        /// </summary>
        /// <param name="name">The name of the category</param>
        public Setting(string name) : this(name, string.Empty) { }

        /// <summary>
        /// Consrtuctor
        /// </summary>
        /// <param name="name">The name of the category</param>
        /// <param name="shops">A list of shops that are contained in the category</param>
        public Setting(string name, string value)
        {
            Name = name;
            Value = value;
        }     
        #endregion
    }
}
