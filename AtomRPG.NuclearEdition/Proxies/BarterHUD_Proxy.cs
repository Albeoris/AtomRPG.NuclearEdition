using System;
using UnityEngine.UI;

namespace AtomRPG.NuclearEdition
{
    internal sealed class BarterHUD_Proxy
    {
        public BarterHUD _instance { get; }

        private readonly InstanceFieldAccessor<BarterHUD, Inventory> mLeftInventory;
        private readonly InstanceFieldAccessor<BarterHUD, Inventory> mRightInventory;
        private readonly InstanceFieldAccessor<BarterHUD, Inventory> mLeftTradeInventory;
        private readonly InstanceFieldAccessor<BarterHUD, Inventory> mRightTradeInventory;

        private readonly DGetSellDiscount _getSellDiscount;
        private readonly DCalcSellCost _calcSellCost;
        private readonly DCalcBuyCost _calcBuyCost;
        private readonly DCalcCostItem _calcCostItem;
        private readonly DDropItemToBackpack _dropItemToBackpack;

        private delegate Int32 DGetSellDiscount(BarterHUD __instance);
        private delegate Int32 DCalcSellCost(BarterHUD __instance, Inventory inventory);
        private delegate Int32 DCalcBuyCost(BarterHUD __instance, Inventory inventory);
        private delegate Int32 DCalcCostItem(BarterHUD __instance, Item item, Int32 sale, Boolean upTo);
        private delegate void DDropItemToBackpack(BarterHUD __instance, BackpackHUD backpack, Inventory inventory, Item item);

        public BarterHUD_Proxy(BarterHUD barterHUD)
        {
            _instance = barterHUD;

            mLeftInventory = new InstanceFieldAccessor<BarterHUD, Inventory>(_instance, nameof(mLeftInventory));
            mRightInventory = new InstanceFieldAccessor<BarterHUD, Inventory>(_instance, nameof(mRightInventory));
            mLeftTradeInventory = new InstanceFieldAccessor<BarterHUD, Inventory>(_instance, nameof(mLeftTradeInventory));
            mRightTradeInventory = new InstanceFieldAccessor<BarterHUD, Inventory>(_instance, nameof(mRightTradeInventory));

            _getSellDiscount = InstanceMethodAccessor.GetDelegate<DGetSellDiscount>("GetSellDiscount");
            _calcSellCost = InstanceMethodAccessor.GetDelegate<DCalcSellCost>("CalcSellCost");
            _calcBuyCost = InstanceMethodAccessor.GetDelegate<DCalcBuyCost>("CalcBuyCost");
            _calcCostItem = InstanceMethodAccessor.GetDelegate<DCalcCostItem>("CalcCostItem");
            _dropItemToBackpack = InstanceMethodAccessor.GetDelegate<DDropItemToBackpack>("DropItemToBackpack");
        }

        public BackpackHUD LeftBackpack => _instance.LeftBackpack;
        public BackpackHUD LeftTradeBackpack => _instance.LeftTradeBackpack;
        public BackpackHUD RightBackpack => _instance.RightBackpack;
        public BackpackHUD RightTradeBackpack => _instance.RightTradeBackpack;
        public Text TradeBoxText => _instance.TradeBoxText;

        public Inventory LeftInventory
        {
            get => mLeftInventory.Value;
            set => mLeftInventory.Value = value;
        }

        public Inventory RightInventory
        {
            get => mRightInventory.Value;
            set => mRightInventory.Value = value;
        }

        public Inventory LeftTradeInventory
        {
            get => mLeftTradeInventory.Value;
            set => mLeftTradeInventory.Value = value;
        }

        public Inventory RightTradeInventory
        {
            get => mRightTradeInventory.Value;
            set => mRightTradeInventory.Value = value;
        }

        public Int32 GetSellDiscount()
        {
            return _getSellDiscount(_instance);
        }

        public Int32 CalcSellCost(Inventory inventory)
        {
            return _calcSellCost(_instance, inventory);
        }

        public Int32 CalcBuyCost(Inventory inventory)
        {
            return _calcBuyCost(_instance, inventory);
        }

        public Int32 CalcCostItem(Item item, Int32 sale, Boolean upTo)
        {
            return _calcCostItem(_instance, item, sale, upTo);
        }

        public void DropItemToBackpack(BackpackHUD toBackpuck, Inventory toInventory, Item item)
        {
            _dropItemToBackpack(_instance, toBackpuck, toInventory, item);
        }

        public void Invalidate()
        {
            _instance.Invalidate();
        }

        public void ShowBackpackWithCost(BackpackHUD backpack, Inventory inventory)
        {
            _instance.ShowBackpackWithCost(backpack, inventory);
        }
    }
}