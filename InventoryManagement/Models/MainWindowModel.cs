using InventoryManagement.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace InventoryManagement.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private string searchText;
        private string status;
        private int progress;
        private int inventoryCount;

        private bool columnNumberVisible = true;
        private bool columnObjectVisible = true;
        private bool columnInDateVisible = true;
        private bool columnPriceVisible = true;
        private bool columnRepackVisible = true;

        public string SearchText { get => searchText; set { if (value != searchText) { searchText = value; NotifyPropertyChanged(); } } }
        public string Status { get => status; set { if (value != status) { status = value; NotifyPropertyChanged(); } } }
        public int Progress { get => progress; set { if (value != progress) { progress = value; NotifyPropertyChanged(); } } }
        public int InventoryCount { get => inventoryCount; set { if (value != inventoryCount) { inventoryCount = value; NotifyPropertyChanged(); } } }

        public bool ColumnNumberVisible { get => columnNumberVisible; set { if (value != columnNumberVisible) { columnNumberVisible = value; NotifyPropertyChanged(); } } }
        public bool ColumnObjectVisible { get => columnObjectVisible; set { if (value != columnObjectVisible) { columnObjectVisible = value; NotifyPropertyChanged(); } } }
        public bool ColumnInDateVisible { get => columnInDateVisible; set { if (value != columnInDateVisible) { columnInDateVisible = value; NotifyPropertyChanged(); } } }
        public bool ColumnPriceVisible { get => columnPriceVisible; set { if (value != columnPriceVisible) { columnPriceVisible = value; NotifyPropertyChanged(); } } }
        public bool ColumnRepackVisible { get => columnRepackVisible; set { if (value != columnRepackVisible) { columnRepackVisible = value; NotifyPropertyChanged(); } } }



        public ObservableCollection<Inventory> InventoryList { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
