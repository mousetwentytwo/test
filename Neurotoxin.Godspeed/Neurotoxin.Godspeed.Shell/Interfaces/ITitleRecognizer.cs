using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface ITitleRecognizer
    {
        bool MergeWithCachedEntry(FileSystemItem item);
        bool RecognizeType(FileSystemItem item);
        CacheEntry<FileSystemItem> RecognizeTitle(FileSystemItem item);
        void UpdateCache(FileSystemItem item);
        void ThrowCache(FileSystemItem item);
        CacheEntry<FileSystemItem> GetCacheEntry(FileSystemItem item);
        CacheEntry<FileSystemItem> GetCacheEntry(FileSystemItem item, out CacheComplexKey cacheKey);
        CacheEntry<FileSystemItem> GetCacheEntry(string key);
    }
}