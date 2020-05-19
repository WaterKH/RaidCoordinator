using System;

namespace RaidCoordinator
{
    public delegate void RaidersChangeDelegate(object sender, RaidersChangeEventArgs args);

    public class RaidersChangeEventArgs : EventArgs
    {
        public Raider Value { get; }

        public RaidersChangeEventArgs(Raider value)
        {
            this.Value = value;
        }
    }
}
