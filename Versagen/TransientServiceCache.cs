using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen
{

    /// <summary>
    /// Preserves all calls made in a time period to allow calling one service that may be the same thing form many other services all at once.
    /// Disposing this will not destroy the services it returns; it simply disposes any references to them.
    /// </summary>
    public class TransientServiceCache : IServiceProvider, IDisposable
    {
        IServiceProvider initProvider { get; }

        List<object> internalObjects { get; set; }

        internal TransientServiceCache(IServiceProvider _initProvider)
        {
            internalObjects = new List<object>();
            initProvider = _initProvider;
        }

        public object GetService(Type serviceType)
        {
            foreach (var item in internalObjects)
            {
                if (serviceType.IsInstanceOfType(item))
                    return item;
            }
            var holdIt = initProvider.GetService(serviceType);
            internalObjects.Add(holdIt);
            return holdIt;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                internalObjects = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }


    public static class TransientServiceCacheExtensions
    {
        public static TransientServiceCache CacheTransients(this IServiceProvider provider) => new TransientServiceCache(provider);
    }
}
