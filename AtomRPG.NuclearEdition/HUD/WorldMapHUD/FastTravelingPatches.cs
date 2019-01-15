using System;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class FastTraveling_Patches
    {
        public static void Patch(HarmonyInstance harmony)
        {
            try
            {
                Patch_HungerAddiction_HungerPerHour_FastTraveling(harmony);
                Patch_CharacterComponent_UpdateMovement_FastTraveling(harmony);

                Debug.Log($"[{nameof(FastTraveling_Patches)}] Successfully patched.");
            }
            catch (Exception ex)
            {
                Debug.Log($"[{nameof(FastTraveling_Patches)}] Failed to patch. Error: {ex}");
            }
        }

        private static void Patch_HungerAddiction_HungerPerHour_FastTraveling(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<HungerAddiction>.GetInstanceProperty("HungerPerHour").GetGetMethod();
            MethodInfo postfix = TypeCache<FastTraveling_Patches>.GetStaticMethod(nameof(HungerAddiction_HungerPerHour_Postifx_FastTraveling));
            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
        }

        private static void Patch_CharacterComponent_UpdateMovement_FastTraveling(HarmonyInstance harmony)
        {

            MethodInfo original = TypeCache<CharacterComponent>.GetInstanceMethod("UpdateMovement");
            MethodInfo postfix = TypeCache<FastTraveling_Patches>.GetStaticMethod(nameof(CharacterComponent_UpdateMovement_Postifx_FastTraveling));
            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
        }

        private static void HungerAddiction_HungerPerHour_Postifx_FastTraveling(HungerAddiction __instance, ref Int32 __result)
        {
            if (Game.World.cameraControl.WorldViewMode && !Game.World.IsDriveVehicle())
            {
                if (InputManager.GetKey(InputManager.Action.Highlight))
                {
                    Character player = Game.World.Player.CharacterComponent.Character;
                    Single curWeight = player.Inventory.TotalWeight;
                    Single maxWeight = player.Stats.MaxCarryWeight;
                    Single weightPenalty = curWeight / maxWeight;
                    __result = (Int32)(__result * (2 + weightPenalty));
                }
            }
        }

        private static void CharacterComponent_UpdateMovement_Postifx_FastTraveling(CharacterComponent __instance)
        {
            if (Game.World.cameraControl.WorldViewMode && !Game.World.IsDriveVehicle())
            {
                if (InputManager.GetKey(InputManager.Action.Highlight))
                {
                    CharacterFinalStats player = __instance.Character.Stats;
                    var bonus = 1 + (0.3f * player.Strength
                                     + 0.5f * player.Endurance
                                     + 0.7f * player.Agility) / 5.0f;

                    MultiplyDesiredSpeed(__instance, bonus);
                }
            }
        }

        private static DGetFieldValue<CharacterComponent, Single> _getMoveSpeedDesired;
        private static DSetFieldValue<CharacterComponent, Single> _setMoveSpeedDesired;

        private static void MultiplyDesiredSpeed(CharacterComponent character, Single factor)
        {
            const String fieldName = "_moveSpeedDesired";

            if (_getMoveSpeedDesired == null)
                _getMoveSpeedDesired = InstanceFieldAccessor.GetValueDelegate<CharacterComponent, Single>(fieldName);
            if (_setMoveSpeedDesired == null)
                _setMoveSpeedDesired = InstanceFieldAccessor.SetValueDelegate<CharacterComponent, Single>(fieldName);

            Single value = _getMoveSpeedDesired(character);
            _setMoveSpeedDesired(character, value * factor);
        }
    }
}