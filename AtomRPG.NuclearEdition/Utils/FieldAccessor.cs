using System;
using System.Reflection;

namespace AtomRPG.NuclearEdition
{
    public sealed class FieldAccessor<TClass, TValue>
    {
        private readonly TClass _instance;
        private readonly FieldInfo _field;

        public FieldAccessor(TClass instance, String fieldName)
        {
            _instance = instance;
            _field = GetField(fieldName);
        }

        private Func<TClass, TValue> _getter;
        private Action<TClass, TValue> _setter;

        public TValue Value
        {
            get
            {
                if (_getter == null)
                    _getter = Expressions.MakeInstanceGetter<TClass, TValue>(_field);

                return _getter(_instance);
            }
            set
            {
                if (_setter == null)
                    _setter = Expressions.MakeInstanceSetter<TClass, TValue>(_field);

                _setter(_instance, value);
            }
        }

        private static FieldInfo GetField(String fieldName)
        {
            FieldInfo field = TypeCache<TClass>.Type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new NotSupportedException($"Cannot find the field {fieldName} of the type {TypeCache<TClass>.Type}.");
            return field;
        }
    }
}