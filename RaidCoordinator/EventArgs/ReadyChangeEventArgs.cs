using System;

namespace RaidCoordinator
{
    public delegate void ReadyChangeDelegate(object sender, ReadyChangeEventArgs args);

    public class ReadyChangeEventArgs : EventArgs
    {
        public bool Value { get; }

        public ReadyChangeEventArgs(bool value)
        {
            this.Value = value;
        }
    }
}
