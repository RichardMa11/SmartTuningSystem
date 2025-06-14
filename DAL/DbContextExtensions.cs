using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DAL
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// 判断数据表是否存在
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tableName">表名称</param>
        /// <returns></returns>
        public static bool ExistTable(this DbContext context, string tableName)
        {
            var countResult = context.Database.SqlQuery(typeof(int),
                    $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='{tableName}'")
                .ToListAsync().Result;
            if (countResult == null || countResult.Count == 0) return false;
            if (!int.TryParse(countResult[0].ToString(), out var count)) return false;
            return count > 0;
        }


        public static List<T> CommSqlStrQuery<T>(string strSql) where T : class // 确保 T 是引用类型（class）
        {
            List<T> list;
            using (CoreDbContext context = new CoreDbContext())
            {
                list = context.Database.SqlQuery<T>(strSql).ToList();
            }

            return list;
        }

        public static int CommSqlStrExecute(string strSql)
        {
            int i = 0;
            using (CoreDbContext context = new CoreDbContext())
            {
                i = context.Database.ExecuteSqlCommand(strSql);
            }

            return i;
        }
    }
}
