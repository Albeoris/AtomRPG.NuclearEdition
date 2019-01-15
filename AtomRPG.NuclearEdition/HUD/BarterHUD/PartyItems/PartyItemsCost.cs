using System;
using System.Collections.Generic;

namespace AtomRPG.NuclearEdition
{
    internal sealed class PartyItemsCost
    {
        private readonly BarterHUD_Proxy _barterHUD;

        private readonly Int32 _sellDiscount;
        private readonly Dictionary<ItemProto, Int32> _cost = new Dictionary<ItemProto, Int32>();

        public PartyItemsCost(BarterHUD_Proxy barterHUD)
        {
            _barterHUD = barterHUD;
            _sellDiscount = _barterHUD.GetSellDiscount();
        }

        public Int32 GetSingleItemCost(PartyItem item)
        {
            if (!_cost.TryGetValue(item.Proto, out var cost))
            {
                cost = CalcItemCost(item);
                _cost.Add(item.Proto, cost);
            }

            return cost;
        }

        private Int32 CalcItemCost(PartyItem partyItem)
        {
            Item item = partyItem.Peek();
            Int32 originalCount = item.Count;
            try
            {
                item.Count = 1;

                if (item is Weapon weapon)
                {
                    return GetWeaponCost(weapon);
                }
                else
                {
                    return _barterHUD.CalcCostItem(item, _sellDiscount, upTo: false);
                }
            }
            finally
            {
                item.Count = originalCount;
            }
        }

        private Int32 GetWeaponCost(Weapon weapon)
        {
            Int32? ammoCount = weapon.Ammo?.Count;
            try
            {
                if (ammoCount != null)
                    weapon.Ammo.Count = 0;

                return _barterHUD.CalcCostItem(weapon, _sellDiscount, upTo: false);
            }
            finally
            {
                if (ammoCount != null)
                    weapon.Ammo.Count = ammoCount.Value;
            }
        }
    }
}