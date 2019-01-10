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
        void Update()
        {
            // Don't use Awake()
            // Now the initialization of mods occurs before the initialization of the environment of the game.
            // We have to wait for it to be initialized.
            BarterHUD barterHud = Game.World?.HUD?.Barter;
            if (barterHud == null) // Not yet initialized
                return;

            // Initialize our mod
            Initialize(barterHud);

            // We no longer need this component. Destroy the object.
            Destroy(this.gameObject);
        }

        private void Initialize(BarterHUD barterHud)
        {
            // Add our own component to an existing interface object.
            ExtendedBarterHUD extendedHud = barterHud.gameObject.AddComponent<ExtendedBarterHUD>();
            extendedHud.Initialize(barterHud);
        }
    }
}