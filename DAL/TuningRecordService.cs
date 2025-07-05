using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Model;

namespace DAL
{
    public class TuningRecordService
    {
        public int InsertTuningRecord(TuningRecord tuningRecord)
        {
            TuningRecord tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.TuningRecord.Add(new TuningRecord
                {
                    DeviceName = tuningRecord.DeviceName,
                    ProductName = tuningRecord.ProductName,
                    SendStr = tuningRecord.SendStr,
                    BefParam = tuningRecord.BefParam,
                    CreateName = tuningRecord.CreateName,
                    CreateNo = tuningRecord.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = tuningRecord.CreateName,
                    UpdateNo = tuningRecord.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        public void UpdateTuningRecord(TuningRecord tuningRecord)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.TuningRecord.Single(c => c.Id == tuningRecord.Id);

                context.SaveChanges();
            }
        }

        public void DeleteTuningRecord(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var tuningRecord = context.TuningRecord.FirstOrDefault(c => c.Id == id);
                if (tuningRecord == null) return;
                context.TuningRecord.Remove(tuningRecord);
                context.SaveChanges();
            }
        }

        public List<TuningRecord> SelectTuningRecord(DateTime startDate, DateTime endDate)
        {
            List<TuningRecord> tuningRecords;
            using (CoreDbContext context = new CoreDbContext())
            {
                tuningRecords = context.TuningRecord.Where(e => e.CreateTime >= startDate && e.CreateTime <= endDate && e.IsValid).ToList();
            }

            return tuningRecords;
        }

        public List<TuningRecord> SqlStrQueryTuningRecord(string strSql)
        {
            List<TuningRecord> tuningRecords;
            using (CoreDbContext context = new CoreDbContext())
            {
                tuningRecords = context.TuningRecord.SqlQuery(strSql).ToList();
            }

            return tuningRecords;
        }

        public (List<TuningRecord>, int) QueryPagedTuningRecords(DateTime? startTime, DateTime? endTime, string keyword, int pageIndex, int pageSize)
        {
            using (var db = new CoreDbContext())
            {
                var query = db.TuningRecord.AsQueryable();

                // 条件筛选
                if (startTime != null)
                    query = query.Where(x => x.CreateTime >= startTime);

                if (endTime != null)
                    query = query.Where(x => x.CreateTime <= endTime);

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(x =>
                    x.DeviceName.Contains(keyword) ||
                    x.ProductName.Contains(keyword) ||
                    x.BefParam.Contains(keyword) ||
                    x.SendStr.Contains(keyword) ||
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

        #region 分库逻辑 

        //分库新增
        public int InsertTuningRecordNew(TuningRecord tuningRecord)
        {
            TuningRecord tmp;
            using (LogDbContext context = new LogDbContext(GetConnectionString(GetDatabaseName(DateTime.Now))))
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.TuningRecord.Add(new TuningRecord
                {
                    DeviceName = tuningRecord.DeviceName,
                    ProductName = tuningRecord.ProductName,
                    SendStr = tuningRecord.SendStr,
                    BefParam = tuningRecord.BefParam,
                    CreateName = tuningRecord.CreateName,
                    CreateNo = tuningRecord.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = tuningRecord.CreateName,
                    UpdateNo = tuningRecord.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        //public MultiDbLogService(string baseConnStr)
        //{
        //    _baseConnStr = baseConnStr + ";MultipleActiveResultSets=true;";
        //}

        public async Task<(List<TuningRecord>, int)> QueryLogsAsync(
            DateTime start, DateTime end, string keyword, int pageIndex, int pageSize)
        {
            var databases = GetRelevantDatabases(start, end);
            var mergedResults = new ConcurrentBag<TuningRecord>();
            var totalCount = 0;

            try
            {
                await Task.WhenAll(databases.Select(async db =>
                {
                    using (var context = new LogDbContext(GetConnectionString(db)))
                    {
                        var query = context.TuningRecord.AsQueryable();
                        query = query.Where(x => x.CreateTime >= start && x.CreateTime <= end)
                            .OrderByDescending(x => x.CreateTime);

                        if (!string.IsNullOrEmpty(keyword))
                            query = query.Where(x =>
                                x.DeviceName.Contains(keyword) ||
                                x.ProductName.Contains(keyword) ||
                                x.BefParam.Contains(keyword) ||
                                x.SendStr.Contains(keyword) ||
                                x.CreateName.Contains(keyword) ||
                                x.CreateNo.Contains(keyword));

                        Interlocked.Add(ref totalCount, await query.CountAsync().ConfigureAwait(false));

                        var pageData = await query
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .AsNoTracking()
                            .ToListAsync().ConfigureAwait(false);

                        foreach (var item in pageData)
                        {
                            mergedResults.Add(item);
                        }
                    }
                })).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("异步查询调机日志报错！", ex);
            }

            //var data = mergedResults
            //    .OrderByDescending(x => x.CreateTime)
            //    .Skip((pageIndex - 1) * pageSize)  // 添加分页跳过
            //    .Take(pageSize)
            //    .ToList();

            return (mergedResults
                .OrderByDescending(x => x.CreateTime)
                .Skip((pageIndex - 1) * pageSize)  // 添加分页跳过
                .Take(pageSize)
                .ToList(), totalCount);
        }

        private List<string> GetRelevantDatabases(DateTime start, DateTime end)
        {
            using (var context = new LogDbContext(GetConnectionString(GetDatabaseName(DateTime.Now))))
            {
                return context.Database.SqlQuery<string>(
                    @"SELECT name FROM sys.databases 
                  WHERE name LIKE 'LogDB_%'")
                    .Where(db => IsRelevantDb(db, start, end)).ToList();
            }
        }

        private bool IsRelevantDb(string dbName, DateTime start, DateTime end)
        {
            var datePart = Convert.ToInt32(dbName.Replace("LogDB_", "").Replace("Q", ""));

            return Convert.ToInt32(GetDatabaseName(start).Replace("LogDB_", "").Replace("Q", "")) <= datePart
                   && datePart <= Convert.ToInt32(GetDatabaseName(end).Replace("LogDB_", "").Replace("Q", ""));
        }

        public static string GetConnectionString(string dataBaseName)
        {
            return DbConnections.Get().Replace("SmartTuningSystemDB", dataBaseName);
        }

        // 按季度路由
        public static string GetDatabaseName(DateTime logTime)
        {
            int quarter = (logTime.Month - 1) / 3 + 1;
            return $"LogDB_{logTime.Year}Q{quarter}";  // 返回 "LogDB_2025Q1"
        }
        #endregion
    }
}
