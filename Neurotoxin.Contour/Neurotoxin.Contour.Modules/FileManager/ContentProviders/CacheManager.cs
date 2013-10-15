using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Neurotoxin.Contour.Modules.FileManager.Database;

namespace Neurotoxin.Contour.Modules.FileManager.ContentProviders
{
    public class CacheManager : IDisposable
    {
        private readonly BinaryFormatter _binaryFormatter;
        private readonly CacheDbContext _context;

        public CacheManager()
        {
            _context = new CacheDbContext();
            _binaryFormatter = new BinaryFormatter();
        }

        public bool HasEntry(string key, long? size, DateTime date)
        {
            return GetEntry(key, size, date) != null;
        }

        public CacheEntry GetEntry(string key, long? size, DateTime date)
        {
            var item = _context.CacheEntries.FirstOrDefault(e => e.CacheKey == key);
            if (item == null) return null;

            if ((item.Expiration.HasValue && item.Expiration < DateTime.Now) ||
                (size.HasValue && item.Size.HasValue && item.Size.Value < size) ||
                (item.Date.HasValue && (date - item.Date.Value).TotalSeconds > 1))
            {
                ClearCache(e => e.CacheKey == key);
                return null;
            }

            return item;
        }

        public T GetEntry<T>(string key)
        {
            var result = default(T);
            var entry = _context.CacheEntries.First(e => e.CacheKey == key);
            if (entry.Content != null)
            {
                var ms = new MemoryStream(entry.Content.ToArray());
                result = (T)_binaryFormatter.Deserialize(ms);
            }
            return result;
        }

        public void SaveEntry(string key, object content, DateTime? date = null, long? size = null, DateTime? expiration = null, string tmpPath = null)
        {
            _context.CacheEntries.Add(new CacheEntry
            {
                CacheKey = key,
                Date = date,
                Size = size,
                Expiration = expiration,
                Content = SerializeContent(content),
                TempFilePath = tmpPath
            });
        }

        public void UpdateEntry(string key, object content)
        {
            var entry = _context.CacheEntries.First(e => e.CacheKey == key);
            entry.Content = SerializeContent(content);
            entry.Expiration = null;
        }

        private byte[] SerializeContent(object content)
        {
            byte[] binary = null;
            if (content != null)
            {
                var ms = new MemoryStream();
                _binaryFormatter.Serialize(ms, content);
                ms.Flush();
                binary = ms.ToArray();
            }
            return binary;
        }

        public void Dispose()
        {
            _context.SaveChanges();
            _context.Dispose();
        }

        public void InvalidateExpiredEntries()
        {
            ClearCache(e => e.Expiration != null && e.Expiration < DateTime.Now);
        }

        public void ClearCache(string key)
        {
            ClearCache(e => e.CacheKey == key);
        }

        public void ClearCache(Func<CacheEntry, bool> where = null)
        {
            var items = where != null ? _context.CacheEntries.Where(where) : _context.CacheEntries;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.TempFilePath)) File.Delete(item.TempFilePath);
                _context.CacheEntries.Remove(item);
            }
        }
    }
}