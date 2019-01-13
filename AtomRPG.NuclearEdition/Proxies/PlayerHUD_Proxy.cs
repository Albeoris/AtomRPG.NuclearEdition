using System;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class PlayerHUD_Proxy
    {
        public PlayerHUD _instance { get; }

        public PlayerHUD_Proxy(PlayerHUD hud)
        {
            _instance = hud;
        }

        private static DGetFieldValue<PlayerHUD, GameObject> s_getBth;
        public GameObject Bth => (s_getBth ?? (s_getBth = InstanceFieldAccessor.GetValueDelegate<PlayerHUD, GameObject>("_bth")))(_instance);

        public Node FindMovementTargetNode()
        {
            if (_instance.movePoint.apText.text == String.Empty)
                return null;

            Vector3 movePoint = _instance.movePoint.transform.position;
            return Pathfinder.Instance.FindNode(movePoint);
        }
    }
}