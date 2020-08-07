using InventoryManagement.Entities;
using InventoryManagement.Helpers;
using InventoryManagement.Models;
using InventoryManagement.SpecialClass;
using InventoryManagement.Windows;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
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
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        Uri English = new Uri(".\\Resources\\Resources.xaml", UriKind.Relative);
        Uri German = new Uri(".\\Resources\\Resources.de.xaml", UriKind.Relative);

        BackgroundWorker LoadInventories = new BackgroundWorker();

        public MainWindow()
        {
            SqlHelpers.GenerateNewDb();
            InitializeComponent();

            MainWindowModel context = new MainWindowModel();
            this.DataContext = context;

            InventoryList.ItemsSource = context.InventoryList;
            PrintList.ItemsSource = context.PrintList;

            InventoryList.IsEnabled = false;

            LoadInventories.WorkerReportsProgress = true;
            LoadInventories.DoWork += LoadInventories_DoWork;
            LoadInventories.ProgressChanged += LoadInventories_ProgressChanged;
            LoadInventories.RunWorkerCompleted += LoadInventories_RunWorkerCompleted;
            LoadInventories.RunWorkerAsync(NumberOfInventories());

            CollectionView view = CollectionViewSource.GetDefaultView(InventoryList.ItemsSource) as CollectionView;
            view.Filter = InventoryFilter;
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
                Inventory inventory = new Inventory { Number = i, InDate = DateTime.Now, Object = "Test" + i.ToString(), Price = new Random().NextDouble(), Repack = "" };
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
                var searchText = context.SearchText.Trim();
                var texts = searchText.Split(" ");
                bool number = false;
                bool inventoryName = false;
                foreach (var text in texts)
                {
                    number |= (item as Inventory).Number.ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
                    inventoryName |= (item as Inventory).Object.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
                }
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
            Inventory inventory = InventoryList.SelectedItem as Inventory;

            new EditInventory(inventory) { Owner = this }.ShowDialog();

            (this.DataContext as MainWindowModel).InventoryList.Clear();
            InventoryList.IsEnabled = false;
            LoadInventories.RunWorkerAsync(NumberOfInventories());
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

        private async void CommandAdd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var numbInventory = NumberOfInventories();
            var addInventoryWindow = await AddInventory.CreateAsync();
            addInventoryWindow.Owner = this;
            addInventoryWindow.ShowDialog();
            if (numbInventory != NumberOfInventories())
            {
                (this.DataContext as MainWindowModel).InventoryList.Clear();
                InventoryList.IsEnabled = false;
                LoadInventories.RunWorkerAsync(NumberOfInventories());
            }
        }

        private void AddToPrint_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = InventoryList?.SelectedItems.Count > 0;
        }

        private void AddToPrint_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindowModel context = this.DataContext as MainWindowModel;

            foreach (var inventory in InventoryList.SelectedItems)
            {
                if (context.PrintList.Contains(inventory) == false)
                    context.PrintList.Add(inventory as Inventory);
                else
                {
                    if (InventoryList.SelectedItems.Count < 2)
                    {
                        CustomMessageBox.ShowOK("Item has been added to the print list.", "Warning", "OK", MessageBoxImage.Warning);
                    }
                }
            }
            InventoryList.SelectedItems.Clear();
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow() { Owner = this }.ShowDialog();
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            SortAdorner.SortClick(sender, e, ref listViewSortCol, ref listViewSortAdorner, ref InventoryList);
        }

        private void ContextMenuCol_Click(object sender, RoutedEventArgs e)
        {
            MainWindowModel context = this.DataContext as MainWindowModel;
            string header = (sender as MenuItem).Header.ToString();
            string colName;
            bool value;
            using (var con = new SQLiteConnection(SqlHelpers.ConnectionString))
            {
                var updateCommand = con.CreateCommand();
                switch (header)
                {
                    case "Id":
                        colName = header;
                        value = context.ColumnIdVisible;
                        break;
                    case "Number":
                    case "Nummer":
                        colName = "Number";
                        value = context.ColumnNumberVisible;
                        break;
                    case "Object":
                        colName = "Object";
                        value = context.ColumnObjectVisible;
                        break;
                    case "InDate":
                        colName = "Income Date";
                        value = context.ColumnInDateVisible;
                        break;
                    case "Repack":
                        colName = "Repack";
                        value = context.ColumnRepackVisible;
                        break;
                    case "Price":
                    case "Preis":
                        colName = "Price";
                        value = context.ColumnPriceVisible;
                        break;
                    default:
                        colName = "Unknown";
                        value = true;
                        break;
                }
                int result = (value) ? 1 : 0;
                updateCommand.CommandText = $"UPDATE ListViewColVisible SET Boolean=@Result WHERE ColName=@Column";
                updateCommand.Parameters.AddWithValue("Result", result);
                updateCommand.Parameters.AddWithValue("Column", colName);
                updateCommand.ExecuteNonQuery();
                con.Close();
            }
        }

        #endregion

        private void RemoveFromPrint_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = PrintList?.SelectedItems.Count > 0;
        }

        private void RemoveFromPrint_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindowModel context = this.DataContext as MainWindowModel;
            var selectedItems = PrintList.SelectedItems.Cast<Inventory>().ToList();
            foreach (var inventory in selectedItems)
            {
                context.PrintList.Remove(inventory);
            }
        }

        private void ClearPrintList_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            MainWindowModel context = this.DataContext as MainWindowModel;
            if (context != null && context.PrintList.Count > 0)
                e.CanExecute = true;
        }

        private void ClearPrintList_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindowModel context = this.DataContext as MainWindowModel;
            context.PrintList.Clear();
        }

        private void PrintCheckList_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindowModel context = this.DataContext as MainWindowModel;
            string pdfPath;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "List Inventories";
            dialog.Filter = "PDF (*.pdf)|*.pdf";
            dialog.InitialDirectory = AppContext.BaseDirectory;
            if (dialog.ShowDialog(this) == true)
            {
                CreateInventoryPDFList(context.PrintList, dialog.FileName);
            }
        }

        private void CreateInventoryPDFList(ICollection<Inventory> inventories, string pdfPath)
        {
            var pdfTitle = "Inventory List";
            using (PdfWriter writer = new PdfWriter(pdfPath))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document document = new Document(pdf);
                    Paragraph paragraph = new Paragraph(pdfTitle)
                        .SetFontSize(20.0f)
                        .SetBold()
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);

                    float[] columnWidths = { 1, 3, 10, 0, 3 };
                    Table table = new Table(columnWidths);
                    table.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));

                    //
                    // add header cells
                    Cell[] cells = new Cell[columnWidths.Length];
                    for (int i = 0; i < cells.Length; i++)
                    {
                        cells[i] = new Cell();
                    }
                    Paragraph[] paragraphs = new Paragraph[columnWidths.Length];
                    for (int i = 0; i < paragraphs.Length; i++)
                    {
                        paragraphs[i] = new Paragraph().SetBold();
                    }
                    paragraphs[0].Add("ID");
                    paragraphs[1].Add("Number");
                    paragraphs[2].Add("Inventory");
                    paragraphs[3].Add("Incoming date");
                    paragraphs[4].Add("Price");

                    for (int i = 0; i < cells.Length; i++)
                    {
                        cells[i].Add(paragraphs[i]);
                    }
                    foreach (var cell in cells)
                    {
                        table.AddHeaderCell(cell);
                    }

                    //
                    // add list content
                    int counter = 0;
                    foreach (var inventory in inventories)
                    {
                        for (int i = 0; i < cells.Length; i++)
                        {
                            cells[i] = new Cell();
                        }
                        cells[0].Add(new Paragraph((++counter).ToString()));
                        cells[1].Add(new Paragraph(inventory.Number.ToString()));
                        cells[2].Add(new Paragraph(inventory.Object));
                        cells[3].Add(new Paragraph(inventory.InDate.ToShortDateString()));
                        cells[4].Add(new Paragraph(inventory.Price.ToString()));
                        foreach (var cell in cells)
                        {
                            table.AddCell(cell);
                        }
                    }

                    document.Add(paragraph);
                    document.Add(table);
                }
            }
            Process proc = new Process();
            proc.StartInfo.FileName = pdfPath;
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
        }
    }
}
