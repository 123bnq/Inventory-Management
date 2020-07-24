using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace InventoryManagement.SpecialClass
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand("Exit", "Exit", typeof(CustomCommands), new InputGestureCollection()
        {
            new KeyGesture(Key.Q, ModifierKeys.Control)
        });

        public static readonly RoutedUICommand Edit = new RoutedUICommand("Edit", "Edit", typeof(CustomCommands));

        public static readonly RoutedUICommand Add = new RoutedUICommand("Add", "Add", typeof(CustomCommands));

        public static readonly RoutedUICommand Delete = new RoutedUICommand("Delete", "Delete", typeof(CustomCommands), new InputGestureCollection()
        {
            new KeyGesture(Key.Delete)
        });

        public static readonly RoutedUICommand English = new RoutedUICommand("English", "English", typeof(CustomCommands));

        public static readonly RoutedUICommand German = new RoutedUICommand("German", "German", typeof(CustomCommands));

        public static readonly RoutedUICommand AddToPrint = new RoutedUICommand("AddToPrint", "AddToPrint", typeof(CustomCommands));

        public static readonly RoutedUICommand RemoveFromPrint = new RoutedUICommand("RemoveFromPrint", "RemoveFromPrint", typeof(CustomCommands));

        public static readonly RoutedUICommand ClearPrintList = new RoutedUICommand("ClearPrintList", "ClearPrintList", typeof(CustomCommands));

        public static readonly RoutedUICommand PrintCheckList = new RoutedUICommand("PrintCheckList", "PrintCheckList", typeof(CustomCommands));


        public static readonly RoutedUICommand AddInventoryBtn = new RoutedUICommand("AddInventoryBtn", "AddInventoryBtn", typeof(CustomCommands));
    }
}
