using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;

namespace ModLoader
{
    /// <summary>
    /// To avoid Unity's BUG.
    /// </summary>
    public static class GameModLoader
    {
        public static CompositionContainer container;
        public static void Initialize(string[] paths)
        {
            DirectoryCatalog[] catalogs = new DirectoryCatalog[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                catalogs[i] = new DirectoryCatalog(paths[i]);
            }
            var catalog = new AggregateCatalog(catalogs);
            container = new CompositionContainer(catalog);
        }
        public static IEnumerable<Lazy<T, IMetaData>> GetExports<T, IMetaData>()
        {
            return container.GetExports<T, IMetaData>();
        }
    }
}
