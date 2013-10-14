using System;
using System.Data.Common;
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
            var connection = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0").CreateConnection();
            connection.ConnectionString = @"Data Source=|DataDirectory|\CacheDb.sdf;Persist Security Info=False;";
            _context = new CacheDbContext(connection);           
            _binaryFormatter = new BinaryFormatter();
        }

        public bool HasEntry(string key)
        {
            return _context.CacheEntries.Any(e => e.CacheKey == key);
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

        public void SaveEntry(string key, object content, DateTime? expiration = null, string tmpPath = null)
        {
            _context.CacheEntries.Add(new CacheEntry
            {
                CacheKey = key,
                Expiration = expiration,
                Content = SerializeContent(content),
                AdditionalDataPath = tmpPath
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

        public void ClearCache(Func<CacheEntry, bool> where = null)
        {
            var items = where != null ? _context.CacheEntries.Where(where) : _context.CacheEntries;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.AdditionalDataPath)) File.Delete(item.AdditionalDataPath);
                _context.CacheEntries.Remove(item);
            }
        }
    }
}