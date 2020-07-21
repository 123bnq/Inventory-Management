using InventoryManagement.Entities;
using InventoryManagement.Models;
using System;
using System.Collections.Generic;
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
            var res = CustomMessageBox.ShowYesNo("Do you want to cancel the operation? All changes will not be saved.", "Warning", "Yes", "No", MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

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
        }
    }
}
