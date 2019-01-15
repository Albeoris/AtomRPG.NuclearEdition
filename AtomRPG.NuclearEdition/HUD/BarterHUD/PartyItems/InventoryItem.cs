namespace AtomRPG.NuclearEdition
{
    internal sealed class InventoryItem
    {
        public Item Item { get; }
        public Inventory Inventory { get; }

        public InventoryItem(Inventory inventory, Item item)
        {
            Inventory = inventory;
            Item = item;
        }
    }
}