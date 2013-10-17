using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Neurotoxin.Contour.Core.Caching;
using Neurotoxin.Contour.Core.Extensions;
using Neurotoxin.Contour.Modules.FileManager.Models;

namespace Neurotoxin.Contour.Modules.FileManager.ContentProviders
{
    public class CacheManager
    {
        private readonly EsentPersistentDictionary _cacheStore = EsentPersistentDictionary.Instance;

        public bool HasEntry(string key, long? size, DateTime date)
        {
            return GetEntry(key, size, date) != null;
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

        public void ClearCache(string key)
        {
            var hashKey = HashKey(key);
            var item = _cacheStore.Get<CacheEntry<FileSystemItem>>(hashKey);
            if (!string.IsNullOrEmpty(item.TempFilePath)) File.Delete(item.TempFilePath);
            _cacheStore.Remove(hashKey);
        }

        private static string HashKey(string s)
        {
            var shA1Managed = new SHA1Managed();
            return string.Format("CacheEntry_{0}", shA1Managed.ComputeHash(Encoding.UTF8.GetBytes(s)).ToHex());
        }

    }
}