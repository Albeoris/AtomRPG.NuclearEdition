using System;
using System.Collections.Generic;

namespace AtomRPG.NuclearEdition
{
    public static class ExtensionMethodsList
    {
        public static Int32 LoopedDecrementIndex<T>(this List<T> list, Int32 index)
        {
            if (index == -1)
                return -1;

            if (list.Count == 0)
                return -1;

            index--;
            if (index < 0)
                index = list.Count - 1;

            return index;
        }

        public static Int32 LoopedIncrementIndex<T>(this List<T> list, Int32 index)
        {
            if (index == -1)
                return -1;

            if (list.Count == 0)
                return -1;

            index++;
            if (index > list.Count - 1)
                index = 0;

            return index;
        }
    }
}