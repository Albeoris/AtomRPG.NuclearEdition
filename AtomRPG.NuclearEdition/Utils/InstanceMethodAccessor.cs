using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace AtomRPG.NuclearEdition
{
    public static class InstanceMethodAccessor
    {
        private static readonly Dictionary<String, Delegate> _cache = new Dictionary<String, Delegate>();

        public static TDelegate GetDelegate<TDelegate>(String methodName) where TDelegate : Delegate
        {
            String methodKey = methodName + '/' + TypeCache<TDelegate>.Type.FullName;

            if (_cache.TryGetValue(methodKey, out var deleg))
                return (TDelegate)deleg;

            MethodInfo delegateMethod = TypeCache<TDelegate>.Type.GetMethod("Invoke");
            if (delegateMethod == null)
            {
                foreach (var item in TypeCache<TDelegate>.Type.GetMethods())
                    Debug.Log(item.Name);

                throw new NotSupportedException($"Cannot find InvokeMember of delegate {TypeCache<TDelegate>.Type}");
            }

            ParameterInfo[] parameters = delegateMethod.GetParameters();
            
            Type returnType = delegateMethod.ReturnType;
            Type[] parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

            Type instanceType = parameters[0].ParameterType;
            MethodInfo instanceMethod = instanceType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (instanceMethod == null)
                throw new ArgumentException($"Cannot find method \"{methodName}\" in the type \"{instanceType}\".", methodName);

            DynamicMethod dm = new DynamicMethod(instanceMethod.Name + "_publicCaller", returnType, parameterTypes, typeof(Expressions), true);
            ILGenerator cg = dm.GetILGenerator();

            if (parameterTypes.Length > 0)
                cg.Emit(OpCodes.Ldarg_0);

            if (parameterTypes.Length > 1)
                cg.Emit(OpCodes.Ldarg_1);

            if (parameterTypes.Length > 2)
                cg.Emit(OpCodes.Ldarg_2);

            if (parameterTypes.Length > 3)
                cg.Emit(OpCodes.Ldarg_3);

            if (parameterTypes.Length > 4)
            {
                for (Byte i = 4; i < parameterTypes.Length; i++)
                    cg.Emit(OpCodes.Ldarg_S, i);
            }


            cg.Emit(OpCodes.Callvirt, instanceMethod);
            cg.Emit(OpCodes.Ret);

            TDelegate result = (TDelegate)dm.CreateDelegate(typeof(TDelegate));
            _cache.Add(methodKey, result);

            return result;
        }
    }
}