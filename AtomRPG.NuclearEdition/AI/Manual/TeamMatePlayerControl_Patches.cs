using System;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class TeamMatePlayerControl_Patches
    {
        public static void Patch(HarmonyInstance harmony)
        {
            try
            {
                Patch_AIControl_OnTurnBegin_TeamMatePlayerControl(harmony);
                Patch_PlayerControl_OnTurnEnd_TeamMatePlayerControl(harmony);
                Patch_CharacterComponent_Dead_TeamMatePlayerControl(harmony);
                Patch_PlayerControl_OnBattleEnd_TeamMatePlayerControl(harmony);

                Debug.Log($"[{nameof(TeamMatePlayerControl_Patches)}] Successfully patched.");
            }
            catch (Exception ex)
            {
                Debug.Log($"[{nameof(TeamMatePlayerControl_Patches)}] Failed to patch. Error: {ex}");
            }
        }

        private static void Patch_AIControl_OnTurnBegin_TeamMatePlayerControl(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<AIControl>.GetInstanceMethod("OnTurnBegin");
            MethodInfo prefix = TypeCache<TeamMatePlayerControl_Patches>.GetStaticMethod(nameof(AIControl_OnTurnBegin_Prefix_TeamMatePlayerControl));
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        private static void Patch_PlayerControl_OnTurnEnd_TeamMatePlayerControl(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<PlayerControl>.GetInstanceMethod("OnTurnEnd");
            MethodInfo prefix = TypeCache<TeamMatePlayerControl_Patches>.GetStaticMethod(nameof(PlayerControl_OnTurnEnd_Prefix_TeamMatePlayerControl));
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        private static void Patch_PlayerControl_OnBattleEnd_TeamMatePlayerControl(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<PlayerControl>.GetInstanceMethod("OnBattleEnd");
            MethodInfo prefix = TypeCache<TeamMatePlayerControl_Patches>.GetStaticMethod(nameof(PlayerControl_OnBattleEnd_Prefix_TeamMatePlayerControl));
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        private static void Patch_CharacterComponent_Dead_TeamMatePlayerControl(HarmonyInstance harmony)
        {
            MethodInfo original = TypeCache<CharacterComponent>.GetInstanceMethod("Dead");
            MethodInfo prefix = TypeCache<TeamMatePlayerControl_Patches>.GetStaticMethod(nameof(CharacterComponent_Dead_Prefix));
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
        }

        private static PlayerControl _playerControl;
        private static CharacterControl _teamMateControl;
        private static CharacterComponent _playerCharacter;
        private static readonly CharacterControl DummyControl = new CharacterControl();

        private static Boolean AIControl_OnTurnBegin_Prefix_TeamMatePlayerControl(AIControl __instance)
        {
            CharacterComponent character = __instance.CharacterComponent;
            if (character.IsTeamMate())
            {
                if (ChangeToPlayer(character))
                {
                    character.Controller.OnTurnBegin();
                    return false;
                }
            }
            
            return true;
        }

        private static Boolean PlayerControl_OnTurnEnd_Prefix_TeamMatePlayerControl(PlayerControl __instance)
        {
            CharacterComponent character = __instance.CharacterComponent;
            if (character.IsTeamMate())
            {
                if (ReturnToAI(character))
                {
                    character.Controller.OnTurnEnd();

                    if (!Game.World.battle.HasPlayerEnemy())
                        Game.World.battle.UnRegisterAll();

                    return false;
                }
            }
            return true;
        }

        private static Boolean PlayerControl_OnBattleEnd_Prefix_TeamMatePlayerControl(PlayerControl __instance)
        {
            CharacterComponent character = __instance.CharacterComponent;
            if (character.IsTeamMate())
            {
                if (ReturnToAI(character))
                {
                    character.Controller.OnBattleEnd();
                    ClearStates();
                    return false;
                }
            }
            ClearStates();
            return true;
        }

        private static void CharacterComponent_Dead_Prefix(CharacterComponent __instance, CharacterComponent attacker)
        {
            if (__instance.IsTeamMate())
                ReturnToAI(__instance);
        }

        private static Boolean ChangeToPlayer(CharacterComponent teamMate)
        {
            if (_teamMateControl != null)
                return false;
            if (teamMate.Controller is PlayerControl)
                return false;

            _playerControl = Game.World.Player;
            _playerCharacter = _playerControl.CharacterComponent;
            _playerControl.CharacterComponent.Controller = DummyControl;

            _teamMateControl = teamMate.Controller;

            _playerControl.Reset();
            teamMate.Controller = _playerControl;
            teamMate.Controller.Start(teamMate);

            return true;
        }

        private static Boolean ReturnToAI(CharacterComponent character)
        {
            if (_teamMateControl == null)
                return false;
            if (character.Controller is AIControl)
                return false;

            character.Controller = _teamMateControl;
            character.Controller.Start(character);

            _playerControl.Reset();
            _playerCharacter.Controller = _playerControl;
            _playerCharacter.Controller.Start(_playerCharacter);

            ClearStates();

            return true;
        }

        private static void ClearStates()
        {
            _teamMateControl = null;
            _playerControl = null;
            _playerCharacter = null;
        }
    }
}