using System.Collections.ObjectModel;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static void Replace<T>(this ObservableCollection<T> collection, T oldItem, T newItem)
        {
            var index = collection.IndexOf(oldItem);
            collection.RemoveAt(index);
            collection.Insert(index, newItem);
        }
    }
}