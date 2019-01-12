using System;
using System.Collections.Generic;

namespace AtomRPG.NuclearEdition
{
    internal sealed class PartyItem
    {
        public ItemProto Proto { get; }
        public Int32 Count { get; private set; }
        public Int32 TotalWeight => Count * Proto.Weight;
        private readonly LinkedList<InventoryItem> _links = new LinkedList<InventoryItem>();

        public PartyItem(ItemProto proto)
        {
            Proto = proto;
        }

        public void Enqueue(InventoryItem pair)
        {
            String itemPrototype = pair.Item.Prototype.name;
            String expectedPrototype = Proto.name;
            if (itemPrototype != expectedPrototype)
                throw new ArgumentException($"[{ToString()}]Invalid item {itemPrototype}. Expected: {expectedPrototype}", nameof(pair));

            _links.AddLast(pair);
            Count += pair.Item.Count;
        }

        public List<KeyValuePair<InventoryItem, Int32>> Dequeue(Int32 dequeueCount)
        {
            List<KeyValuePair<InventoryItem, Int32>> result = new List<KeyValuePair<InventoryItem, Int32>>();

            while (dequeueCount > 0)
            {
                if (_links.Count == 0)
                    throw new InvalidOperationException($"[{ToString()}] Unexpected end of list.");

                InventoryItem pair = _links.First.Value;

                Int32 itemCount;
                if (dequeueCount >= pair.Item.Count)
                {
                    itemCount = pair.Item.Count;
                    dequeueCount -= itemCount;
                    _links.RemoveFirst();
                }
                else
                {
                    itemCount = dequeueCount;
                    dequeueCount = 0;
                }

                Count -= itemCount;
                result.Add(new KeyValuePair<InventoryItem, Int32>(pair, itemCount));
            }

            return result;;
        }

        public Item Peek()
        {
            if (_links.Count == 0 || Count == 0)
                throw new InvalidOperationException($"[{ToString()}] Collection is empty.");

            return _links.First.Value.Item;
        }

        public override String ToString()
        {
            return $"{Proto.Caption} ({Proto.name})";
        }
    }
}