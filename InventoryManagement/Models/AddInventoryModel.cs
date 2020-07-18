﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace InventoryManagement.Models
{
    class AddInventoryModel : INotifyPropertyChanged
    {
        private int number;
        private string objectName;
        private string inDate;
        private double price;
        private string repack;

        public int Number { get => number; set { if (value != number) { number = value; NotifyPropertyChanged(); } } }

        public string ObjectName { get => objectName; set { if (value != objectName) { objectName = value; NotifyPropertyChanged(); } } }

        public string InDate { get => inDate; set { if (value != inDate) { if (string.IsNullOrEmpty(value)) inDate = value; else inDate = DateTime.Parse(value).Date.ToShortDateString(); NotifyPropertyChanged(); } } }

        public double Price { get => price; set { if (value != price) { price = value; NotifyPropertyChanged(); } } }

        public string Repack { get => repack; set { if (value!= repack) { repack = value; NotifyPropertyChanged(); } } }


        public AddInventoryModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsNull()
        {
            return Number == 0 && string.IsNullOrEmpty(ObjectName) && string.IsNullOrEmpty(InDate);
        }
    }
}