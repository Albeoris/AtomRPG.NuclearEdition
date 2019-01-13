using System;
using UnityEngine;
using UnityEngine.UI;

namespace AtomRPG.NuclearEdition
{
    internal sealed class TargetHitHud
    {
        private readonly GameObject _obj;
        private readonly Text _text;

        public TargetHitHud(GameObject obj)
        {
            _obj = obj;
            _text = obj.GetComponent<Text>();
        }

        public String Name => _obj.name;

        public void Refresh(Vector3 visibleScreenPoint, String aimInfo)
        {
            _obj.SetActive(true);
            _obj.transform.position = visibleScreenPoint;
            _text.text = aimInfo;
        }
    }
}