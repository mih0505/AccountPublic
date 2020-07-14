using System;

namespace AccountRPD.EventArguments
{
    public class PermissionCheckEventArgs : EventArgs
    {
        public bool CanEdit { get; set; }
        public bool CanRemove { get; set; }
    }
}
