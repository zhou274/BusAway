using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TJ.Scripts
{
    public class ParkingManager : MonoBehaviour
    {
        public static ParkingManager instance;
        public List<ParkingSlots> slots;
        public List<Vehicle> parkedVehicles;
        public ParkingSlots parkingSlot_Rv;

        public Transform exitPoint;

        // Start is called before the first frame update
        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            //slots = FindObjectsOfType<ParkingSlots>().ToList();
        }
        
        public ParkingSlots CheckForFreeSlot()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].isOccupied)
                {
                    return slots[i];
                }
            }
            return null;
        }
        /*public bool GetSlot(Vehicle veh)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].isOccupied)
                {
                    //MoveToSlot(slots[i], veh);
                    return true;
                }
            }
            return false;
        }
   
        public bool CheckIfAllOccupied()
        {
            int occupiedCount = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].isOccupied)
                {
                    occupiedCount++;
                }
            }

            if (occupiedCount == slots.Count)
            {
                return true;
            }

            return false;
        }

        public void findOccupiedSlots()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].isOccupied)
                {
                    occupiedSlots.Add(slots[i]);
                }
            }
        }*/
    }
}