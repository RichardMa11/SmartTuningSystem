using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class DeviceInfoManager
    {
        public readonly DeviceInfoService DeviceInfoService = new DeviceInfoService();

        //public void AddLog(Log log)
        //{
        //    LogService.InsertLog(log);
        //}

        //public List<Log> GetLogByDate(DateTime startDate, DateTime endDate)
        //{
        //    return LogService.SelectLog(startDate, endDate);
        //}

        //public List<Log> GetLogBySql(string strSql)
        //{
        //    return LogService.SqlStrQueryLog(strSql);
        //}

        //public static List<T> QueryBySql<T>(string strSql) where T : class
        //{
        //    return DbContextExtensions.CommSqlStrQuery<T>(strSql);
        //}

        public (List<DeviceInfo>, int) GetPagedDeviceInfo(string keyword, int pageIndex, int pageSize)
        {
            return DeviceInfoService.QueryPagedDeviceInfo(keyword, pageIndex, pageSize);
        }
    }
}
