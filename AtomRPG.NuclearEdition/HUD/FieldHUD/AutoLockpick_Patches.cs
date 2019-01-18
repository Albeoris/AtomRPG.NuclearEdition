using System;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class AutoLockpick_Patches
    {
        public static void Patch(HarmonyInstance harmony)
        {
            try
            {
                Patch_ChestComponent_Use_AutoLockpick(harmony);
                Patch_DoorComponent_Use_AutoLockpick(harmony);

                Debug.Log($"[{nameof(AutoLockpick_Patches)}] Successfully patched.");
            }
            catch (Exception ex)
            {
                Debug.Log($"[{nameof(AutoLockpick_Patches)}] Failed to patch. Error: {ex}");
            }
        }

        private static void Patch_ChestComponent_Use_AutoLockpick(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<ChestComponent>.GetInstanceMethod("Use");
            MethodInfo prefix = TypeCache<AutoLockpick_Patches>.GetStaticMethod(nameof(Patch_ChestComponent_Use_Prefix_AutoLockpick));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        private static void Patch_DoorComponent_Use_AutoLockpick(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<DoorComponent>.GetInstanceMethod("Use");
            MethodInfo prefix = TypeCache<AutoLockpick_Patches>.GetStaticMethod(nameof(Patch_DoorComponent_Use_Prefix_AutoLockpick));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        public static Boolean Patch_ChestComponent_Use_Prefix_AutoLockpick(ChestComponent __instance, CharacterComponent character)
        {
            return AutoLockpick(__instance, character);
        }

        public static Boolean Patch_DoorComponent_Use_Prefix_AutoLockpick(DoorComponent __instance, CharacterComponent character)
        {
            return AutoLockpick(__instance, character);
        }

        private static Boolean AutoLockpick(LockerComponent lockerComponent, CharacterComponent character)
        {
            if (!InputManager.GetKey(InputManager.Action.Highlight))
                return true;

            if (lockerComponent.locker.lockLevel == 0)
                return true;

            Int32 maxLockpick = character.Character.Stats.Lockpick;
            foreach (CharacterComponent teammate in Game.World.GetAllTeamMates())
            {
                Int32 teammateLockpick = teammate.Character.Stats.Lockpick;
                if (teammate.IsHuman() && maxLockpick < teammateLockpick)
                {
                    character = teammate;
                    maxLockpick = teammateLockpick;
                }
            }

            character.Lockpick(lockerComponent);
            return false;
        }
    }
}