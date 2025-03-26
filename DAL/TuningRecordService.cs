using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}
