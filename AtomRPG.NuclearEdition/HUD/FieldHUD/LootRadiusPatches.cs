using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class LootRadius_Patches
    {
        public static void Patch(HarmonyInstance harmony)
        {
            try
            {
                Patch_PlayerSelection_HighlightLoot_ShowOutline(harmony);
                Patch_PlayerSelection_HighlightLoot_GetColorBySelection(harmony);
                Patch_PlayerHUD_ShowPocket_LootRadius(harmony);

                Debug.Log($"[{nameof(LootRadius_Patches)}] Successfully patched.");
            }
            catch (Exception ex)
            {
                Debug.Log($"[{nameof(LootRadius_Patches)}] Failed to patch. Error: {ex}");
            }
        }

        private static void Patch_PlayerSelection_HighlightLoot_ShowOutline(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<PlayerSelection>.GetInstanceMethod("ShowOutline");
            MethodInfo prefix = TypeCache<LootRadius_Patches>.GetStaticMethod(nameof(PlayerSelection_ShowOutline_Prefix_HighlightLoot));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        private static void Patch_PlayerSelection_HighlightLoot_GetColorBySelection(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<PlayerSelection>.GetInstanceMethod("GetColorBySelection");
            MethodInfo prefix = TypeCache<LootRadius_Patches>.GetStaticMethod(nameof(PlayerSelection_GetColorBySelection_Prefix_HighlightLoot));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        private static void Patch_PlayerHUD_ShowPocket_LootRadius(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<PlayerHUD>.GetInstanceMethod("ShowPocket");
            MethodInfo prefix = TypeCache<LootRadius_Patches>.GetStaticMethod(nameof(PlayerHUD_ShowPocket_Prefix));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        public static Boolean PlayerSelection_GetColorBySelection_Prefix_HighlightLoot(PlayerSelection __instance, PlayerSelection.SelectionType type, ref Color __result)
        {
            UInt32 colorBits = (UInt32)type;
            if (colorBits < 0x01000000)
                return true;

            Single a = ((colorBits & 0xFF_00_00_00) >> 24) / 256.0f;
            Single r = ((colorBits & 0x00_FF_00_00) >> 16) / 256.0f;
            Single g = ((colorBits & 0x00_00_FF_00) >> 8) / 256.0f;
            Single b = (colorBits & 0x00_00_00_FF) / 256.0f;

            __result = new Color(r, g, b, a);
            return false;
        }

        public static void PlayerSelection_ShowOutline_Prefix_HighlightLoot(PlayerSelection __instance, EntityComponent entity, ref PlayerSelection.SelectionType type, Single intensity = 0f, Single outline = 0.002f)
        {
            const PlayerSelection.SelectionType GrayColor = unchecked((PlayerSelection.SelectionType)0xFF____80_80_80);
            const PlayerSelection.SelectionType VioletColor = unchecked((PlayerSelection.SelectionType)0xFF__EA_04_FF);

            if (type != PlayerSelection.SelectionType.Select)
                return;

            if (entity is CharacterComponent charComp)
            {
                if (!charComp.IsDead())
                    return;

                const String key = "AtomRPG.NuclearEdition.ShowOutline_Prefix_HighlightLoot.OnDeadLoot";

                if (!charComp.Character.HasKey(key))
                {
                    charComp.Character.AddKey(key);

                    Boolean fromCannibal = Game.World.Player.CharacterComponent.Character.CharProto.Stats.HasPerk(CharacterStats.Perk.Cannibal);
                    charComp.OnDeadLoot(fromCannibal);
                }

                if (charComp.Character.GetItemsCost() == 0)
                    type = GrayColor;
            }
            else if (entity is ChestComponent chestComp)
            {
                Chest chest = chestComp.chest;
                if (chest.lockLevel != 0)
                {
                    type = VioletColor;
                    return;
                }

                if (chest.Inventory.Count == 0)
                    type = GrayColor;
            }
        }

        private delegate void DMergeItem(CharacterComponent __instance, Inventory inv, Character character);

        public static void PlayerHUD_ShowPocket_Prefix(PlayerHUD __instance, CharacterComponent characterComponent, Inventory pocket, Boolean steal, String fraction, ref PocketHUD.PocketEnded notify)
        {
            if (notify == null)
                return;

            if (pocket.Count == 0)
                return;

            List<CharacterComponent> characters = new List<CharacterComponent>();

            foreach (Delegate del in notify.GetInvocationList())
            {
                PocketHUD.PocketEnded invocation = (PocketHUD.PocketEnded)del;
                if (!(invocation.Target is CharacterComponent cc))
                    return;

                if (!cc.IsDead())
                    return;

                characters.Add(cc);
            }

            if (characters.Count == 0)
                return;

            Cell cell = characters.First().GetCell();

            DMergeItem mergeItem = InstanceMethodAccessor.GetDelegate<DMergeItem>("MergeItem");

            Boolean fromCannibal = characterComponent.Character.CharProto.Stats.HasPerk(CharacterStats.Perk.Cannibal);
            foreach (BehaviorComponent behaviorComponent in Game.World.Behaviors)
            {
                if (!(behaviorComponent is CharacterComponent cc))
                    continue;

                if (!cc.IsDead())
                    continue;

                Cell otherCell = cc.GetCell();
                var dx = otherCell.X - cell.X;
                var dy = otherCell.Y - cell.Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);
                if (distance > 20)
                    continue;

                if (characters.Contains(cc))
                    continue;

                notify = (PocketHUD.PocketEnded)Delegate.Combine(new PocketHUD.PocketEnded(cc.CkeckSlostsOnClose), notify);
                cc.OnDeadLoot(fromCannibal);

                mergeItem(characterComponent, pocket, cc.Character);
            }
        }
    }
}