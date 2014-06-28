using System;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.Database.Attributes;
using ServiceStack.DataAnnotations;

namespace Neurotoxin.Godspeed.Shell.Database
{
    public class QueryBuilder<T>
    {
        public Type TableType { get; private set; }
        public string PrimaryKey { get; private set; }
        public Type PrimaryKeyType { get; private set; }

        public QueryBuilder()
        {
            TableType = typeof (T);
            var pk = TableType.GetProperties().FirstOrDefault(pi => pi.HasAttribute<PrimaryKeyAttribute>());
            if (pk == null) return;
            PrimaryKey = pk.Name;
            PrimaryKeyType = pk.PropertyType;
        }

        public string Build(string key, params string[] fieldNames)
        {
            const string comma = ",";
            const string before = " \"";
            const string after = "\"";
            var sb = new StringBuilder("SELECT");

            if (fieldNames.Length == 0)
            {
                fieldNames = (from pi in TableType.GetProperties()
                              let a = pi.GetAttribute<IgnoreOnReadAttribute>()
                              where a == null
                              select pi.Name).ToArray();
            }

            var first = true;
            foreach (var fieldName in fieldNames)
            {
                if (!first) sb.Append(comma);
                sb.Append(before);
                sb.Append(fieldName);
                sb.Append(after);
                first = false;
            }

            sb.Append(" FROM \"");
            sb.Append(TableType.Name);
            sb.Append(after);

            if (!string.IsNullOrEmpty(key) && PrimaryKey != null)
            {
                var isString = PrimaryKeyType == typeof (string);
                sb.Append(" WHERE \"");
                sb.Append(PrimaryKey);
                sb.Append("\" = ");
                if (isString) sb.Append("'");
                sb.Append(key);
                if (isString) sb.Append("'");
            }

            return sb.ToString();
            
        }
    }
}