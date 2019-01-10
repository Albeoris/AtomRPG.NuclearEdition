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
            // Return an original weight for the left backpack
            ChangeLeftBackpack(Game.World.Player.CharacterComponent);
        }

        private void ChangeLeftBackpack(CharacterComponent component)
        {
            Character character = component.Character;
            
            mLeftInventory.Value = character.Inventory;
            LeftBackpack.SetMaxWeigth(character.Stats.MaxCarryWeight);
            _originalHud.ShowBackpackWithCost(LeftBackpack, character.Inventory);
        }

        private void Switch(SwitchDirection switchDirection)
        {
            if (!TryGetSwitchableAllies(out List<CharacterComponent> allies))
                return;

            Int32 newIndex = SwitchCurrentCharacterIndex(switchDirection, allies);
            if (newIndex == -1)
                return;

            CharacterComponent leftAlly = allies[newIndex];
            ChangeLeftBackpack(leftAlly);

            _originalHud.TradeBoxText.text = leftAlly.GetShortName();
        }

        private Int32 SwitchCurrentCharacterIndex(SwitchDirection switchDirection, List<CharacterComponent> allies)
        {
            var currentIndex = allies.FindIndex(cc => cc.Character.Inventory == mLeftInventory.Value);
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

        private Boolean TryGetSwitchableAllies(out List<CharacterComponent> allies)
        {
            allies = Game.World.GetAllTeamMates();
            if (allies.Count == 0)
                return false;

            Inventory rightInventory = mRightInventory.Value;

            // Filter right side team mate
            foreach (CharacterComponent ally in allies)
            {
                if (ally.Character.Inventory == rightInventory)
                {
                    allies.Remove(ally);
                    break;
                }
            }

            // Filter right side player
            if (rightInventory != Game.World.Player.CharacterComponent.Character.Inventory)
                allies.Add(Game.World.Player.CharacterComponent);

            return allies.Count > 1;
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