using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class InspectionLockManager
    {
        public readonly InspectionLockService InspectionLockService = new InspectionLockService();

        public void AddLock(InspectionLock inspectionLock)
        {
            InspectionLockService.InsertLock(inspectionLock);
        }

        public void BulkAddLock(List<InspectionLock> inspectionLocks)
        {
            InspectionLockService.BulkInsertLock(inspectionLocks);
        }

        public List<InspectionLock> GetLockByLockName(string lockName)
        {
            return InspectionLockService.SelectLocks(lockName);
        }

        public List<InspectionLock> GetAllLock()
        {
            return InspectionLockService.SelectAllLocks();
        }

    }
}
