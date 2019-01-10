using System.IO;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    public class ModEntryPoint : MonoBehaviour
    {
        void Update()
        {
            var barterHud = Game.World?.HUD?.Barter;
            if (barterHud == null) // Not yet initialized
                return;

            Initialize(barterHud);
            Destroy(this.gameObject);
        }

        private void Initialize(BarterHUD barterHud)
        {
            ExtendedBarterHUD extendedHud = barterHud.gameObject.AddComponent<ExtendedBarterHUD>();
            extendedHud.Initialize(barterHud);
        }
    }
}