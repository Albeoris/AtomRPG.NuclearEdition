using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class TargetHitHudCollection
    {
        private readonly HashSet<String> _hudObjects = new HashSet<String>();

        public HashSet<String> GetKnownHudNames()
        {
            return new HashSet<String>(_hudObjects);
        }

        public void Cleanup()
        {
            if (_hudObjects.Count > 0)
            {
                _hudObjects.Do(CleanupHud);
                _hudObjects.Clear();
            }
        }

        private void CleanupHud(String hudName)
        {
            GameObject obj = GameObject.Find(hudName);
            if (obj != null)
                GameObject.Destroy(obj);
        }

        public void DisableHud(String hudName)
        {
            GameObject obj = GameObject.Find(hudName);
            if (obj != null)
                obj.SetActive(false);
        }

        public TargetHitHud ProvideHud(PlayerHUD_Proxy playerHud, CharacterComponent cc)
        {
            Int32 instanceId = cc.GetInstanceID();
            String hudName = MakeHudName(instanceId);

            GameObject hudObject = GameObject.Find(hudName);
            if (hudObject != null)
            {
                return new TargetHitHud(hudObject);
            }

            return CreateHud(playerHud, hudName);
        }

        private TargetHitHud CreateHud(PlayerHUD_Proxy playerHud, String hudName)
        {
            GameObject template = playerHud.Bth;

            GameObject hudLayer = ProvideHudLayer(playerHud);
            GameObject hudObject = GameObject.Instantiate(template);

            hudObject.name = hudName;
            hudObject.transform.SetParent(hudLayer.transform, false);

            _hudObjects.Add(hudName);

            return new TargetHitHud(hudObject);
        }

        private GameObject ProvideHudLayer(PlayerHUD_Proxy playerHud)
        {
            String layerName = MakeHudName("Root");

            GameObject rootObject = GameObject.Find(layerName);
            if (rootObject == null)
            {
                rootObject = new GameObject(layerName);
                rootObject.transform.SetParent(playerHud.Bth.transform.parent.transform);
            }

            return rootObject;
        }

        private String MakeHudName<T>(T id)
        {
            return "AtomRPG.NuclearEdition/TargetHitHUD/" + id;
        }
    }
}