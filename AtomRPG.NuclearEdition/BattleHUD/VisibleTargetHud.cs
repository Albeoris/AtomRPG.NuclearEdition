using System;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class VisibleTargetHud
    {
        public CharacterComponent Character { get; }
        public Vector3 ScreenPoint { get; }

        public VisibleTargetHud(CharacterComponent character, Vector3 screenPoint)
        {
            this.Character = character;
            this.ScreenPoint = screenPoint;
        }

        public Boolean TryGetAimInfo(PlayerControl_Proxy playerControl, out String aimChance)
        {
            aimChance = playerControl.MakeStringCTH(Character);
            return aimChance.Contains("%");
        }
    }
}