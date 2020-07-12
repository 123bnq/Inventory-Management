using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryManagement.Entities
{
    public class Inventory
    {
        public int? Id { get; set; }

        public int Number { get; set; }

        public string Object { get; set; }

        public string InDate { get; set; }

        public double Price { get; set; }

        public string Repack { get; set; }
    }
}
