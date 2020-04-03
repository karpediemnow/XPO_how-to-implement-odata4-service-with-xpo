using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ODataService.Helpers
{
    //https://michaelscodingspot.com/cache-implementations-in-csharp-net/

    public class MemoryCacheManager
    {
        private static MemoryCacheManager instance = null;
        public static MemoryCacheManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MemoryCacheManager();
                }
                return instance;
            }
        }

        private MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        private ConcurrentDictionary<object, SemaphoreSlim> cacheLocks = new ConcurrentDictionary<object, SemaphoreSlim>();

        public async Task<CacheItem> GetOrCreate(object key, Func<Task<CacheItem>> createItem)
        {
            CacheItem cacheEntry;
            if (!memoryCache.TryGetValue(key, out cacheEntry))// Look for cache key.
            {
                SemaphoreSlim mylock = cacheLocks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
                await mylock.WaitAsync();
                try
                {
                    if (!memoryCache.TryGetValue(key, out cacheEntry))
                    {
                        // Key not in cache, so get data.
                        cacheEntry = await createItem();

                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var cacheEntryOptions = new MemoryCacheEntryOptions().RegisterPostEvictionCallback(CacheItemRemoved)
                            .AddExpirationToken(new CancellationChangeToken(cts.Token));
                        memoryCache.Set(key, cacheEntry, cacheEntryOptions);
                    }
                }
                finally
                {
                    mylock.Release();
                }
            }
            return cacheEntry;
        }

        private void CacheItemRemoved(object key, object value, EvictionReason reason, object state)
        {
            if (value != null && value is CacheItem)
            {
                ((CacheItem)value).Dispose();
            }
        }
    }

}