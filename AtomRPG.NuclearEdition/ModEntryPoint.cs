using System;
using System.IO;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    // This is the entry point.
    // It should be named ModEntryPoint.
    // It should be inherited from MonoBehaviour.
    // This component will be instantiated while loading mods. You control the lifetime of the object.
    public class ModEntryPoint : MonoBehaviour
    {
        private Single _recheckTimeSec = 0;

        void Update()
        {
            // Exit to the main menu unloads game scenes and interface objects. We must ensure that our mod remains active.
            // Recheck it every 10 seconds
            _recheckTimeSec += Time.deltaTime;
            if (_recheckTimeSec < 10.0 /*sec*/)
                return;

            _recheckTimeSec = 0;

            // Don't use Awake()
            // Now the initialization of mods occurs before the initialization of the environment of the game.
            // We have to wait for it to be initialized.
            BarterHUD barterHud = Game.World?.HUD?.Barter;
            if (barterHud == null) // Not yet initialized
                return;

            // Initialize our mod
            Initialize(barterHud);

            // We can destroy this component after initialization but this mod will be unloaded when the player exit to the main menu. Leave it active in the background.
            // Destroy(this.gameObject);
        }

        private void Initialize(BarterHUD barterHud)
        {
            ExtendedBarterHUD extendedHud = barterHud.gameObject.GetComponent<ExtendedBarterHUD>();
            if (extendedHud == null)
            {
                // Add our own component to an existing interface object.
                extendedHud = barterHud.gameObject.AddComponent<ExtendedBarterHUD>();
                extendedHud.Initialize(barterHud);
            }
        }
    }
}