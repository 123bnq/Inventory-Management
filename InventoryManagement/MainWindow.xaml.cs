using InventoryManagement.Entities;
using InventoryManagement.Helpers;
using InventoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventoryManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Uri English = new Uri(".\\Resources\\Resources.xaml", UriKind.Relative);
        Uri German = new Uri(".\\Resources\\Resources.de.xaml", UriKind.Relative);

        public MainWindow()
        {
            SqlHelpers.GenerateNewDb();
            InitializeComponent();

            MainWindowModel context = new MainWindowModel();
            this.DataContext = context;

            context.InventoryList = new ObservableCollection<Inventory> { new Inventory { Number = 1, InDate = DateTime.UtcNow.ToString(), Object = "Test inventory", Price = 2.2, Repack = "" } };
            InventoryList.ItemsSource = context.InventoryList;
            LoadInventoryAsync().Wait();

        }

        private async Task LoadInventoryAsync()
        {
            using (var connection = new SQLiteConnection(SqlHelpers.ConnectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT [Id], [Number], [Object], [Incoming Date], [Repack], [Price] FROM [Inventory]";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var inventory = SqlHelpers.ReadInventoryFromDb(reader);
                            ((MainWindowModel)this.DataContext).InventoryList.Add(inventory);
                        }
                        reader.Close();
                    }
                }
                connection.Close();
            }
        }

        private async Task GenerateInvetoriesToDbAsync()
        {
            List<Inventory> inventories = new List<Inventory>();
            for (int i = 0; i < 100000; i++)
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

        private async Task<int> DeleteInventory(List<int> inventoryIds)
        {
            int result;
            string ids = string.Join(", ", inventoryIds);
            using (var connection = new SQLiteConnection(SqlHelpers.ConnectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Inventory WHERE Id in (@Ids)";
                    cmd.Parameters.AddWithValue("Ids", ids);
                    result = await cmd.ExecuteNonQueryAsync();
                }
            }

            return result;
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
            await LoadInventoryAsync();
        }

        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            LoadInventoryAsync().Wait();
        }
        #endregion
    }
}
