using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    internal sealed class AutoSellAnalyzer
    {
        private static AutoSellAnalyzer _instance;

        public static AutoSellAnalyzer Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = new AutoSellAnalyzer();
                return _instance;
            }
        }

        private readonly Dictionary<String, Int32> _craftUsage;

        private AutoSellAnalyzer()
        {
            _craftUsage = CalcCraftUsage();
        }

        public Int32 GetCountToSell(PartyInventory inventory, PartyItem item)
        {
            if (item.Proto.name.ToLower().Contains("quest"))
                return 0;

            switch (item.Proto)
            {
                case AmmoProto Ammo:
                    return CheckAmmo(Ammo, item, inventory);
                case ConsumableProto Consumable:
                    return CheckConsumable(Consumable, item, inventory);
                case ExplosiveProto Explosive:
                    return CheckExplosive(Explosive, item);
                case FishingRodProto FishingRod:
                    return CheckFishingRod(FishingRod, item);
                case GeigerProto Geiger:
                    return CheckGeiger(Geiger, item);
                case MemoProto Memo:
                    return CheckMemo(Memo, item);
                case ScannerProto Scanner:
                    return CheckScanner(Scanner, item);
                case UniformProto Uniform:
                    return CheckUniform(Uniform, item);
                case WeaponProto Weapon:
                    return CheckWeapon(Weapon, item);
                case ItemProto Item:
                    return CheckItem(Item, item);
                default:
                    return NotSupported(item);
            }
        }

        private Int32 CheckAmmo(AmmoProto ammo, PartyItem item, PartyInventory inventory)
        {
            Boolean IsWeaponCanUseThisAmmo(PartyItem weaponCandidate)
            {
                if (!(weaponCandidate.Proto is WeaponProto weaponProto))
                    return false;

                if (weaponProto.Ammo == null)
                    return false;

                if (weaponProto.Ammo == ammo)
                    return true;

                if (weaponProto.Ammo.Type == ammo.Type)
                    return true;

                return false;
            }

            Int32 weaponCount = inventory.Enumerate(IsWeaponCanUseThisAmmo).Sum(w => w.Count);
            if (weaponCount == 0)
            {
                switch (ammo.name)
                {
                    case "PB": // Pellets: Small ammo, often made of lead. They can penetrate any poorly armored target.
                    case "BB_AP": // Zip-gun round: A heavy bullet round made for a zip-gun.
                    case "BB_JHP": // Zip-gun shot round: A custom round made for a zip gun. Filled with metal shot.
                    case "Bolt": // Crossbow bolt: A wooden bolt with a sharp metal arrowhead, used for a crossbow.
                    case "bolt_ap": // Metal bolt : An improved version of a wooden bolt.
                        return item.Count; // Sell all
                }
            }

            Int32 minCount = 100 + weaponCount * 200;
            if (item.Count > minCount)
                return item.Count - minCount; // Sell surplus ammunition

            return 0;
        }

        private Int32 CheckConsumable(ConsumableProto consumable, PartyItem item, PartyInventory inventory)
        {
            // Don't sell unique consumable
            if (item.Count == 1)
                return 0;

            // Don't sell expensive consumables
            if (consumable.Cost > 250)
                return 0;
            
            Int32 NutritionalValue(ConsumableProto consumableProto)
            {
                if (consumable.Effects == null)
                    return 0;

                Int32 result = 0;
                foreach (EffectProto effect in consumable.Effects)
                {
                    foreach (EffectApplyer applyer in effect.Applyers)
                    {
                        if (applyer.Type == EffectApplyer.ApplyerType.Hunger && applyer.ValueType == EffectApplyer.ApplyerValueType.Number)
                            result += applyer.Value[0];
                    }
                }

                return result;
            }

            Int32 nutritionalValue = NutritionalValue(consumable);
            if (nutritionalValue < 0)
            {
                Int32 totalNutritionalValue = 0;
                foreach (var other in inventory.Enumerate(_ => true))
                {
                    if (other.Proto is ConsumableProto consumableProto)
                        totalNutritionalValue += NutritionalValue(consumableProto);
                }

                if (Game.World.Player.CharacterComponent.HasPerk(CharacterStats.Perk.SP_Hunger))
                {
                    totalNutritionalValue += totalNutritionalValue * 20 / 100;
                    nutritionalValue += totalNutritionalValue * 20 / 100;
                }

                Int32 minimumFoodReserves = 1500 * 10;

                if (Game.World.Player.CharacterComponent.HasPerk(CharacterStats.Perk.Glutton))
                    minimumFoodReserves *= 2;
                else if (Game.World.Player.CharacterComponent.HasPerk(CharacterStats.Perk.Ascetic))
                    minimumFoodReserves /= 2;

                Int32 surplusFood = -(totalNutritionalValue) - minimumFoodReserves;
                if (surplusFood < 0)
                    return 0;

                Int32 surplusCount = surplusFood / nutritionalValue;

                if (consumable.name == "Meat")
                    surplusCount = Math.Min(surplusCount, item.Count - 10);
                else
                    surplusCount = Math.Min(surplusCount, item.Count - 1);

                if (surplusCount > 0)
                    return surplusCount;
            }

            return Math.Max(0, item.Count - 30);
        }

        private Int32 CheckExplosive(ExplosiveProto explosive, PartyItem item)
        {
            // TODO: Check for better explosive

            const Int32 maxSameExplosive = 30;

            if (item.Count > maxSameExplosive)
                return item.Count - maxSameExplosive;

            return 0;
        }

        private Int32 CheckFishingRod(FishingRodProto fishingRod, PartyItem item)
        {
            // Don't sell fishing rods
            return 0;
        }

        private Int32 CheckGeiger(GeigerProto geiger, PartyItem item)
        {
            // Don't sell geiger devices
            return 0;
        }

        private Int32 CheckMemo(MemoProto memo, PartyItem item)
        {
            // Don't sell notes and books
            return 0;
        }

        private Int32 CheckScanner(ScannerProto scanner, PartyItem item)
        {
            // Don't sell scanner devices
            return 0;
        }

        private Int32 CheckUniform(UniformProto uniform, PartyItem item)
        {
            // TODO: Check for better uniforms

            const Int32 maxUniformCost = 500;
            const Int32 maxSameUniform = 2;

            if (uniform.Cost < maxUniformCost && item.Count > maxSameUniform)
                return item.Count - maxSameUniform;

            return 0;
        }

        private Int32 CheckWeapon(WeaponProto weapon, PartyItem item)
        {
            // TODO: Check for better weapons

            const Int32 maxWeaponCost = 500;
            const Int32 maxSameWeapon = 3;

            switch (weapon.name)
            {
                case "zatochka":
                case "Rozochka":
                case "Shovel_Work":
                case "Nagant_Rust":
                case "Pipe_Pistol":
                case "Pipe_Rifle":
                case "Brick":
                case "Knife":
                case "Makarov_Rust":
                case "Mauser":
                case "Montirovka":
                case "sawed-off_rust":
                case "tokarev_rust":
                    return item.Count - 1;
            }

            if (weapon.Cost < maxWeaponCost && item.Count > maxSameWeapon)
                return item.Count - maxSameWeapon;

            return 0;
        }

        private Int32 CheckItem(ItemProto item, PartyItem partyItem)
        {
            const Int32 maxItemCost = 250;

            // Don't sell expensive items
            if (item.Cost > maxItemCost)
                return 0;

            switch (item.name)
            {
                case "Rojo_part": // Piece of Shield (quest)
                case "Gun_Powder": // Gunpowder
                    return 0;
                case "SpiderLymph": // Arachnid lymph node (quest)
                case "SpiderBrain": // Spider brain
                case "WaspFeet": // Wasp legs
                case "AntGland": // Ant salivary gland
                    return Math.Max(0, partyItem.Count - 10);
            }

            Int32 minSameItems = 3;

            if (_craftUsage.TryGetValue(item.name, out var usage))
            {
                if (usage < 20)
                    minSameItems *= 2;
                else
                    minSameItems *= 3;
            }

            if (partyItem.Count > minSameItems)
                return partyItem.Count - minSameItems;

            return 0;
        }

        private readonly HashSet<ItemProto> _notSupportedItems = new HashSet<ItemProto>();

        private Int32 NotSupported(PartyItem item)
        {
            if (_notSupportedItems.Add(item.Proto))
                Debug.LogError($"[NuclearEdition] Cannot evaluate item {item.Proto.Caption} ({item.Proto.name})");
            return 0;
        }

        private static Dictionary<String, Int32> CalcCraftUsage()
        {
            Dictionary<String, Int32> materials = new Dictionary<String, Int32>(capacity: 512);

            CraftProto[] crafts = Resources.LoadAll<CraftProto>("Entities/Craft");

            foreach (var recept in crafts)
            {
                foreach (var ing in recept.ingredients)
                {
                    foreach (var variant in ing.variants)
                    {
                        if (materials.TryGetValue(variant.name, out var value))
                            materials[variant.name] = value + ing.count;
                        else
                            materials.Add(variant.name, ing.count);
                    }
                }
            }

            return materials;
        }
    }
}