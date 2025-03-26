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
        }

        public (List<TuningRecord>, int) GetPagedTuningRecords(DateTime? startTime, DateTime? endTime, string keyword, int pageIndex, int pageSize)
        {
            return TuningRecordService.QueryPagedTuningRecords(startTime, endTime, keyword, pageIndex, pageSize);
        }
    }
}
