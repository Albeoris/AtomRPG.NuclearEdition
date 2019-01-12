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

        public static Action<TInstance, TArg1, TArg2> MakeInstanceAction<TInstance, TArg1, TArg2>(MethodInfo method)
        {
            DynamicMethod dm = new DynamicMethod(method.Name + "_publicInvoker", typeof(void), new[] {typeof(TInstance), typeof(TArg1), typeof(TArg2)}, typeof(Expressions), true);
            ILGenerator cg = dm.GetILGenerator();

            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldarg_1);
            cg.Emit(OpCodes.Ldarg_2);
            cg.Emit(OpCodes.Callvirt, method);
            cg.Emit(OpCodes.Ret);

            return (Action<TInstance, TArg1, TArg2>)dm.CreateDelegate(typeof(Action<TInstance, TArg1, TArg2>));
        }

        public static Func<TInstance, TResult> MakeInstanceFunc<TResult, TInstance>(MethodInfo method)
        {
            DynamicMethod dm = new DynamicMethod(method.Name + "_publicCaller", typeof(TResult), new[] {typeof(TInstance)}, typeof(Expressions), true);
            ILGenerator cg = dm.GetILGenerator();

            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Callvirt, method);
            cg.Emit(OpCodes.Ret);

            return (Func<TInstance, TResult>)dm.CreateDelegate(typeof(Func<TInstance, TResult>));
        }

        public static Func<TInstance, TArg1, TResult> MakeInstanceFunc<TResult, TInstance, TArg1>(MethodInfo method)
        {
            DynamicMethod dm = new DynamicMethod(method.Name + "_publicCaller", typeof(TResult), new[] {typeof(TInstance), typeof(TArg1)}, typeof(Expressions), true);
            ILGenerator cg = dm.GetILGenerator();

            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldarg_1);
            cg.Emit(OpCodes.Callvirt, method);
            cg.Emit(OpCodes.Ret);

            return (Func<TInstance, TArg1, TResult>)dm.CreateDelegate(typeof(Func<TInstance, TArg1, TResult>));
        }

        public static Func<TInstance, TArg1, TArg2, TResult> MakeInstanceFunc<TResult, TInstance, TArg1, TArg2>(MethodInfo method)
        {
            DynamicMethod dm = new DynamicMethod(method.Name + "_publicCaller", typeof(TResult), new[] {typeof(TInstance), typeof(TArg1), typeof(TArg2)}, typeof(Expressions), true);
            ILGenerator cg = dm.GetILGenerator();

            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldarg_1);
            cg.Emit(OpCodes.Ldarg_2);
            cg.Emit(OpCodes.Callvirt, method);
            cg.Emit(OpCodes.Ret);

            return (Func<TInstance, TArg1, TArg2, TResult>)dm.CreateDelegate(typeof(Func<TInstance, TArg1, TArg2, TResult>));
        }

        public static Func<TInstance, TArg1, TArg2, TArg3, TResult> MakeInstanceFunc<TResult, TInstance, TArg1, TArg2, TArg3>(MethodInfo method)
        {
            DynamicMethod dm = new DynamicMethod(method.Name + "_publicCaller", typeof(TResult), new[] {typeof(TInstance), typeof(TArg1), typeof(TArg2), typeof(TArg3)}, typeof(Expressions), true);
            ILGenerator cg = dm.GetILGenerator();

            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldarg_1);
            cg.Emit(OpCodes.Ldarg_2);
            cg.Emit(OpCodes.Ldarg_3);
            cg.Emit(OpCodes.Callvirt, method);
            cg.Emit(OpCodes.Ret);

            return (Func<TInstance, TArg1, TArg2, TArg3, TResult>)dm.CreateDelegate(typeof(Func<TInstance, TArg1, TArg2, TArg3, TResult>));
        }
    }
}