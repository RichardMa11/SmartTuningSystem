using System;
using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class LogManager
    {
        public readonly LogService LogService = new LogService();

        public void AddLog(Log log)
        {
            LogService.InsertLog(log);
        }

        public List<Log> GetLogByDate(DateTime startDate, DateTime endDate)
        {
            return LogService.SelectLog(startDate, endDate);
        }

        public List<Log> GetLogBySql(string strSql)
        {
            return LogService.SqlStrQueryLog(strSql);
        }

        public static List<T> QueryBySql<T>(string strSql) where T : class
        {
            return DbContextExtensions.CommSqlStrQuery<T>(strSql);
        }

        public (List<Log>, int) GetPagedLogs(DateTime? startTime, DateTime? endTime, int? logType, string keyword, int pageIndex, int pageSize)
        {
            return LogService.QueryPagedLogs(startTime, endTime, logType, keyword, pageIndex, pageSize);
        }
    }
}
