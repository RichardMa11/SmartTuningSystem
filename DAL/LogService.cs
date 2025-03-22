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
    }
}
