using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    public class LogService
    {
        public int InsertLog(Log log)
        {
            Log tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.Logs.Add(new Log
                {
                    LogStr = log.LogStr,
                    LogType = log.LogType,
                    CreateName = log.CreateName,
                    CreateNo = log.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = log.CreateName,
                    UpdateNo = log.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        /// <summary>
        /// 为了防止日志太多（sql express 最大容量10G)
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public int InsertLogNew(Log log)
        {
            Log tmp;
            using (var context = new CoreDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //加入数据库
                        var timeTmp = DateTime.Now;
                        tmp = context.Logs.Add(new Log
                        {
                            LogStr = log.LogStr,
                            LogType = log.LogType,
                            CreateName = log.CreateName,
                            CreateNo = log.CreateNo,
                            CreateTime = timeTmp,
                            UpdateName = log.CreateName,
                            UpdateNo = log.CreateNo,
                            UpdateTime = timeTmp,
                            IsValid = true
                        });
                        context.SaveChanges();

                        var cutoffDate = DateTime.Now.AddMonths(-3);
                        var tmpLogs = context.Logs.Where(e => e.CreateTime < cutoffDate).ToList();
                        if (tmpLogs.Count > 0)
                        {
                            context.Logs.RemoveRange(tmpLogs);
                            context.SaveChanges();
                        }

                        // 提交事务
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务（所有 SaveChanges 操作均撤销）
                        transaction.Rollback();
                        throw new ApplicationException("事务执行失败，已回滚", ex);
                    }
                }
            }

            return tmp.Id;
        }

        public void UpdateLog(Log log)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.Logs.Single(c => c.Id == log.Id);

                context.SaveChanges();
            }
        }

        public void DeleteLog(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var log = context.Logs.FirstOrDefault(c => c.Id == id);
                if (log == null) return;
                context.Logs.Remove(log);
                context.SaveChanges();
            }
        }

        public List<Log> SelectLog(DateTime startDate, DateTime endDate)
        {
            List<Log> logs;
            using (CoreDbContext context = new CoreDbContext())
            {
                logs = context.Logs.Where(e => e.CreateTime >= startDate && e.CreateTime <= endDate && e.IsValid).ToList();
            }

            return logs;
        }

        public List<Log> SqlStrQueryLog(string strSql)
        {
            List<Log> logs;
            using (CoreDbContext context = new CoreDbContext())
            {
                logs = context.Logs.SqlQuery(strSql).ToList();
            }

            return logs;
        }

        public (List<Log>, int) QueryPagedLogs(DateTime? startTime, DateTime? endTime, int? logType, string keyword, int pageIndex, int pageSize)
        {
            using (var db = new CoreDbContext())
            {
                var query = db.Logs.AsQueryable();

                // 条件筛选
                if (startTime != null)
                    query = query.Where(x => x.CreateTime >= startTime);

                if (endTime != null)
                    query = query.Where(x => x.CreateTime <= endTime);

                if (logType != null)
                    query = query.Where(x => (int)x.LogType == logType);

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(x =>
                    x.LogStr.Contains(keyword) ||
                    x.CreateName.Contains(keyword) ||
                    x.CreateNo.Contains(keyword));

                // 分页处理
                int total = query.Count();
                var data = query.OrderByDescending(x => x.CreateTime)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return (data, total);
            }
        }

    }
}
