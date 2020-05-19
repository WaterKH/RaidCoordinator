using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaidCoordinator
{
    public delegate void BoostersAddedDelegate(object sender, BoostersAddedEventArgs args);

    public class BoostersAddedEventArgs : EventArgs
    {
        public string Value { get; }

        public BoostersAddedEventArgs(string value)
        {
            this.Value = value;
        }
    }
}
