using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ViewModel;

namespace ExpanseWatcher.ViewModels
{
    public class CategoriesPageVM : BaseViewModel
    {
        private Dictionary<string, List<int>> AllCategories;

        public CategoriesPageVM()
        {

            AssignItemCommand = new RelayCommand(AssignItem, CanAssignItem);
            UnassignItemCommand = new RelayCommand(UnassignItem, CanUnassignItem);
            AddCategoryCommand = new RelayCommand(AddCategory, CanAddCategory);
            RemoveCategoryCommand = new RelayCommand(RemoveCategory, CanRemoveCategory);

            Categories = Globals.Categories;
            Globals.Shops.ForEach(s=> UnassignedItems.Add(s.Name));
        }

        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        private Category _selectedCategory;
        /// <summary>
        /// The category that is currently selected
        /// </summary>
        public Category SelectedCategory
        {
            get
            {
                return _selectedCategory;
            }
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    _newCategory = _selectedCategory.Name;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(NewCategory));
                }
            }
        }

        private string _newCategory;
        /// <summary>
        /// A new category that can be added
        /// </summary>
        public string NewCategory
        {
            get
            {
                return _newCategory;
            }
            set
            {
                if (_newCategory != value)
                {
                    _newCategory = value;
                    NotifyPropertyChanged();
                }
            }

        }

        /// <summary>
        /// Items assigned to this category
        /// </summary>
        public ObservableCollection<string> AssignedItems
        {
            get
            {
                var coll = new ObservableCollection<string>();
                if (SelectedCategory!=null)
                {
                    //var cat = Categories.FirstOrDefault(c => c.Name == SelectedCategory);
                    foreach (var item in SelectedCategory.AttachedShops)
                    {
                        coll.Add(item);
                    }
                }
                return coll;
            }
        }

        public string _selectedAssigned;
        public string SelectedAssigned
        {
            get
            {
                return _selectedAssigned;
            }
            set{
                if (_selectedAssigned != value)
                {
                    _selectedAssigned = value;
                    NotifyPropertyChanged();
                }

            }
        }

        public string _selectedUnassigned;
        public string SelectedUnassigned
        {
            get
            {
                return _selectedUnassigned;
            }
            set
            {
                if (_selectedUnassigned != value)
                {
                    _selectedUnassigned = value;
                    NotifyPropertyChanged();
                }

            }
        }

        /// <summary>
        /// items that are not assigned to any category
        /// </summary>
        public ObservableCollection<string> UnassignedItems { get; set; } = new ObservableCollection<string>();

        #region Add Category Command
        /// <summary>
        /// Command that handles adding of new categories to a list
        /// </summary>
        public RelayCommand AddCategoryCommand { get; private set; }

        /// <summary>
        /// Adds a category to the list of categories
        /// </summary>
        /// <param name="o"></param>
        public void AddCategory(object o)
        {
            Globals.Categories.Add(new Category(NewCategory, new List<string>()));
        }
        /// <summary>
        /// Defines if a categrory can be added
        /// </summary>
        /// <param name="o"></param>
        /// <returns>true if this category does not already exist in the list</returns>
        public bool CanAddCategory(object o)
        {
            return !Globals.Categories.Any(cat => cat.Name == NewCategory);
        }
        #endregion

        #region Remove Category Command
        /// <summary>
        /// Command that handles removing of caregories from a list
        /// </summary>
        public RelayCommand RemoveCategoryCommand { get; private set; }

        /// <summary>
        /// Removes a category from the list of categories
        /// </summary>
        /// <param name="o"></param>
        public void RemoveCategory(object o)
        {
            var cat = Globals.Categories.FirstOrDefault(c => c.Name == NewCategory);
            Globals.Categories.Remove(cat);
        }
        /// <summary>
        /// Defines if a categrory can be added
        /// </summary>
        /// <param name="o"></param>
        /// <returns>true if the category exists in the list</returns>
        public bool CanRemoveCategory(object o)
        {
            return Globals.Categories.Any(cat => cat.Name == NewCategory);
        }
        #endregion

        #region Assign Item Command
        /// <summary>
        /// Command that handles assignment of items to a category
        /// </summary>
        public RelayCommand AssignItemCommand { get; private set; }

        /// <summary>
        /// Adds an itemto a category
        /// </summary>
        /// <param name="o"></param>
        public void AssignItem(object o)
        {
            // add item to assigned items
            SelectedCategory.AttachedShops.Add(SelectedUnassigned);
            // remove from unassigned
            UnassignedItems.Remove(SelectedUnassigned);
        }
        /// <summary>
        /// Defines if an item can be assigned
        /// </summary>
        /// <param name="o"></param>
        /// <returns>true if there are any unassigned items</returns>
        public bool CanAssignItem(object o)
        {
            return SelectedUnassigned != null && SelectedCategory != null;
        }

        #endregion

        #region Unassign Item Command
        /// <summary>
        /// Command that handles removal of items from a category
        /// </summary>
        public RelayCommand UnassignItemCommand { get; private set; }

        /// <summary>
        /// removes an item from a category
        /// </summary>
        /// <param name="o"></param>
        public void UnassignItem(object o)
        {
            // add item to unassigned
            UnassignedItems.Add(SelectedAssigned);
            //remove item from assigned
            SelectedCategory.AttachedShops.Remove(SelectedAssigned);
            var ordered = UnassignedItems.OrderBy(ord => ord).ToList();
            UnassignedItems.Clear();
            foreach (var item in ordered)
            {
                UnassignedItems.Add(item);
            }
        }
        /// <summary>
        /// Defines if an item can be unassigned
        /// </summary>
        /// <param name="o"></param>
        /// <returns>true if there is any assigned item</returns>
        public bool CanUnassignItem(object o)
        {
            return SelectedAssigned != null && SelectedCategory != null;
        }
        #endregion

    }
}
