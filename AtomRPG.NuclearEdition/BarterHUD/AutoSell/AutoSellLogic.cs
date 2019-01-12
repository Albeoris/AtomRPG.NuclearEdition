using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class AutoSellLogic
    {
        public const Int32 MinimalBudget = 20;

        private readonly BarterHUD_Proxy _barterHUD;
        private readonly PartyInventory _inventory;
        private readonly PartyItemsCost _itemsCost;

        public AutoSellLogic(BarterHUD_Proxy barterHUD, IEnumerable<Inventory> inventories)
        {
            _barterHUD = barterHUD;
            _inventory = new PartyInventory(inventories);
            _itemsCost = new PartyItemsCost(_barterHUD);
        }

        public static Int32 GetBudget(BarterHUD_Proxy hud)
        {
            Int32 rightCost = hud.CalcBuyCost(hud.RightTradeInventory);
            Int32 leftCost = hud.CalcSellCost(hud.LeftTradeInventory);
            return rightCost - leftCost;
        }

        public void Sell()
        {
            Boolean somethingSelled = false;

            List<PartyItem> items = _inventory.GetItems();

            Int32 budget = AutoSellLogic.GetBudget(_barterHUD);
            while (budget > MinimalBudget)
            {
                SortByWeightAscending(items);

                Int32 previousValue = budget;

                for (Int32 index = items.Count - 1; index >= 0; index--)
                {
                    PartyItem item = items[index];
                    Int32 cost = _itemsCost.GetSingleItemCost(item);
                    if (budget - cost < 0)
                        continue;

                    Int32 count = AutoSellAnalyzer.Instance.GetCountToSell(_inventory, item);
                    if (count < 1)
                        continue;

                    Int32 maxCount = budget / cost;
                    count = Math.Min(count, maxCount);

                    somethingSelled = true;
                    SellItem(item, count);

                    Int32 totalCost = count * cost;
                    if (budget > totalCost * 1.5)
                    {
                        budget -= count * cost;
                    }
                    else
                    {
                        // Recheck budget to avoid error due to rounding discounts
                        budget = AutoSellLogic.GetBudget(_barterHUD);
                    }

                    if (item.Count == 0)
                        items.RemoveAt(index);

                    if (budget <= 0)
                        break;
                }

                if (previousValue == budget)
                    break;
            }

            if (somethingSelled)
            {
                _barterHUD.Instance.Invalidate();
                _barterHUD.Instance.ShowBackpackWithCost(_barterHUD.LeftBackpack, _barterHUD.LeftInventory);
            }

        }

        private static void SortByWeightAscending(List<PartyItem> items)
        {
            items.Sort((x, y) => x.TotalWeight.CompareTo(y.TotalWeight));
        }

        private void SellItem(PartyItem item2, Int32 count2)
        {
            if (count2 > item2.Count)
            {
                Debug.LogError($"[NuclearEdition] Trying to sell more items than we have. Item: {item2}, Have: {item2.Count}, Want to Sell: {count2}");
                return;
            }

            List<KeyValuePair<InventoryItem, Int32>> itemsAndNumbers = item2.Dequeue(count2);
            foreach (var item in itemsAndNumbers)
                SellItem(item.Key, item.Value);
        }

        private void SellItem(InventoryItem invItem, Int32 sellCount)
        {
            Inventory inventory = invItem.Inventory;
            Item item = invItem.Item;

            if (sellCount < item.Count)
            {
                item.Count -= sellCount;

                Item itemClone = item.Clone();
                itemClone.Count = sellCount;

                DropItemToTradeZone(itemClone);
            }
            else
            {
                inventory.Remove(item);
                DropItemToTradeZone(item);
            }
        }

        private void DropItemToTradeZone(Item itemClone)
        {
            _barterHUD.DropItemToBackpack(_barterHUD.LeftTradeBackpack, _barterHUD.LeftTradeInventory, itemClone);
        }
    }
}