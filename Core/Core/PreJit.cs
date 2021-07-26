using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    public static class PreJit
    { // taken from: http://www.liranchen.com/2010/08/forcing-jit-compilation-during-runtime.html
        /// <summary>
        /// recursively load all of assemblies referenced by the given assembly
        /// </summary>
        /// <param name="assembly"></param>
        public static void ForceLoadAll(Assembly assembly)
          => ForceLoadAll(assembly, new HashSet<Assembly>());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        public static void PreJITMethods(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type curType in types)
            {
                MethodInfo[] methods = curType.GetMethods(
                        BindingFlags.DeclaredOnly |
                        BindingFlags.NonPublic |
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.Static);

                foreach (MethodInfo curMethod in methods)
                {
                    if (curMethod.IsAbstract ||
                        curMethod.ContainsGenericParameters)
                    {
                        continue;
                    }

                    RuntimeHelpers.PrepareMethod(curMethod.MethodHandle);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="loadedAssemblies"></param>
        private static void ForceLoadAll(Assembly assembly,
                                                 HashSet<Assembly> loadedAssemblies)
        {
            bool alreadyLoaded = !loadedAssemblies.Add(assembly);
            if (alreadyLoaded)
            {
                return;
            }

            AssemblyName[] refrencedAssemblies =
                assembly.GetReferencedAssemblies();

            foreach (AssemblyName curAssemblyName in refrencedAssemblies)
            {
                Assembly nextAssembly = Assembly.Load(curAssemblyName);
                if (nextAssembly.GlobalAssemblyCache)
                {
                    continue;
                }

                ForceLoadAll(nextAssembly, loadedAssemblies);
            }
        }
    } // cls
}