using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class TargetHitInfo_Logic
    {
        private PlayerControl_Proxy _playerControl;
        private CharacterComponent _player;

        private readonly TargetHitHudCollection _huds = new TargetHitHudCollection();

        public void Init(PlayerControl_Proxy playerControl)
        {
            _playerControl = playerControl;
            _player = _playerControl.CharacterComponent;
        }

        public void ShowHitChance()
        {
            CharacterComponent player = _playerControl.CharacterComponent;

            if (!CanShowHud())
            {
                _huds.Cleanup();
                return;
            }

            PlayerHUD_Proxy playerHud = new PlayerHUD_Proxy(Game.World.HUD);

            Node movementTarget = playerHud.FindMovementTargetNode();

            HashSet<String> remainingHuds = _huds.GetKnownHudNames();

            using (PinnedCharacterPosition.Hack(player, movementTarget))
            {
                foreach (VisibleTargetHud visible in EnumerateVisibleEnemyHuds(player))
                {
                    if (!visible.TryGetAimInfo(_playerControl, out String aimInfo))
                        continue;

                    TargetHitHud hud = _huds.ProvideHud(playerHud, visible.Character);
                    hud.Refresh(visible.ScreenPoint, aimInfo);
                    remainingHuds.Remove(hud.Name);
                }
            }

            remainingHuds.Do(_huds.DisableHud);
        }

        private Boolean CanShowHud()
        {
            if (!InputManager.GetKey(InputManager.Action.Highlight))
                return false;

            if (!Game.World.battle.InBattle)
                return false;

            if (_playerControl.HasManualTarget)
                return false;

            if (_player.HasAttackTarget())
                return false;

            return true;
        }

        private static IEnumerable<VisibleTargetHud> EnumerateVisibleEnemyHuds(CharacterComponent player)
        {
            Rect screenArea = Screen.safeArea;

            foreach (BehaviorComponent behaviorComponent in Game.World.Behaviors)
            {
                if (!(behaviorComponent is CharacterComponent cc))
                    continue;

                if (cc.IsDead())
                    continue;

                Vector3 screenPoint = Camera.main.WorldToScreenPoint(behaviorComponent.transform.position);
                screenPoint.x += 20f;
                screenPoint.y -= 35f;

                if (!screenArea.Contains(screenPoint))
                    continue;

                if (!cc.IsEnemy(player))
                    continue;

                yield return new VisibleTargetHud(cc, screenPoint);
            }
        }
    }
}