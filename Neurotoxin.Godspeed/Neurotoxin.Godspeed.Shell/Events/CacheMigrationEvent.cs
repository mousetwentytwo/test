using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class CacheMigrationEvent : CompositePresentationEvent<CacheMigrationEventArgs> { }

    public class CacheMigrationEventArgs
    {
        public Action<string, CacheEntry<FileSystemItem>, EsentPersistentDictionary, object> ItemMigrationAction { get; private set; }
        public Action MigrationFinishedAction { get; private set; }
        public Dictionary<string, CacheEntry<FileSystemItem>> Items { get; private set; }
        public EsentPersistentDictionary CacheStore { get; private set; }
        public object Payload { get; private set; }

        public CacheMigrationEventArgs(Action<string, CacheEntry<FileSystemItem>, EsentPersistentDictionary, object> itemMigrationAction,
                                       Action migrationFinishedAction, 
                                       Dictionary<string, CacheEntry<FileSystemItem>> items,
                                       EsentPersistentDictionary cacheStore, 
                                       object payload)
        {
            ItemMigrationAction = itemMigrationAction;
            MigrationFinishedAction = migrationFinishedAction;
            Items = items;
            CacheStore = cacheStore;
            Payload = payload;
        }
    }
}