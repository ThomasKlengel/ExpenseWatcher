using System.Collections.ObjectModel;
using System.Linq;
using ViewModel;

namespace ExpanseWatcher.ViewModels
{
    public class CategoriesPageVM : BaseViewModel
    {
        public CategoriesPageVM()
        {
            AssignedItems = new ObservableCollection<string>() { "Assigned1", "Assigned2", "Assigned3" };
            UnassignedItems = new ObservableCollection<string>() { "unAssigned1", "unAssigned2", "unAssigned3" };
            Categories = new ObservableCollection<string>() { "Cat1", "Cat2", "Cat3" };

            AssignItemCommand = new RelayCommand(AssignItem, CanAssignItem);
            UnassignItemCommand = new RelayCommand(UnassignItem, CanUnassignItem);
            AddCategoryCommand = new RelayCommand(AddCategory, CanAddCategory);
            RemoveCategoryCommand = new RelayCommand(RemoveCategory, CanRemoveCategory);
        }

        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();
        private string _selectedCategory;
        public string SelectedCategory
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
                    _newCategory = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(NewCategory));
                }
            }
        }

        private string _newCategory;
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

        public ObservableCollection<string> AssignedItems { get; set; } = new ObservableCollection<string>();
        private int _selectedIndexAssigned;
        public int SelectedIndexAssigned
        {
            get
            {
                return _selectedIndexAssigned;
            }
            set
            {
                if (_selectedIndexAssigned != value)
                {
                    _selectedIndexAssigned = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> UnassignedItems { get; set; } = new ObservableCollection<string>();
        private int _selectedIndexUnassigned;
        public int SelectedIndexUnassigned
        {
            get
            {
                return _selectedIndexUnassigned;
            }
            set
            {
                if (_selectedIndexUnassigned != value)
                {
                    _selectedIndexUnassigned = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public RelayCommand AddCategoryCommand { get; private set; }
        public RelayCommand RemoveCategoryCommand { get; private set; }
        public RelayCommand AssignItemCommand { get; private set; }
        public RelayCommand UnassignItemCommand { get; private set; }

        public void AssignItem(object o)
        {
            AssignedItems.Add(UnassignedItems.ElementAt(SelectedIndexUnassigned));
            UnassignedItems.RemoveAt(SelectedIndexUnassigned);
            SelectedIndexUnassigned = 0;
        }
        public bool CanAssignItem(object o)
        {
            return UnassignedItems.Count > 0;
        }

        public void UnassignItem(object o)
        {
            UnassignedItems.Add(AssignedItems.ElementAt(SelectedIndexAssigned));
            AssignedItems.RemoveAt(SelectedIndexAssigned);
            SelectedIndexAssigned = 0;
        }
        public bool CanUnassignItem(object o)
        {
            return AssignedItems.Count > 0;
        }

        public void AddCategory (object o)
        {
                Categories.Add(NewCategory);            
        }
        public bool CanAddCategory(object o)
        {
            return !Categories.Contains(NewCategory);
        }

        public void RemoveCategory(object o)
        {
            Categories.Remove(NewCategory);
        }
        public bool CanRemoveCategory(object o)
        {
            return Categories.Contains(NewCategory);
        }
    }
}
