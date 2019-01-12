using System;
using UnityEngine.UI;

namespace AtomRPG.NuclearEdition
{
    internal sealed class BarterHUD_Proxy
    {
        public BarterHUD Instance { get; }

        private readonly FieldAccessor<BarterHUD, Inventory> mLeftInventory;
        private readonly FieldAccessor<BarterHUD, Inventory> mRightInventory;
        private readonly FieldAccessor<BarterHUD, Inventory> mLeftTradeInventory;
        private readonly FieldAccessor<BarterHUD, Inventory> mRightTradeInventory;

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
            Instance = barterHUD;

            mLeftInventory = new FieldAccessor<BarterHUD, Inventory>(Instance, nameof(mLeftInventory));
            mRightInventory = new FieldAccessor<BarterHUD, Inventory>(Instance, nameof(mRightInventory));
            mLeftTradeInventory = new FieldAccessor<BarterHUD, Inventory>(Instance, nameof(mLeftTradeInventory));
            mRightTradeInventory = new FieldAccessor<BarterHUD, Inventory>(Instance, nameof(mRightTradeInventory));

            _getSellDiscount = InstanceMethodAccessor.GetDelegate<DGetSellDiscount>("GetSellDiscount");
            _calcSellCost = InstanceMethodAccessor.GetDelegate<DCalcSellCost>("CalcSellCost");
            _calcBuyCost = InstanceMethodAccessor.GetDelegate<DCalcBuyCost>("CalcBuyCost");
            _calcCostItem = InstanceMethodAccessor.GetDelegate<DCalcCostItem>("CalcCostItem");
            _dropItemToBackpack = InstanceMethodAccessor.GetDelegate<DDropItemToBackpack>("DropItemToBackpack");
        }

        public BackpackHUD LeftBackpack => Instance.LeftBackpack;
        public BackpackHUD LeftTradeBackpack => Instance.LeftTradeBackpack;
        public BackpackHUD RightBackpack => Instance.RightBackpack;
        public BackpackHUD RightTradeBackpack => Instance.RightTradeBackpack;
        public Text TradeBoxText => Instance.TradeBoxText;

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
            return _getSellDiscount(Instance);
        }

        public Int32 CalcSellCost(Inventory inventory)
        {
            return _calcSellCost(Instance, inventory);
        }

        public Int32 CalcBuyCost(Inventory inventory)
        {
            return _calcBuyCost(Instance, inventory);
        }

        public Int32 CalcCostItem(Item item, Int32 sale, Boolean upTo)
        {
            return _calcCostItem(Instance, item, sale, upTo);
        }

        public void DropItemToBackpack(BackpackHUD toBackpuck, Inventory toInventory, Item item)
        {
            _dropItemToBackpack(Instance, toBackpuck, toInventory, item);
        }

        public void ShowBackpackWithCost(BackpackHUD backpack, Inventory inventory)
        {
            Instance.ShowBackpackWithCost(backpack, inventory);
        }
    }
}