using System;

namespace AtomRPG.NuclearEdition
{
    internal sealed class PlayerControl_Proxy
    {
        private readonly PlayerControl _instance;

        public PlayerControl_Proxy(PlayerControl instance)
        {
            _instance = instance;
        }

        public CharacterComponent CharacterComponent => _instance.CharacterComponent;
        public Boolean HasManualTarget => _instance.HasManualTarget;

        private static DMakeStringCTH s_makeStringCTH;
        private delegate String DMakeStringCTH(PlayerControl __instance, ITargetable target);
        public String MakeStringCTH(ITargetable target)
        {
            DMakeStringCTH call = s_makeStringCTH ?? (s_makeStringCTH = InstanceMethodAccessor.GetDelegate<DMakeStringCTH>("MakeStringCTH"));
            return call(_instance, target);
        }
    }
}