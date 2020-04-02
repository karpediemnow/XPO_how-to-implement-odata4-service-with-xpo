using Microsoft.Extensions.Caching.Memory;
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

        private MemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());
        private ConcurrentDictionary<object, SemaphoreSlim> locks = new ConcurrentDictionary<object, SemaphoreSlim>();

        public async Task<CacheItem> GetOrCreate(object key, Func<Task<CacheItem>> createItem)
        {
            CacheItem cacheEntry;
            if (!MemoryCache.TryGetValue(key, out cacheEntry))// Look for cache key.
            {
                SemaphoreSlim mylock = locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
                await mylock.WaitAsync();
                try
                {
                    if (!MemoryCache.TryGetValue(key, out cacheEntry))
                    {
                        // Key not in cache, so get data.
                        cacheEntry = await createItem();

                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                         .SetSize(1)//Size amount
                        //Priority on removing when reaching size limit (memory pressure)
                        .SetPriority(CacheItemPriority.High)
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromSeconds(10))
                        // Remove from cache after this time, regardless of sliding expiration
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(10));


                        MemoryCache.Set(key, cacheEntry, cacheEntryOptions);
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

        }
    }

}