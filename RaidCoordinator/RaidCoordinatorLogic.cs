using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using RaidCoordinator.Data;

namespace RaidCoordinator
{
    public class RaidCoordinatorLogic
    {
        private static RaidCoordinatorLogic instance = new RaidCoordinatorLogic();

        public static RaidCoordinatorLogic Instance
        {
            get
            {
                if (instance == null)
                    instance = new RaidCoordinatorLogic();

                return instance;
            }
        }


        //public ObservableCollection<string> Raiders { get; }

        //public RaidCoordinatorLogic()
        //{
        //    Raiders = new ObservableCollection<string>();
        //    Raiders.CollectionChanged += OnListChanged;
        //}


        //public void Clear()
        //{
        //    Raiders.Clear();
        //}

        //private void OnListChanged(object sender, NotifyCollectionChangedEventArgs args)
        //{

        //}

        //public void AddRaiderToList(string username)
        //{
        //    Raiders.Add(username);
        //}
    }
}
