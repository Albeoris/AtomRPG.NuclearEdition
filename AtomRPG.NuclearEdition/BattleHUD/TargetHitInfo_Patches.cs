using System;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class TargetHitInfo_Patches
    {
        private static readonly TargetHitInfo_Logic s_logic = new TargetHitInfo_Logic();

        public static void Patch(HarmonyInstance harmony)
        {
            try
            {
                Patch_PlayerControl_ProcessSelection_ShowHitChance(harmony);

                Debug.Log($"[{nameof(TargetHitInfo_Patches)}] Successfully patched.");
            }
            catch (Exception ex)
            {
                Debug.Log($"[{nameof(TargetHitInfo_Patches)}] Failed to patch. Error: {ex}");
            }
        }

        private static void Patch_PlayerControl_ProcessSelection_ShowHitChance(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<PlayerControl>.GetInstanceMethod("ProcessSelection");
            MethodInfo prefix = TypeCache<TargetHitInfo_Patches>.GetStaticMethod(nameof(ProcessSelection_Postfix_ShowHitChance));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        private static void ProcessSelection_Postfix_ShowHitChance(PlayerControl __instance)
        {
            var playerControl = new PlayerControl_Proxy(__instance);
            
            s_logic.Init(playerControl);
            s_logic.ShowHitChance();
        }
    }
}