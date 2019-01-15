using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    public sealed class ExtendedBarterHUD : MonoBehaviour
    {
        private BarterHUD_Proxy _hud;
        
        public void Initialize(BarterHUD originalHud)
        {
            _hud = new BarterHUD_Proxy(originalHud);
        }

        void Update()
        {
            ExtendedAction extendedAction = CheckSwitch();
            switch (extendedAction)
            {
                case ExtendedAction.SwitchLeft:
                case ExtendedAction.SwitchRight:
                    Switch(extendedAction);
                    break;
         
                case ExtendedAction.AutoSell:
                    AutoSell();
                    break;
            }
        }

        void OnDisable()
        {
            // Return the MC's data of the left backpack
            ChangeLeftBackpack(new CharacterInventory(Game.World.Player.CharacterComponent));
        }

        private void ChangeLeftBackpack(IInventoryOwner character)
        {
            _hud.LeftInventory = character.Inventory;
            _hud.LeftBackpack.SetMaxWeigth(character.MaxCarryWeight);
            _hud.ShowBackpackWithCost(_hud.LeftBackpack, character.Inventory);
        }

        private void Switch(ExtendedAction extendedAction)
        {
            List<IInventoryOwner> allies = GetLeftSideAllies();
            if (allies.Count < 2)
                return;

            Int32 newIndex = SwitchCurrentCharacterIndex(extendedAction, allies);
            if (newIndex == -1)
                return;

            IInventoryOwner leftAlly = allies[newIndex];
            ChangeLeftBackpack(leftAlly);

            _hud.TradeBoxText.text = leftAlly.DisplayName;
        }

        private Int32 SwitchCurrentCharacterIndex(ExtendedAction extendedAction, List<IInventoryOwner> allies)
        {
            Int32 currentIndex = allies.FindIndex(cc => cc.Inventory == _hud.LeftInventory);
            switch (extendedAction)
            {
                case ExtendedAction.SwitchLeft:
                    return allies.LoopedDecrementIndex(currentIndex);
                case ExtendedAction.SwitchRight:
                    return allies.LoopedIncrementIndex(currentIndex);
                default:
                    throw new NotSupportedException(extendedAction.ToString());
            }
        }

        private List<IInventoryOwner> GetLeftSideAllies()
        {
            List<IInventoryOwner> result = new List<IInventoryOwner>(capacity: 8);

            Inventory rightInventory = _hud.RightInventory;

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

        private ExtendedAction CheckSwitch()
        {
            if (!Game.World.HUD.HasWaitState() && !Game.World.HUD.ItemDragging)
            {
                if (InputManager.GetKey(InputManager.Action.Highlight))
                {
                    if (InputManager.GetKeyUp(InputManager.Action.Camera_A))
                        return ExtendedAction.AutoSell;
                }
                else
                {
                    if (InputManager.GetKeyUp(InputManager.Action.Camera_A))
                        return ExtendedAction.SwitchLeft;

                    if (InputManager.GetKeyUp(InputManager.Action.Camera_D))
                        return ExtendedAction.SwitchRight;
                }
            }

            return ExtendedAction.None;
        }

        private void AutoSell()
        {
            Int32 budget = AutoSellLogic.GetBudget(_hud);
            if (budget < AutoSellLogic.MinimalBudget)
            {
                Debug.LogWarning($"[NuclearEdition] Cannot use auto-sell. Not enough budget. Drop something to the trader's zone. Minimal budget: {AutoSellLogic.MinimalBudget}. Current budget: {budget}");
                return;
            }

            List<IInventoryOwner> allies = GetLeftSideAllies();

            AutoSellLogic autoSell = new AutoSellLogic(_hud, allies.Select(a=>a.Inventory));
            autoSell.Sell();
        }

        private enum ExtendedAction
        {
            None = 0,
            SwitchLeft,
            SwitchRight,
            AutoSell
        }
    }
}