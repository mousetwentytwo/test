using System;
using System.Collections.Generic;
using System.IO;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Models;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class CacheManager
    {
        private const string KeyPrefix = "CacheEntry_";
        private readonly EsentPersistentDictionary _cacheStore = EsentPersistentDictionary.Instance;

        public CacheManager()
        {
            //Read cache to memory to fasten access
            _cacheStore.Keys.Where(k => k.StartsWith(KeyPrefix)).ForEach(k => _cacheStore.Get<CacheEntry<FileSystemItem>>(k));
        }

        public bool HasEntry(string key)
        {
            return GetEntry(key) != null;
        }

        public CacheEntry<FileSystemItem> GetEntry(string key, long? size = null, DateTime? date = null)
        {
            var hashKey = HashKey(key);
            if (!_cacheStore.ContainsKey(hashKey)) return null;

            var item = _cacheStore.Get<CacheEntry<FileSystemItem>>(hashKey);

            if ((item.Expiration.HasValue && item.Expiration < DateTime.Now) ||
                (size.HasValue && item.Size.HasValue && item.Size.Value < size) ||
                (date.HasValue && item.Date.HasValue && (date.Value - item.Date.Value).TotalSeconds > 1))
            {
                ClearCache(key);
                return null;
            }

            return item;
        }

        public void SaveEntry(string key, FileSystemItem content, DateTime? expiration = null, DateTime? date = null, long? size = null, string tmpPath = null)
        {
            var entry = new CacheEntry<FileSystemItem>
                {
                    Expiration = expiration,
                    Date = date,
                    Size = size,
                    Content = content,
                    TempFilePath = tmpPath
                };
            var hashKey = HashKey(key);
            _cacheStore.Put(hashKey, entry);
        }

        public void UpdateEntry(string key, FileSystemItem content)
        {
            var hashKey = HashKey(key);
            if (!_cacheStore.ContainsKey(hashKey)) 
            {
                SaveEntry(key, content);
                return;
            }
            var item = _cacheStore.Get<CacheEntry<FileSystemItem>>(hashKey);
            item.Content = content;
            item.Expiration = null;
            _cacheStore.Update(hashKey, item);
        }

        public void ClearCache()
        {
            foreach (var key in _cacheStore.Keys.Where(key => key.StartsWith(KeyPrefix)))
            {
                RemoveCacheEntry(key);
            }
        }

        public void ClearCache(string key)
        {
            RemoveCacheEntry(HashKey(key));
        }

        private void RemoveCacheEntry(string hashKey)
        {
            if (!_cacheStore.ContainsKey(hashKey)) return;
            var item = _cacheStore.Get<CacheEntry<FileSystemItem>>(hashKey);
            if (!string.IsNullOrEmpty(item.TempFilePath)) File.Delete(item.TempFilePath);
            _cacheStore.Remove(hashKey);
        }

        private static string HashKey(string s)
        {
            return KeyPrefix + s.Hash();
        }

    }
}