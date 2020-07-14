using AccountRPD.Enums;
using System;

namespace AccountRPD.EventArguments
{
    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionTypes SelectionType { get; }

        public SelectionChangedEventArgs(SelectionTypes selectionType)
        {
            SelectionType = selectionType;
        }
    }
}
