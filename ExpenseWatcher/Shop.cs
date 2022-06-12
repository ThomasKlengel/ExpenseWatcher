using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseWatcher
{
    public class Shop
    {   
        public Shop (string name)
        {
            Name = name;
        }


        [AutoIncrement][PrimaryKey]
        public int Index { get; set; }
        public string Name { get; set; }
    }
}
