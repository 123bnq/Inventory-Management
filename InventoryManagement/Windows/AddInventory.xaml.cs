using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using InventoryManagement.Entities;
using InventoryManagement.Helpers;
using InventoryManagement.Models;
using WPFCustomMessageBox;

namespace InventoryManagement.Windows
{
    /// <summary>
    /// Interaction logic for AddInventory.xaml
    /// </summary>
    public partial class AddInventory : Window
    {
        public AddInventory()
        {
            InitializeComponent();

            AddInventoryModel context = new AddInventoryModel();
            this.DataContext = context;
        }

        private async void AddBtnClick(object sender, RoutedEventArgs e)
        {
            AddInventoryModel context = this.DataContext as AddInventoryModel;
            if (context.IsNull())
            {
                CustomMessageBox.ShowOK("Not enough information for inventory. Please try again.", "Error", "OK", MessageBoxImage.Error);
                return;
            }

            Inventory inventory = GetInventoryFromContext();

            using (SQLiteConnection connection = new SQLiteConnection(SqlHelpers.ConnectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Number FROM Inventory WHERE Number = @Number";
                    cmd.Parameters.AddWithValue("Number", inventory.Number);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            CustomMessageBox.ShowOK("The Inventory has already existed", "Error", "OK", MessageBoxImage.Error);
                            return;
                        }
                    }

                    cmd.CommandText = "INSERT INTO Inventory ([Number], [Object], [Incoming Date], [Repack], [Price]) VALUES (@Number, @Object, @InDate, @Repack, @Price)";
                    cmd.Parameters.AddWithValue("Number", inventory.Number);
                    cmd.Parameters.AddWithValue("Object", inventory.Object);
                    cmd.Parameters.AddWithValue("InDate", inventory.InDate);
                    cmd.Parameters.AddWithValue("Repack", inventory.Repack);
                    cmd.Parameters.AddWithValue("Price", inventory.Price);

                    var res = await cmd.ExecuteNonQueryAsync();

                    if (res != -1)
                    {
                        CustomMessageBox.ShowOK("Inventory added successfully.", "Information", "OK", MessageBoxImage.Information);
                        return;
                    }

                    CustomMessageBox.ShowOK("Something wrong while adding inventory. Please try again later.", "Error", "OK", MessageBoxImage.Error);
                }
            }

        }

        private void ClearBtnClick(object sender, RoutedEventArgs e)
        {
            AddInventoryModel context = this.DataContext as AddInventoryModel;
            context.Number = 0;
            context.ObjectName = context.InDate = context.Repack = string.Empty;
            context.Price = 0;
        }

        private Inventory GetInventoryFromContext()
        {
            AddInventoryModel context = this.DataContext as AddInventoryModel;
            return new Inventory
            {
                Number = context.Number,
                Object = context.ObjectName,
                InDate = context.InDate,
                Price = context.Price,
                Repack = context.Repack
            };
        }

        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
