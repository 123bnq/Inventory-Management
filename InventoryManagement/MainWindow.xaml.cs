using InventoryManagement.Entities;
using InventoryManagement.Helpers;
using InventoryManagement.Models;
using InventoryManagement.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WPFCustomMessageBox;

namespace InventoryManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Uri English = new Uri(".\\Resources\\Resources.xaml", UriKind.Relative);
        Uri German = new Uri(".\\Resources\\Resources.de.xaml", UriKind.Relative);

        BackgroundWorker LoadInventories = new BackgroundWorker();

        public MainWindow()
        {
            SqlHelpers.GenerateNewDb();
            InitializeComponent();

            MainWindowModel context = new MainWindowModel();
            this.DataContext = context;

            context.InventoryList = new ObservableCollection<Inventory>();
            InventoryList.ItemsSource = context.InventoryList;

            InventoryList.IsEnabled = false;

            LoadInventories.WorkerReportsProgress = true;
            LoadInventories.DoWork += LoadInventories_DoWork;
            LoadInventories.ProgressChanged += LoadInventories_ProgressChanged;
            LoadInventories.RunWorkerCompleted += LoadInventories_RunWorkerCompleted;
            LoadInventories.RunWorkerAsync(NumberOfInventories());

            CollectionView view = CollectionViewSource.GetDefaultView(InventoryList.ItemsSource) as CollectionView;
            view.Filter = InventoryFilter;

            //LoadInventoryAsync().Wait();

        }

        private void LoadInventories_DoWork(object sender, DoWorkEventArgs e)
        {
            var max = (int)e.Argument;
            int item = 0;

            using (var connection = new SQLiteConnection(SqlHelpers.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT [Id], [Number], [Object], [Incoming Date], [Repack], [Price] FROM [Inventory]";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var inventory = SqlHelpers.ReadInventoryFromDb(reader);
                            
                            item++;
                            var progressPercentage = Convert.ToInt32(((double)item / max) * 100);
                            (sender as BackgroundWorker).ReportProgress(progressPercentage, inventory);
                            Thread.Sleep(1);
                        }
                        reader.Close();
                    }
                }
                e.Result = e.Argument;
                connection.Close();
            }
        }

        private void LoadInventories_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MainWindowModel context = this.DataContext as MainWindowModel;
            context.Progress = e.ProgressPercentage;
            context.Status = "Loading...";
            if (e.UserState != null)
            {
                context.InventoryList.Add(e.UserState as Inventory);
            }
        }

        private void LoadInventories_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MainWindowModel context = this.DataContext as MainWindowModel;
            context.Status = "Finished";
            context.InventoryCount = (int)e.Result;

            InventoryList.IsEnabled = true;
        }

        private async Task GenerateInvetoriesToDbAsync()
        {
            List<Inventory> inventories = new List<Inventory>();
            for (int i = 0; i < 100; i++)
            {
                Inventory inventory = new Inventory { Number = i, InDate = DateTime.Now.ToString("d"), Object = "Test" + i.ToString(), Price = new Random().NextDouble(), Repack = "" };
                inventories.Add(inventory);
            }

            using (var connection = new SQLiteConnection(SqlHelpers.ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    await Task.Run(() =>
                    {
                        foreach (var inv in inventories)
                        {
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = "INSERT INTO Inventory ([Number], [Object], [Incoming Date], [Repack], [Price]) VALUES (@Number, @Object, @InDate, @Repack, @Price)";
                                cmd.Parameters.AddWithValue("Number", inv.Number);
                                cmd.Parameters.AddWithValue("Object", inv.Object);
                                cmd.Parameters.AddWithValue("InDate", inv.InDate);
                                cmd.Parameters.AddWithValue("Repack", inv.Repack);
                                cmd.Parameters.AddWithValue("Price", inv.Price);
                                cmd.ExecuteNonQuery();
                                Task.Delay(TimeSpan.FromTicks(5));
                            }
                        }
                        transaction.Commit();
                    });
                }
                connection.Close();
            }
        }

        private async Task<int> DeleteInventory(IEnumerable<int> inventoryIds)
        {
            int result;
            using (var connection = new SQLiteConnection(SqlHelpers.ConnectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $"DELETE FROM [Inventory] WHERE Id IN ({SqlHelpers.GenerateParamStringFromList(inventoryIds)})";
                    cmd.Parameters.AddRange(SqlHelpers.GenerateSQLParamFromList(inventoryIds).ToArray());
                    result = await cmd.ExecuteNonQueryAsync();
                }
            }

            return result;
        }

        private int NumberOfInventories()
        {
            int itemsCount;

            using (var connection = new SQLiteConnection(SqlHelpers.ConnectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Count(*) FROM Inventory";
                    var reader = cmd.ExecuteReader();
                    reader.Read();
                    int.TryParse(Convert.ToString(reader["Count(*)"]), out itemsCount);
                }
            }
            return itemsCount;
        }

        private bool InventoryFilter(object item)
        {
            var context = this.DataContext as MainWindowModel;
            if (string.IsNullOrWhiteSpace(context.SearchText))
            {
                return true;
            }
            else
            {
                bool number = (item as Inventory).Number.ToString().IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                bool inventoryName = (item as Inventory).Object.IndexOf(context.SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                return (number || inventoryName);
            }
        }

        #region EventHandlers

        #region Commands
        private void CommandExit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandExit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CommandEdit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InventoryList.SelectedItem != null;
        }

        private void CommandEdit_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private async void CommandDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (InventoryList.SelectedItems.Count > 0)
            {
                if (CustomMessageBox.ShowYesNo($"Do you want to delete {InventoryList.SelectedItems.Count} {(InventoryList.SelectedItems.Count > 1 ? "items" : "item")}?", "Warning", "Yes", "No", MessageBoxImage.Warning).Equals(MessageBoxResult.Yes))
                {
                    IEnumerable<int> ids;
                    List<Inventory> inventories = new List<Inventory>();

                    foreach (var inventory in InventoryList.SelectedItems)
                    {
                        inventories.Add(inventory as Inventory);
                    }

                    ids = inventories.Select(inventory => inventory.Id.Value);

                    var res = await DeleteInventory(ids);
                    if (res > 0)
                    {
                        foreach (var inventory in inventories)
                        {
                            (this.DataContext as MainWindowModel).InventoryList.Remove(inventory);
                        }
                    }
                }
            }
        }

        private void CommandEnglish_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandEnglish_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = English;
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        private void CommandGerman_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = German;
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        private void CommandGerman_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandAdd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var numbInventory = NumberOfInventories();
            new AddInventory() { Owner = this }.ShowDialog();
            if (numbInventory != NumberOfInventories())
            {
                (this.DataContext as MainWindowModel).InventoryList.Clear();
                LoadInventories.RunWorkerAsync(NumberOfInventories()); 
            }
        }
        #endregion

        private void InventoryList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(ListBoxItem))
            {
                InventoryList.SelectedItem = null;
            }
        }

        private async void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            await GenerateInvetoriesToDbAsync();
            (this.DataContext as MainWindowModel).InventoryList.Clear();
            InventoryList.IsEnabled = false;
            LoadInventories.RunWorkerAsync(NumberOfInventories());
        }

        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainWindowModel).InventoryList.Clear();
            InventoryList.IsEnabled = false;
            LoadInventories.RunWorkerAsync(NumberOfInventories());
        }
        
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(InventoryList.ItemsSource).Refresh();
        }

        #endregion
    }
}
