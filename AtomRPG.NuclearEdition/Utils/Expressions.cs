using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AtomRPG.NuclearEdition
{
    public static class Expressions
    {
        public static Action<TClass, TValue> MakeInstanceSetter<TClass, TValue>(FieldInfo field)
        {
            DynamicMethod method = new DynamicMethod(field.Name + "_publicSetter", typeof(void), new[] {typeof(TClass), typeof(TValue)}, typeof(Expressions), true);
            ILGenerator cg = method.GetILGenerator();

            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldarg_1);
            cg.Emit(OpCodes.Stfld, field);
            cg.Emit(OpCodes.Ret);

            return (Action<TClass, TValue>)method.CreateDelegate(typeof(Action<TClass, TValue>));
        }

        public static Func<TClass, TValue> MakeInstanceGetter<TClass, TValue>(FieldInfo field)
        {
            DynamicMethod method = new DynamicMethod(field.Name + "_publicGetter", typeof(TValue), new[]{typeof(TClass)}, typeof(Expressions), true);
            ILGenerator cg = method.GetILGenerator();

            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldfld, field);
            cg.Emit(OpCodes.Ret);

            return (Func<TClass, TValue>)method.CreateDelegate(typeof(Func<TClass, TValue>));
        }
    }
}