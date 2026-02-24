using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatApp.Services
{
    public interface ICacheService
    {
        public void Add(string key, string dictKey,string value);
        public void Remove(string key, string value);
        public void RemoveKey(string key);
        public bool ContainsKey(string key, string dictKey);
        public bool ContainsValue(string key, string value);
    }
}