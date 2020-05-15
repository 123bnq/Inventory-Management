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
    }
}
