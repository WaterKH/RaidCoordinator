using System;

namespace RaidCoordinator
{
    public delegate void BoostersAddedDelegate(object sender, BoostersAddedEventArgs args);

    public class BoostersAddedEventArgs : EventArgs
    {
        public Booster Value { get; }

        public BoostersAddedEventArgs(Booster value)
        {
            this.Value = value;
        }
    }
}
