using System;
using System.IO;
using Harmony;
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

        void Awake()
        {
            try
            {
                HarmonyInstance harmony = HarmonyInstance.Create("com.Albeoris.AtomRPG.NuclearEdition.Patches");

                LootRadiusPatches.Patch(harmony);
                TargetHitInfo_Patches.Patch(harmony);

                Debug.Log("[AtomRPG.NuclearEdition] Successfully patched via Harmony.");
            }
            catch (Exception ex)
            {
                Debug.Log($"[AtomRPG.NuclearEdition] Failed to patch via Harmony. Error: {ex}");
            }
        }

        void Update()
        {
            try
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
            catch (Exception ex)
            {
                Debug.LogError("[AtomRPG.NuclearEdition] Something went wrong. Modification will be disabled. Error: " + ex);
                Destroy(this.gameObject);
            }
        }

        private void Initialize(BarterHUD barterHud)
        {
            ExtendedBarterHUD extendedHud = barterHud.gameObject.GetComponent<ExtendedBarterHUD>();
            if (extendedHud == null)
                InitializeExtendedBarterHUD(barterHud);
        }

        private static void InitializeExtendedBarterHUD(BarterHUD barterHud)
        {
            ExtendedBarterHUD extendedHud;
            try
            {
                // Add our own component to an existing interface object.
                extendedHud = barterHud.gameObject.AddComponent<ExtendedBarterHUD>();
                extendedHud.Initialize(barterHud);

                Debug.Log("[AtomRPG.NuclearEdition] ExtendedBarterHUD is initialized.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[AtomRPG.NuclearEdition] Failed to initialize ExtendedBarterHUD: " + ex);
            }
        }
    }

    //   public void DoPocket(CharacterComponent target, bool steal)
	//{
	//	if (Game.World.battle.InBattle && !Game.World.battle.IsOwnTurn(this))
	//	{
	//		return;
	//	}
	//	if (steal && target.IsMoving())
	//	{
	//		return;
	//	}
	//	if (target.IsDead())
	//	{
	//		if (this.Character.Hallucinating.Level != ConditionLevel.Normal)
	//		{
	//			this.PlayState(Resources.Load<TextAsset>("Entities/Behavior/Hallucinating_DogMeat"), base.gameObject);
	//			return;
	//		}
	//		PocketHUD.PocketEnded pocketEnded = new PocketHUD.PocketEnded(target.CkeckSlostsOnClose);
	//		bool fromCanibal = this.Character.CharProto.Stats.HasPerk(CharacterStats.Perk.Cannibal);
	//		target.OnDeadLoot(fromCanibal);
	//		Inventory inventory = new Inventory(null);
	//		this.MergeItem(inventory, target.Character);
	//		Cell cell = target.GetCell();
	//		foreach (BehaviorComponent behaviorComponent in Game.World.Behaviors)
	//		{
	//			if (behaviorComponent is CharacterComponent)
	//			{
	//				CharacterComponent characterComponent = behaviorComponent as CharacterComponent;
	//				if (characterComponent.IsDead() && characterComponent != target && cell.X == characterComponent.GetCell().X && cell.Y == characterComponent.GetCell().Y)
	//				{
	//					pocketEnded = (PocketHUD.PocketEnded)Delegate.Combine(pocketEnded, new PocketHUD.PocketEnded(characterComponent.CkeckSlostsOnClose));
	//					characterComponent.OnDeadLoot(fromCanibal);
	//					this.MergeItem(inventory, characterComponent.Character);
	//				}
	//			}
	//		}
	//		pocketEnded = (PocketHUD.PocketEnded)Delegate.Combine(pocketEnded, new PocketHUD.PocketEnded(target.OnPocketClosed));
	//		Game.World.HUD.ShowPocket(this, inventory, steal, string.Empty, pocketEnded);
	//	}
	//	else
	//	{
	//		if (this.Character.Hallucinating.Level != ConditionLevel.Normal)
	//		{
	//			this.PlayState(Resources.Load<TextAsset>("Entities/Behavior/Hallucinating_Steal"), base.gameObject);
	//			return;
	//		}
	//		target.CloseBackpackByScript = false;
	//		this.PlayState(target.Character.CharProto.OnPocket, target.gameObject);
	//		if (!target.CloseBackpackByScript)
	//		{
	//			Game.World.HUD.ShowPocket(this, target.Character.Inventory, steal, string.Empty, null);
	//		}
	//	}
	//}
}