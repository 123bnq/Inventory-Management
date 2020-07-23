using InventoryManagement.Entities;
using InventoryManagement.Helpers;
using InventoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFCustomMessageBox;

namespace InventoryManagement.Windows
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditInventory : Window
    {
        Inventory OldInventory = new Inventory();
        private int? InventoryId = null;
        public EditInventory(Inventory inventory)
        {
            InitializeComponent();

            EditInventoryModel context = new EditInventoryModel();
            this.DataContext = context;
            PutInventoryToContext(inventory);
            OldInventory = inventory;
        }

        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            Inventory newInventory = GetInventoryFromContext();

            if (OldInventory.Equals(newInventory))
            {
                this.Close();
                return;
            }

            var res = CustomMessageBox.ShowYesNo("Do you want to cancel the operation? All changes will not be saved.", "Warning", "Yes", "No", MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                this.Close();
            }

        }

        private void UndoBtnClick(object sender, RoutedEventArgs e)
        {
            PutInventoryToContext(OldInventory);
        }

        private async void SaveBtnClick(object sender, RoutedEventArgs e)
        {
            var newInventory = GetInventoryFromContext();

            using (SQLiteConnection connection = new SQLiteConnection(SqlHelpers.ConnectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "UPDATE Inventory SET [Number]=@Number, [Object]=@Object, [Incoming Date]=@InDate, [Repack]=@Repack, [Price]=@Price WHERE Id=@Id";
                    cmd.Parameters.AddWithValue("Number", newInventory.Number);
                    cmd.Parameters.AddWithValue("Object", newInventory.Object);
                    cmd.Parameters.AddWithValue("InDate", newInventory.InDate);
                    cmd.Parameters.AddWithValue("Price", newInventory.Price);
                    cmd.Parameters.AddWithValue("Repack", newInventory.Repack);

                    cmd.Parameters.AddWithValue("Id", InventoryId.Value);

                    var result = await cmd.ExecuteNonQueryAsync();
                    if (result != -1)
                    {
                        OldInventory = GetInventoryFromContext();

                        var res = CustomMessageBox.ShowOK("Inventory added successfully.", "Information", "OK", MessageBoxImage.Information);
                        if (res == MessageBoxResult.OK || res == MessageBoxResult.None)
                        {
                            this.Close();
                        }
                        return;
                    }

                    CustomMessageBox.ShowOK("Something wrong while adding inventory. Please try again later.", "Error", "OK", MessageBoxImage.Error);
                }
            }


        }

        private Inventory GetInventoryFromContext()
        {
            EditInventoryModel context = this.DataContext as EditInventoryModel;
            return new Inventory
            {
                Number = context.Number,
                Object = context.ObjectName,
                InDate = context.InDate,
                Price = context.Price,
                Repack = context.Repack
            };
        }

        private void PutInventoryToContext(Inventory inventory)
        {
            EditInventoryModel context = this.DataContext as EditInventoryModel;
            context.Number = inventory.Number;
            context.ObjectName = inventory.Object;
            context.InDate = inventory.InDate;
            context.Price = inventory.Price;
            context.Repack = inventory.Repack;
            if(InventoryId == null)
            {
                InventoryId = inventory.Id;
            }
        }
    }
}
