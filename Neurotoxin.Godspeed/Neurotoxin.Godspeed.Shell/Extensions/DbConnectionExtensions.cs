using System.Collections.Generic;
using System.Data;
using Neurotoxin.Godspeed.Shell.Database;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System.Linq;
using DataException = System.Data.DataException;

namespace Neurotoxin.Godspeed.Shell.Extensions
{
    public static class DbConnectionExtensions
    {
        public static List<T> Get<T>(this IDbConnection db)
        {
            return db.Select<T>(new QueryBuilder<T>().Build(null));
        }

        public static T ReadField<T>(this IDbConnection db, string key, string fieldName)
        {
            var qb = new QueryBuilder<T>();
            var res = db.Select<T>(qb.Build(key, fieldName));
            switch (res.Count)
            {
                case 0:
                    throw new DataException(string.Format("Row with the key \"{0}\" doesn't exist in the table \"{1}\"", key, qb.TableType.Name));
                case 1:
                    return res.First();
                default:
                    throw new DataException(string.Format("Key violation error. Result set contains more than one rows. (Key: {0}, Table: {1})", key, qb.TableType.Name));
            }
        }

        public static int UpdateOnly<T>(this IDbConnection db, T model, ICollection<string> updateFields)
        {
            return db.Exec(dbCmd =>
                               {
                                   if (OrmLiteConfig.UpdateFilter != null) OrmLiteConfig.UpdateFilter(dbCmd, model);
                                   var flag = OrmLiteConfig.DialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd, updateFields);
                                   if (string.IsNullOrEmpty(dbCmd.CommandText)) return 0;
                                   var definition = ModelDefinition<T>.Definition;
                                   var param = OrmLiteConfig.DialectProvider.GetParam();
                                   dbCmd.CommandText = string.Format("{0} WHERE {1} = {2}", dbCmd.CommandText, OrmLiteConfig.DialectProvider.GetQuotedColumnName(definition.PrimaryKey.FieldName), param);
                                   OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, model);
                                   var dbDataParameter = dbCmd.CreateParameter();
                                   dbDataParameter.ParameterName = param;
                                   dbDataParameter.Value = definition.PrimaryKey.GetValue(model);
                                   dbCmd.Parameters.Add(dbDataParameter);
                                   var num = dbCmd.ExecuteNonQuery();
                                   if (flag && num == 0) throw new OptimisticConcurrencyException();
                                   return num;
                               });
        }
    }
}