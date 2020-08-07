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

        public DateTime InDate { get; set; }

        public double Price { get; set; }

        public string Repack { get; set; }

        public override bool Equals(object obj)
        {
            Inventory inventory;
            try
            {
                inventory = obj as Inventory;
                if (inventory == null)
                    return false;

                return this.Number == inventory.Number && this.Object.Equals(inventory.Object) && this.InDate.Equals(inventory.InDate) && this.Price == inventory.Price && this.Repack.Equals(inventory.Repack);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
