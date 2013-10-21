using Microsoft.Isam.Esent.Collections.Generic;
using System;
using ServiceStack.Text;

namespace Neurotoxin.Godspeed.Core.Caching
{
	public class EsentPersistentDictionary
	{
	    private static EsentPersistentDictionary _instance;
	    public static EsentPersistentDictionary Instance
	    {
	        get {
	            return _instance ??
	                   (_instance = new EsentPersistentDictionary(AppDomain.CurrentDomain.GetData("DataDirectory").ToString()));
	        }
	    }

		private readonly PersistentDictionary<string, string> _persistentDictionary;

		private EsentPersistentDictionary(string path)
		{
			_persistentDictionary = new PersistentDictionary<string, string>(path);
		}

	    public string[] Keys
	    {
            get
            {
                try
                {
                    return _persistentDictionary.Keys.ToArray();
                }
                catch
                {
                    return new string[0];
                }
            }
	    }

		public T Get<T>(string key)
		{
		    return _persistentDictionary[key].FromJson<T>();
		}

	    public void Put<T>(string key, T value)
	    {
            _persistentDictionary.Add(key, value.ToJson());
            _persistentDictionary.Flush();
	    }

        public void Update<T>(string key, T newvalue)
        {
            if (_persistentDictionary.ContainsKey(key)) _persistentDictionary.Remove(key);
            Put(key, newvalue);
        }

        public void Remove(string key)
        {
            if (!_persistentDictionary.ContainsKey(key)) return;
            _persistentDictionary.Remove(key);
            _persistentDictionary.Flush();
        }

	}
}