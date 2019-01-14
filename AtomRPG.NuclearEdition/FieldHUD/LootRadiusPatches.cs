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
                PatchPlayerSelection_HighlightLoot(harmony);
                PatchPlayerHUD_LootRadius(harmony);

                Debug.Log($"[{nameof(LootRadius_Patches)}] Successfully patched.");
            }
            catch (Exception ex)
            {
                Debug.Log($"[{nameof(LootRadius_Patches)}] Failed to patch. Error: {ex}");
            }
        }

        private static void PatchPlayerSelection_HighlightLoot(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<PlayerSelection>.GetInstanceMethod("ShowOutline");
            MethodInfo prefix = TypeCache<LootRadius_Patches>.GetStaticMethod(nameof(ShowOutline_Prefix_HighlightLoot));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        private static void PatchPlayerHUD_LootRadius(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<PlayerHUD>.GetInstanceMethod("ShowPocket");
            MethodInfo prefix = TypeCache<LootRadius_Patches>.GetStaticMethod(nameof(ShowPocket_Prefix));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        public static void ShowOutline_Prefix_HighlightLoot(PlayerSelection __instance, EntityComponent entity, ref PlayerSelection.SelectionType type, Single intensity = 0f, Single outline = 0.002f)
        {
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
                    type = PlayerSelection.SelectionType.Panic;
            }
            else if (entity is ChestComponent chestComp)
            {
                Chest chest = chestComp.chest;
                if (chest.lockLevel != 0)
                    return;

                if (chest.Inventory.Count == 0)
                    type = PlayerSelection.SelectionType.Panic;
            }
        }

        private delegate void DMergeItem(CharacterComponent __instance, Inventory inv, Character character);

        public static void ShowPocket_Prefix(PlayerHUD __instance, CharacterComponent characterComponent, Inventory pocket, Boolean steal, String fraction, ref PocketHUD.PocketEnded notify)
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