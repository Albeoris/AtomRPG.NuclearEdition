using System;

namespace AtomRPG.NuclearEdition
{
    internal sealed class VehicleInventory : IInventoryOwner
    {
        private readonly Game.VehicleData _vehicle;

        public Inventory Inventory => _vehicle.Inventory;
        public Int32 MaxCarryWeight => -1;
        public String DisplayName { get; }

        public VehicleInventory(Game.VehicleData vehicle)
        {
            _vehicle = vehicle;
            DisplayName = GetDisplayName(_vehicle);
        }

        private static String GetDisplayName(Game.VehicleData vehicle)
        {
            String vehicleProto = vehicle.proto;

            try
            {
                if (TryParsePobedaName(vehicleProto, out var name))
                    return name;

                UnityEngine.Debug.LogError($"[ExtendedBarterHUD] Idk name of this vehicle: \"{vehicleProto}\"");
                return vehicleProto;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ExtendedBarterHUD] Failed to get name of this vehicle: \"{vehicleProto}\". Error: {ex}");
                return vehicleProto;
            }
        }

        public static Boolean TryParsePobedaName(String vehicleProto, out String name)
        {
            name = null;

            if (!String.Equals(vehicleProto, "Pobeda", StringComparison.InvariantCultureIgnoreCase))
                return false;

            // [Вы открываете дверь своего верного автомобиля \"ГАЗ-20-СГ1\"]
            // [You open the door of your loyal car \"GAZ-20-SG1\"]
            const String localizationKey = "#pobeda.34929b91-46c3-47fc-b2a6-c326b2da3ced";
            String phrase = Localization.Translate(localizationKey);

            Boolean Fail()
            {
                UnityEngine.Debug.LogError($"[ExtendedBarterHUD] Cannot parse vehicle's name from the string ({localizationKey}: \"{phrase}\")");
                return false;
            }

            Int32 startIndex = phrase.IndexOf('"');
            if (startIndex < 0)
                return Fail();

            Int32 endIndex = phrase.IndexOf('"', startIndex + 1);
            if (endIndex < 0)
                return Fail();

            Int32 length = endIndex - startIndex - 1;

            const Int32 minVehicleNameLength = 2;
            const Int32 maxVehicleNameLength = 16;

            if (length < minVehicleNameLength || length > maxVehicleNameLength)
                return Fail();

            name = phrase.Substring(startIndex + 1, length);
            return true;
        }
    }
}