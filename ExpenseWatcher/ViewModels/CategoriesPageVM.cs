using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ViewModel;

namespace ExpenseWatcher.ViewModels
{
    public class CategoriesPageVM : BaseViewModel
    {
        public CategoriesPageVM()
        {
            AssignItemCommand = new RelayCommand(AssignItem, CanAssignItem);
            UnassignItemCommand = new RelayCommand(UnassignItem, CanUnassignItem);
            AddCategoryCommand = new RelayCommand(AddCategory, CanAddCategory);
            RemoveCategoryCommand = new RelayCommand(RemoveCategory, CanRemoveCategory);
            SaveCategoriesCommand = new RelayCommand(SaveCategory);

            Categories = Globals.Categories;
        }

        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        private bool _isInfoAssignmentMode = true;
        public bool IsInfoAssignmentMode
        {
            get => _isInfoAssignmentMode;
            set
            {
                if (_isInfoAssignmentMode != value)
                {
                    _isInfoAssignmentMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AssignmentToggleText));
                    OnPropertyChanged(nameof(UnassignedItems));
                    OnPropertyChanged(nameof(AssignedItems));
                    SelectedAssigned = AssignedItems.FirstOrDefault();
                    SelectedUnassigned = UnassignedItems.FirstOrDefault();
                }
            }
        }

        public string AssignmentToggleText => IsInfoAssignmentMode ? "Info" : "Empfänger";

        public ObservableCollection<string> UnassignedItems
        {
            get
            {
                var unassignedValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var payment in Globals.Payments)
                {
                    var value = IsInfoAssignmentMode ? payment.Info : payment.Shop;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        unassignedValues.Add(value);
                    }
                }

                foreach (var category in Categories)
                {
                    foreach (var item in category.GetAssignedValues(IsInfoAssignmentMode))
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            unassignedValues.Remove(item);
                        }
                    }
                }

                return new ObservableCollection<string>(
                    unassignedValues.OrderBy(item => item, StringComparer.OrdinalIgnoreCase));
            }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    _newCategory = _selectedCategory?.Name;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NewCategory));
                    OnPropertyChanged(nameof(AssignedItems));
                    OnPropertyChanged(nameof(UnassignedItems));
                    SelectedAssigned = AssignedItems.FirstOrDefault();
                    SelectedUnassigned = UnassignedItems.FirstOrDefault();
                }
            }
        }

        private string _newCategory;
        public string NewCategory
        {
            get => _newCategory;
            set
            {
                if (_newCategory != value)
                {
                    _newCategory = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> AssignedItems
        {
            get
            {
                var coll = new ObservableCollection<string>();
                if (SelectedCategory != null)
                {
                    foreach (var item in SelectedCategory.GetAssignedValues(IsInfoAssignmentMode))
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
            get => _selectedAssigned;
            set
            {
                if (_selectedAssigned != value)
                {
                    _selectedAssigned = value;
                    OnPropertyChanged();
                }
            }
        }

        public string _selectedUnassigned;
        public string SelectedUnassigned
        {
            get => _selectedUnassigned;
            set
            {
                if (_selectedUnassigned != value)
                {
                    _selectedUnassigned = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<string> GetCurrentAssignments(Category category)
        {
            return category == null ? new ObservableCollection<string>() : category.GetAssignedValues(IsInfoAssignmentMode);
        }

        private void RefreshCategoryLists()
        {
            OnPropertyChanged(nameof(AssignedItems));
            OnPropertyChanged(nameof(UnassignedItems));
            SelectedAssigned = AssignedItems.FirstOrDefault();
            SelectedUnassigned = UnassignedItems.FirstOrDefault();
        }

        public RelayCommand AddCategoryCommand { get; private set; }
        public void AddCategory(object o)
        {
            Globals.Categories.Add(new Category(NewCategory, new List<string>()));
            SelectedCategory = Globals.Categories.FirstOrDefault(cat => cat.Name == NewCategory);
        }

        public bool CanAddCategory(object o)
        {
            return !string.IsNullOrEmpty(NewCategory?.Trim())
                && !Globals.Categories.Any(cat => cat.Name == NewCategory);
        }

        public RelayCommand RemoveCategoryCommand { get; private set; }
        public void RemoveCategory(object o)
        {
            var cat = Globals.Categories.FirstOrDefault(c => c.Name == NewCategory);
            if (cat != null)
            {
                Globals.Categories.Remove(cat);
            }

            NewCategory = null;
            OnPropertyChanged(nameof(UnassignedItems));
            SelectedCategory = Categories.FirstOrDefault();
        }

        public bool CanRemoveCategory(object o)
        {
            return Globals.Categories.Any(cat => cat.Name == NewCategory);
        }

        public RelayCommand SaveCategoriesCommand { get; private set; }
        public void SaveCategory(object o)
        {
            Task.Run(() => DataBaseHelper.SaveCategoriesToDB());
        }

        public RelayCommand AssignItemCommand { get; private set; }
        public void AssignItem(object o)
        {
            if (SelectedCategory == null || string.IsNullOrWhiteSpace(SelectedUnassigned))
            {
                return;
            }

            var target = GetCurrentAssignments(SelectedCategory);
            if (!target.Any(item => string.Equals(item, SelectedUnassigned, StringComparison.OrdinalIgnoreCase)))
            {
                target.Add(SelectedUnassigned);
            }

            RefreshCategoryLists();
        }

        public bool CanAssignItem(object o)
        {
            return SelectedUnassigned != null && SelectedCategory != null;
        }

        public RelayCommand UnassignItemCommand { get; private set; }
        public void UnassignItem(object o)
        {
            if (SelectedCategory == null || string.IsNullOrWhiteSpace(SelectedAssigned))
            {
                return;
            }

            var target = GetCurrentAssignments(SelectedCategory);
            var toRemove = target.FirstOrDefault(item => string.Equals(item, SelectedAssigned, StringComparison.OrdinalIgnoreCase));
            if (toRemove != null)
            {
                target.Remove(toRemove);
            }

            RefreshCategoryLists();
        }

        public bool CanUnassignItem(object o)
        {
            return SelectedAssigned != null && SelectedCategory != null;
        }
    }
}
