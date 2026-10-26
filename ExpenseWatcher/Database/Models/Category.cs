using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using sqla = SqliteAccessForExpenseWatcher;

namespace ExpenseWatcher
{
    /// <summary>
    /// class representing a category of <see cref="Payment"/>s (e.g. groceries, car)
    /// </summary>
    public class Category
    {
        private class CategoryAssignmentData
        {
            public List<string> Shops { get; set; } = new List<string>();
            public List<string> Infos { get; set; } = new List<string>();
        }

        #region Properties
        public string Name { get; set; }

        public ObservableCollection<string> AttachedShops { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> AttachedInfos { get; set; } = new ObservableCollection<string>();

        public string AttachedShopsJson
        {
            get
            {
                return JsonConvert.SerializeObject(new CategoryAssignmentData
                {
                    Shops = AttachedShops?.ToList() ?? new List<string>(),
                    Infos = AttachedInfos?.ToList() ?? new List<string>()
                });
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    AttachedShops = new ObservableCollection<string>();
                    AttachedInfos = new ObservableCollection<string>();
                    return;
                }

                try
                {
                    var assignmentData = JsonConvert.DeserializeObject<CategoryAssignmentData>(value);
                    if (assignmentData != null)
                    {
                        AttachedShops = new ObservableCollection<string>(assignmentData.Shops ?? new List<string>());
                        AttachedInfos = new ObservableCollection<string>(assignmentData.Infos ?? new List<string>());
                        return;
                    }
                }
                catch (JsonException)
                {
                    // legacy format fallback below
                }

                try
                {
                    var legacyShops = JsonConvert.DeserializeObject<List<string>>(value);
                    AttachedShops = new ObservableCollection<string>(legacyShops ?? new List<string>());
                    AttachedInfos = new ObservableCollection<string>();
                }
                catch (JsonException)
                {
                    AttachedShops = new ObservableCollection<string>();
                    AttachedInfos = new ObservableCollection<string>();
                }
            }
        }
        #endregion

        #region Constructors
        public Category() { }

        public Category(string name) : this(name, new List<string>()) { }

        public Category(string name, List<string> shops)
        {
            Name = name;
            AttachedShops = new ObservableCollection<string>(shops ?? new List<string>());
            AttachedInfos = new ObservableCollection<string>();
        }

        public Category(string name, string attachedShopsJson)
        {
            Name = name;
            AttachedShopsJson = attachedShopsJson;
        }
        #endregion

        public bool MatchesPayment(Payment payment)
        {
            if (payment == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(payment.Info) &&
                AttachedInfos.Any(info => string.Equals(info, payment.Info, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(payment.Shop) &&
                AttachedShops.Any(shop => string.Equals(shop, payment.Shop, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        public bool MatchesInfo(Payment payment)
        {
            return payment != null
                && !string.IsNullOrWhiteSpace(payment.Info)
                && AttachedInfos.Any(info => string.Equals(info, payment.Info, StringComparison.OrdinalIgnoreCase));
        }

        public bool MatchesShop(Payment payment)
        {
            return payment != null
                && !string.IsNullOrWhiteSpace(payment.Shop)
                && AttachedShops.Any(shop => string.Equals(shop, payment.Shop, StringComparison.OrdinalIgnoreCase));
        }

        public ObservableCollection<string> GetAssignedValues(bool useInfoAssignment)
        {
            return useInfoAssignment ? AttachedInfos : AttachedShops;
        }

        public sqla.Category ConvertToDbo()
        {
            return new sqla.Category()
            {
                AttachedShopsJson = this.AttachedShopsJson,
                Name = this.Name,
            };
        }

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
