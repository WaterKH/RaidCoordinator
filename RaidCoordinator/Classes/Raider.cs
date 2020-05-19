using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaidCoordinator
{
    public class Raider
    {
        public string Username { get; set; }
        public bool IsAvailable { get; set; }
        public bool HasSpawnedBoss { get; set; }
    }
}
