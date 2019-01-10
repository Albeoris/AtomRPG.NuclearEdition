using System;

namespace AtomRPG.NuclearEdition
{
    internal interface IInventoryOwner
    {
        Inventory Inventory { get; }
        Int32 MaxCarryWeight { get; }
        String DisplayName { get; }
    }
}