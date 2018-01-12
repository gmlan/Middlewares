using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Com.Gmlan.Core.Helper
{
    public static class AssemblyHelper
    {
        #region private Properties


        #endregion

        #region Public Properties

        private static IList<Assembly> LoadedAppDomainAssemblies { get; set; } = new List<Assembly>();

        private static IList<Assembly> LoadedPluginAssemblies { get; set; } = new List<Assembly>();

        #endregion

        public static void ReadLoadedAssemblies()
        {
            LoadedAppDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        }


        public static void ReadPluginAssemblies(string relativePath, string pluginKeywords)
        {
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException(nameof(relativePath));
            if (string.IsNullOrEmpty(pluginKeywords)) throw new ArgumentNullException(nameof(pluginKeywords));

            var newPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            if (!Directory.Exists(newPath))
                return;
            var files = Directory.GetFiles(newPath, "*.dll", SearchOption.AllDirectories);

            foreach (var dll in files.Where(item =>
            {
                var fileName = Path.GetFileName(item);
                return fileName != null && fileName.Contains(pluginKeywords);
            }))
            {
                LoadedPluginAssemblies.Add(Assembly.LoadFrom(dll));
            }
        }


        public static IList<Type> FindAllConcreteTypes(Type assignTypeFrom)
        {
            var loaded = FindAllConcreteTypes(assignTypeFrom, LoadedAppDomainAssemblies).ToList();
            loaded.AddRange(FindAllConcreteTypes(assignTypeFrom, LoadedPluginAssemblies));
            return loaded;
        }

        public static IList<Type> FindAllConcreteTypesFromPlugin(Type assignTypeFrom)
        {
            return FindAllConcreteTypes(assignTypeFrom, LoadedPluginAssemblies);
        }

        public static IList<Type> FindAllConcreteTypes(Type assignTypeFrom, IList<Assembly> assemblies)
        {
            var result = new List<Type>();
            try
            {
                foreach (var a in assemblies)
                {
                    Type[] types = null;
                    try
                    {
                        types = a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        //ignore ef may throw exception.....
                    }
                    if (types != null)
                    {
                        foreach (var type in types)
                        {
                            if (
                                (assignTypeFrom.IsAssignableFrom(type) || (assignTypeFrom.IsGenericTypeDefinition && DoesTypeImplementOpenGeneric(type, assignTypeFrom)))
                                && type.IsClass && !type.IsAbstract)
                            {
                                result.Add(type);
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                    msg += e.Message + Environment.NewLine;

                var fail = new Exception(msg, ex);
                Debug.WriteLine(fail.Message, fail);

                throw fail;
            }
            return result;
        }

        private static bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    var isMatch = genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                    return isMatch;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
