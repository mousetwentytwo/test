﻿using System;
using System.IO;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.Models;
using Microsoft.Practices.ObjectBuilder2;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class CacheManager
    {
        private readonly EsentPersistentDictionary _cacheStore = EsentPersistentDictionary.Instance;

        public bool HasEntry(string key)
        {
            return GetEntry(key) != null;
        }

        public CacheEntry<FileSystemItem> GetEntry(string key, long? size = null, DateTime? date = null)
        {
            CacheEntry<FileSystemItem> item;
            var hashKey = HashKey(key);
            if (!_cacheStore.TryGet(hashKey, out item)) return null;

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
            _cacheStore.Put(HashKey(key), entry);
        }

        public void UpdateEntry(string key, FileSystemItem content)
        {
            var hashKey = HashKey(key);
            var item = _cacheStore.Get<CacheEntry<FileSystemItem>>(hashKey);
            item.Content = content;
            item.Expiration = null;
            _cacheStore.Update(hashKey, item);
        }

        public void ClearCache()
        {
            foreach (var key in _cacheStore.Keys.Where(key => key.StartsWith("CacheEntry_")))
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
            var item = _cacheStore.Get<CacheEntry<FileSystemItem>>(hashKey);
            if (!string.IsNullOrEmpty(item.TempFilePath)) File.Delete(item.TempFilePath);
            _cacheStore.Remove(hashKey);
        }

        private static string HashKey(string s)
        {
            return string.Format("CacheEntry_{0}", s.Hash());
        }

    }
}