using System;
using System.Linq;
using System.Reflection;

namespace AtomRPG.NuclearEdition
{
    public static class TypeCache<T>
    {
        public static Type Type = typeof(T);

        public static MethodInfo GetStaticMethod(String methodName)
        {
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            MethodInfo method = Type.GetMethods(bindingFlags).SingleOrDefault(m => m.Name == methodName);
            if (method == null)
                throw new ArgumentException($"Cannot find method \"{methodName}\" in the type \"{Type}\".", methodName);

            return method;
        }

        public static MethodInfo GetInstanceMethod(String methodName)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            MethodInfo method = Type.GetMethods(bindingFlags).SingleOrDefault(m => m.Name == methodName);
            if (method == null)
                throw new ArgumentException($"Cannot find method \"{methodName}\" in the type \"{Type}\".", methodName);

            return method;
        }

        public static PropertyInfo GetInstanceProperty(String propertyName)
        {
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            PropertyInfo property = Type.GetProperty(propertyName, bindingFlags);
            if (property == null)
                throw new ArgumentException($"Cannot find property \"{propertyName}\" in the type \"{Type}\".", propertyName);

            return property;
        }
    }
}