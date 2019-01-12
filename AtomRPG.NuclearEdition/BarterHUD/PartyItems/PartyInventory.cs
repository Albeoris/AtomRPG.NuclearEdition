using System;
using System.Collections.Generic;

namespace AtomRPG.NuclearEdition
{
    internal sealed class PartyInventory
    {
        private readonly Dictionary<ItemProto, PartyItem> _items = new Dictionary<ItemProto, PartyItem>();

        public PartyInventory(IEnumerable<Inventory> inventories)
        {
            foreach (Inventory inventory in inventories)
                Add(inventory);
        }

        public void Add(Inventory inventory)
        {
            foreach (Item item in inventory.GetItems())
            {
                ItemProto proto = item.Prototype;
                InventoryItem pair = new InventoryItem(inventory, item);

                if (!_items.TryGetValue(proto, out var partyItem))
                {
                    partyItem = new PartyItem(proto);
                    _items.Add(proto, partyItem);
                }

                partyItem.Enqueue(pair);
            }
        }

        public List<PartyItem> GetItems()
        {
            List<PartyItem> result = new List<PartyItem>(_items.Count);
            List<ItemProto> remove = new List<ItemProto>();

            foreach (var item in _items.Values)
            {
                if (item.Count > 0)
                    result.Add(item);
                else
                    remove.Add(item.Proto);
            }

            foreach (ItemProto proto in remove)
                _items.Remove(proto);

            return result;
        }

        public IEnumerable<PartyItem> Enumerate(Predicate<PartyItem> predicate)
        {
            foreach (var pair in _items)
            {
                if (predicate(pair.Value))
                    yield return pair.Value;
            }
        }
    }
}