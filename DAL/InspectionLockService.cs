using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    public class InspectionLockService
    {
        /// <summary>
        /// 为了防止Lock数据太多（sql express 最大容量10G)
        /// </summary>
        /// <param name="inspectionLock"></param>
        /// <returns></returns>
        public int InsertLock(InspectionLock inspectionLock)
        {
            InspectionLock tmp;
            using (var context = new CoreDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //加入数据库
                        var timeTmp = DateTime.Now;
                        tmp = context.InspectionLock.Add(new InspectionLock
                        {
                            LockName = inspectionLock.LockName,
                            IpAddress = inspectionLock.IpAddress,
                            PointAddress = inspectionLock.PointAddress,
                            LockValue = inspectionLock.LockValue,
                            CreateName = inspectionLock.CreateName,
                            CreateNo = inspectionLock.CreateNo,
                            CreateTime = timeTmp,
                            UpdateName = inspectionLock.CreateName,
                            UpdateNo = inspectionLock.CreateNo,
                            UpdateTime = timeTmp,
                            IsValid = true
                        });
                        context.SaveChanges();

                        var cutoffDate = DateTime.Now.AddMonths(-1);
                        var tmpLocks = context.InspectionLock.Where(e => e.CreateTime < cutoffDate).ToList();
                        if (tmpLocks.Count > 0)
                        {
                            context.InspectionLock.RemoveRange(tmpLocks);
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

        public List<InspectionLock> SelectLocks(string lockName)
        {
            List<InspectionLock> locks;
            using (CoreDbContext context = new CoreDbContext())
            {
                locks = context.InspectionLock.Where(e => e.LockName == lockName && e.IsValid).ToList();
            }

            return locks;
        }

        public List<InspectionLock> SelectAllLocks()
        {
            List<InspectionLock> locks;
            using (CoreDbContext context = new CoreDbContext())
            {
                locks = context.InspectionLock.Where(e => e.IsValid).ToList();
            }

            return locks;
        }

    }
}
