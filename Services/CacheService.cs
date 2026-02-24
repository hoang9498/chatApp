using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace chatApp.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }
        private static readonly TimeSpan Sliding = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan Absolute = TimeSpan.FromHours(2);
        public bool ContainsKey(string key, string dictKey)
        {
            
            return _cache.TryGetValue(key, out ConcurrentDictionary<string, string>? dict)
                    && dict?.ContainsKey(dictKey) == true;
        }

        public bool ContainsValue(string key, string value)
        {
            
            return _cache.TryGetValue(key, out ConcurrentDictionary<string, string>? dict)
                    && dict?.Values.Contains(value) == true;
        }

        public void Remove(string key, string value)
        {
            if (!_cache.TryGetValue(key, out ConcurrentDictionary<string, string>? dict))
                return;

            dict?.TryRemove(value, out _);
        }

        public void RemoveKey(string key)
        {
            _cache.Remove(key);
        }

        public void Add(string key, string dictKey, string value)
        {
            var dict = _cache.GetOrCreate(key, _ =>{
                _.SetSlidingExpiration(Sliding);
                _.SetAbsoluteExpiration(Absolute); 
                return new ConcurrentDictionary<string, string>();
            });
            dict?.TryAdd(dictKey, value);
        }
    }
}