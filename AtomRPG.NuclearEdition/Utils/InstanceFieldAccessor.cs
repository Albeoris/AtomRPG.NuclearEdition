using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace AtomRPG.NuclearEdition
{
    public delegate TValue DGetFieldValue<TInstance, TValue>(TInstance instance);
    public delegate void DSetFieldValue<TInstance, TValue>(TInstance instance, TValue value);

    public static class InstanceFieldAccessor
    {
        private static readonly Dictionary<String, Delegate> _getters = new Dictionary<String, Delegate>();
        private static readonly Dictionary<String, Delegate> _setters = new Dictionary<String, Delegate>();

        public static DGetFieldValue<TInstance, TValue> GetValueDelegate<TInstance, TValue>(String fieldName)
        {
            Type delegateType = TypeCache<DGetFieldValue<TInstance, TValue>>.Type;
            String methodKey = fieldName + '/' + delegateType.FullName;

            if (_getters.TryGetValue(methodKey, out var deleg))
                return (DGetFieldValue<TInstance, TValue>)deleg;

            Type instanceType = TypeCache<TInstance>.Type;
            Type valueType = TypeCache<TValue>.Type;

            FieldInfo field = GetExpectedFieldInfo<TInstance, TValue>(fieldName, instanceType, valueType);

            DynamicMethod method = new DynamicMethod(
                name: field.Name + "_publicGetter",
                returnType: valueType,
                parameterTypes: new[] {instanceType},
                owner: typeof(InstanceFieldAccessor),
                skipVisibility: true);

            ILGenerator cg = method.GetILGenerator();

            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldfld, field);
            cg.Emit(OpCodes.Ret);

            var result = (DGetFieldValue<TInstance, TValue>)method.CreateDelegate(delegateType);
            _getters.Add(methodKey, result);

            return result;
        }

        public static DSetFieldValue<TInstance, TValue> SetValueDelegate<TInstance, TValue>(String fieldName)
        {
            Type delegateType = TypeCache<DSetFieldValue<TInstance, TValue>>.Type;
            String methodKey = fieldName + '/' + delegateType.FullName;

            if (_setters.TryGetValue(methodKey, out var deleg))
                return (DSetFieldValue<TInstance, TValue>)deleg;

            Type instanceType = TypeCache<TInstance>.Type;
            Type valueType = TypeCache<TValue>.Type;

            FieldInfo field = GetExpectedFieldInfo<TInstance, TValue>(fieldName, instanceType, valueType);

            DynamicMethod method = new DynamicMethod(
                name: field.Name + "_publicSetter",
                returnType: typeof(void),
                parameterTypes: new[] {instanceType, valueType},
                owner: typeof(InstanceFieldAccessor),
                skipVisibility: true);

            ILGenerator cg = method.GetILGenerator();

            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldarg_1);
            cg.Emit(OpCodes.Stfld, field);
            cg.Emit(OpCodes.Ret);

            var result = (DSetFieldValue<TInstance, TValue>)method.CreateDelegate(delegateType);
            _setters.Add(methodKey, result);

            return result;
        }

        private static FieldInfo GetExpectedFieldInfo<TInstance, TValue>(String fieldName, Type instanceType, Type valueType)
        {
            FieldInfo field = TypeCache<TInstance>.Type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new NotSupportedException($"Cannot find the field {fieldName} of the type {instanceType}.");

            if (field.FieldType != valueType)
                throw new NotSupportedException($"Unexpected type {field.FieldType} of field {instanceType}.{fieldName}. Expected: {valueType}.");

            return field;
        }
    }

    public sealed class InstanceFieldAccessor<TClass, TValue>
    {
        private readonly TClass _instance;
        private readonly String _fieldName;

        public InstanceFieldAccessor(TClass instance, String fieldName)
        {
            _instance = instance;
            _fieldName = fieldName;
        }

        private DGetFieldValue<TClass, TValue> _getter;
        private DSetFieldValue<TClass, TValue> _setter;

        public TValue Value
        {
            get
            {
                if (_getter == null)
                    _getter = InstanceFieldAccessor.GetValueDelegate<TClass, TValue>(_fieldName);

                return _getter(_instance);
            }
            set
            {
                if (_setter == null)
                    _setter = InstanceFieldAccessor.SetValueDelegate<TClass, TValue>(_fieldName);

                _setter(_instance, value);
            }
        }
    }
}