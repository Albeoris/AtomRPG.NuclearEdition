using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    public sealed class ExtendedBarterHUD : MonoBehaviour
    {
        private BarterHUD _originalHud;
        private FieldAccessor<BarterHUD, Inventory> mLeftInventory;
        private FieldAccessor<BarterHUD, Inventory> mRightInventory;
        private FieldAccessor<BarterHUD, Inventory> mLeftTradeInventory;
        private FieldAccessor<BarterHUD, Inventory> mRightTradeInventory;

        private BackpackHUD LeftBackpack => _originalHud.LeftBackpack;

        public void Initialize(BarterHUD originalHud)
        {
            _originalHud = originalHud;

            mLeftInventory = new FieldAccessor<BarterHUD, Inventory>(_originalHud, nameof(mLeftInventory));
            mRightInventory = new FieldAccessor<BarterHUD, Inventory>(_originalHud, nameof(mRightInventory));
            mLeftTradeInventory = new FieldAccessor<BarterHUD, Inventory>(_originalHud, nameof(mLeftTradeInventory));
            mRightTradeInventory = new FieldAccessor<BarterHUD, Inventory>(_originalHud, nameof(mRightTradeInventory));
        }

        void Update()
        {
            SwitchDirection switchDirection = CheckSwitch();
            if (switchDirection != SwitchDirection.None)
                Switch(switchDirection);
        }

        void OnDisable()
        {
            // Return the MC's data of the left backpack
            ChangeLeftBackpack(new CharacterInventory(Game.World.Player.CharacterComponent));
        }

        private void ChangeLeftBackpack(IInventoryOwner character)
        {
            mLeftInventory.Value = character.Inventory;
            LeftBackpack.SetMaxWeigth(character.MaxCarryWeight);
            _originalHud.ShowBackpackWithCost(LeftBackpack, character.Inventory);
        }

        private void Switch(SwitchDirection switchDirection)
        {
            List<IInventoryOwner> allies = GetSwitchableAllies();
            if (allies.Count < 2)
                return;

            Int32 newIndex = SwitchCurrentCharacterIndex(switchDirection, allies);
            if (newIndex == -1)
                return;

            IInventoryOwner leftAlly = allies[newIndex];
            ChangeLeftBackpack(leftAlly);

            _originalHud.TradeBoxText.text = leftAlly.DisplayName;
        }

        private Int32 SwitchCurrentCharacterIndex(SwitchDirection switchDirection, List<IInventoryOwner> allies)
        {
            Int32 currentIndex = allies.FindIndex(cc => cc.Inventory == mLeftInventory.Value);
            switch (switchDirection)
            {
                case SwitchDirection.Left:
                    return allies.LoopedDecrementIndex(currentIndex);
                case SwitchDirection.Right:
                    return allies.LoopedIncrementIndex(currentIndex);
                default:
                    throw new NotSupportedException(switchDirection.ToString());
            }
        }

        private List<IInventoryOwner> GetSwitchableAllies()
        {
            List<IInventoryOwner> result = new List<IInventoryOwner>(capacity: 8);

            Inventory rightInventory = mRightInventory.Value;

            // Add vehicle if it's on the current location and not on the right side of the barter window
            if (Game.World.VehicleOnLocation())
            {
                VehicleInventory vehicle = new VehicleInventory(Game.World.Vehicle);
                if (rightInventory != vehicle.Inventory)
                    result.Add(vehicle);
            }

            // Add player if it isn't on the right side of the barter window
            CharacterInventory player = new CharacterInventory(Game.World.Player.CharacterComponent);
            if (rightInventory != player.Inventory)
                result.Add(player);

            // Add teammates except the owner of the right side inventory
            List<CharacterComponent> teamMates = Game.World.GetAllTeamMates();
            foreach (CharacterComponent teamMate in teamMates)
            {
                CharacterInventory ally = new CharacterInventory(teamMate);
                if (rightInventory != ally.Inventory)
                    result.Add(ally);
            }

            return result;
        }

        private SwitchDirection CheckSwitch()
        {
            if (!Game.World.HUD.HasWaitState() && !Game.World.HUD.ItemDragging)
            {
                if (InputManager.GetKeyUp(InputManager.Action.Camera_A))
                    return SwitchDirection.Left;

                if (InputManager.GetKeyUp(InputManager.Action.Camera_D))
                    return SwitchDirection.Right;
            }

            return SwitchDirection.None;
        }

        private enum SwitchDirection
        {
            Left = -1,
            None = 0,
            Right = 1
        }
    }
}