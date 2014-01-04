using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Extensions;

namespace Neurotoxin.Godspeed.Core.Models
{
    public class Tree<T> where T : class
    {
        private class TreeItem<T>
        {
            public T Content { get; set; }
            public List<TreeItem<T>> Children { get; private set; }

            public TreeItem(T content)
            {
                Content = content;
                Children = new List<TreeItem<T>>();
            }
        }

        private readonly Dictionary<string, TreeItem<T>> _items;

        public IEnumerable<string> Keys
        {
            get { return _items.Keys.ToArray(); }
        }

        public Tree()
        {
            _items = new Dictionary<string, TreeItem<T>> {{string.Empty, new TreeItem<T>(null)}};
        }

        public void AddItem(string path, T content)
        {
            AddItem(path, content, null);
        }

        private void AddItem(string path, T content, TreeItem<T> newChild)
        {
            TreeItem<T> item;
            var newItem = false;
            if (!_items.ContainsKey(path))
            {
                item = new TreeItem<T>(content);
                _items.Add(path, item);
                newItem = true;
            }
            else
            {
                item = _items[path];
            }

            if (newChild != null) item.Children.Add(newChild);

            if (path == string.Empty) return;

            var parent = path.GetParentPath();
            AddItem(parent, null, newItem ? item: null);
        }

        public bool ItemHasContent(string path)
        {
            return GetItem(path) != null;
        }

        public T GetItem(string path)
        {
            T content;
            if (!TryGetItem(path, out content)) throw new ArgumentException("Unregistered path: " + path);
            return content;
        }

        public bool TryGetItem(string path, out T content)
        {
            if (_items.ContainsKey(path))
            {
                content = _items[path].Content;
                return true;
            }
            content = null;
            return false;
        }

        public void UpdateItem(string path, T content, bool createIfNotExists = false)
        {
            if (!_items.ContainsKey(path))
            {
                if (createIfNotExists)
                {
                    AddItem(path, content);
                }
                else
                {
                    throw new ArgumentException("Unregistered path: " + path);    
                }
            }
            _items[path].Content = content;
        }

        public List<T> GetChildren(string path)
        {
            if (!_items.ContainsKey(path)) throw new ArgumentException("Unregistered path: " + path);
            return _items[path].Children.Select(c => c.Content).ToList();
        }
   
    }

}