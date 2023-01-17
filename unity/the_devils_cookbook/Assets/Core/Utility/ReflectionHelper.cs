using System.Collections.Generic;

namespace TDC.Core.Utility
{
    public static class ReflectionHelper
    {
        public static System.Type[] GetAllDerivedTypes(this System.AppDomain appDomain, System.Type baseType)
        {
            List<System.Type> result = new List<System.Type>();
            System.Reflection.Assembly[] assemblies = appDomain.GetAssemblies();
            foreach (System.Reflection.Assembly assembly in assemblies)
            {
                System.Type[] types = assembly.GetTypes();
                foreach (System.Type type in types)
                {
                    if (type.IsSubclassOf(baseType))
                    {
                        result.Add(type);
                    }
                }
            }
            return result.ToArray();
        }
    }
}
