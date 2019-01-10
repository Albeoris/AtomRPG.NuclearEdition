using System;

namespace AtomRPG.NuclearEdition
{
    internal sealed class CharacterInventory : IInventoryOwner
    {
        private readonly CharacterComponent _comp;

        public Inventory Inventory => _comp.Character.Inventory;
        public Int32 MaxCarryWeight => _comp.Character.Stats.MaxCarryWeight;
        public String DisplayName => _comp.GetShortName();

        public CharacterInventory(CharacterComponent comp)
        {
            _comp = comp;
        }
    }
}