using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace InventoryManagement.Models
{
    class AddInventoryModel : INotifyPropertyChanged
    {
        private int number;
        private string objectName;
        private DateTime inDate;
        private double price;
        private string repack;

        public int Number { get => number; set { if (value != number) { number = value; NotifyPropertyChanged(); } } }

        public string ObjectName { get => objectName; set { if (value != objectName) { objectName = value; NotifyPropertyChanged(); } } }

        //public DateTime InDate { get => inDate; set { if (value != inDate) { if (string.IsNullOrEmpty(value)) inDate = value; else inDate = DateTime.Parse(value).Date.ToShortDateString(); NotifyPropertyChanged(); } } }
        public DateTime InDate { get => inDate; set { if (value != inDate) { inDate = value; NotifyPropertyChanged(); } } }

        public double Price { get => price; set { if (value != price) { price = value; NotifyPropertyChanged(); } } }

        public string Repack { get => repack; set { if (value != repack) { repack = value; NotifyPropertyChanged(); } } }

        public ObservableCollection<int> ObjectNumbers { get; set; }

        public AddInventoryModel()
        {
            ObjectNumbers = new ObservableCollection<int>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsNull()
        {
            return Number == 0 || string.IsNullOrEmpty(ObjectName) || InDate == null;
        }
    }
}
