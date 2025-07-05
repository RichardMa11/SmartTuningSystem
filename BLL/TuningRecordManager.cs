using System;
using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class TuningRecordManager
    {
        public readonly TuningRecordService TuningRecordService = new TuningRecordService();

        public void AddTuningRecord(TuningRecord tuningRecord)
        {
            TuningRecordService.InsertTuningRecord(tuningRecord);
            //TuningRecordService.InsertTuningRecordNew(tuningRecord);
        }

        public (List<TuningRecord>, int) GetPagedTuningRecords(DateTime? startTime, DateTime? endTime, string keyword, int pageIndex, int pageSize)
        {
            return TuningRecordService.QueryPagedTuningRecords(startTime, endTime, keyword, pageIndex, pageSize);

            //if (startTime == null || endTime == null)
            //    return TuningRecordService.QueryPagedTuningRecords(startTime, endTime, keyword, pageIndex, pageSize);

            //return TuningRecordService.QueryLogsAsync(Convert.ToDateTime(startTime), Convert.ToDateTime(endTime), keyword, pageIndex, pageSize).Result;
        }
    }
}
