using sqla = SqliteAccessForExpenseWatcher;

namespace ExpenseWatcher
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

        /// <summary>
        /// Converts a <see cref="Setting"/> to a <see cref="sqla.Setting"/> data base object.
        /// </summary>
        /// <returns>The converted <see cref="sqla.Setting"/> DBO.</returns>
        public sqla.Setting ConvertToDbo()
        {
            return new sqla.Setting()
            {
                Value = this.Value,
                Name = this.Name,
            };
        }

        /// <summary>
        /// Creates a new instance of a <see cref="Setting"/> from a <see cref="sqla.Setting"/> data base object.
        /// </summary>
        /// <param name="settingDBO">The <see cref="sqla.Setting"/> that is used to create the creat the <see cref="Setting"/>.</param>
        /// <returns>The resulting <see cref="Setting"/>.</returns>
        public static Setting CreateFromDbo(sqla.Setting settingDBO)
        {
            return new Setting()
            {
                Value = settingDBO.Value,
                Name = settingDBO.Name,
            };
        }
    }
}
