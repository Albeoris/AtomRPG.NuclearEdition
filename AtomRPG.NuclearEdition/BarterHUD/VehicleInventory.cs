using System;

namespace AtomRPG.NuclearEdition
{
    internal sealed class VehicleInventory : IInventoryOwner
    {
        private readonly Game.VehicleData _vehicle;

        public Inventory Inventory => _vehicle.Inventory;
        public Int32 MaxCarryWeight => -1;
        public String DisplayName => _vehicle.proto;

        public VehicleInventory(Game.VehicleData vehicle)
        {
            _vehicle = vehicle;
        }
    }
}