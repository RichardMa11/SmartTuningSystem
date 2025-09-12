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

        //public void InsertLock(List<InspectionLock> inspectionLocks)
        //{
        //    using (var context = new CoreDbContext())
        //    {
        //        // 获取数据库连接
        //        var connection = context.Database.GetDbConnection();
        //        bool wasOpen = connection.State == System.Data.ConnectionState.Open;
        //        if (!wasOpen)
        //        {
        //            connection.Open();
        //        }

        //        using (var transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                var timeTmp = DateTime.Now;

        //                // 创建与数据库表结构匹配的DataTable
        //                var dataTable = new System.Data.DataTable();
        //                dataTable.Columns.Add("LockName", typeof(string));
        //                dataTable.Columns.Add("IpAddress", typeof(string));
        //                dataTable.Columns.Add("PointAddress", typeof(string));
        //                dataTable.Columns.Add("LockValue", typeof(string));
        //                dataTable.Columns.Add("CreateName", typeof(string));
        //                dataTable.Columns.Add("CreateNo", typeof(string));
        //                dataTable.Columns.Add("CreateTime", typeof(DateTime));
        //                dataTable.Columns.Add("UpdateName", typeof(string));
        //                dataTable.Columns.Add("UpdateNo", typeof(string));
        //                dataTable.Columns.Add("UpdateTime", typeof(DateTime));
        //                dataTable.Columns.Add("IsValid", typeof(bool));

        //                // 批量添加数据到DataTable
        //                foreach (var inspectionLock in inspectionLocks)
        //                {
        //                    dataTable.Rows.Add(
        //                        inspectionLock.LockName,
        //                        inspectionLock.IpAddress,
        //                        inspectionLock.PointAddress,
        //                        inspectionLock.LockValue,
        //                        inspectionLock.CreateName,
        //                        inspectionLock.CreateNo,
        //                        timeTmp,
        //                        inspectionLock.CreateName,
        //                        inspectionLock.CreateNo,
        //                        timeTmp,
        //                        true
        //                    );
        //                }

        //                // 使用SqlBulkCopy执行批量插入
        //                using (var bulkCopy = new System.Data.SqlClient.SqlBulkCopy(
        //                    (System.Data.SqlClient.SqlConnection)connection,
        //                    System.Data.SqlClient.SqlBulkCopyOptions.Default,
        //                    (System.Data.SqlClient.SqlTransaction)transaction))
        //                {
        //                    bulkCopy.DestinationTableName = "InspectionLock";
        //                    bulkCopy.WriteToServer(dataTable);
        //                }

        //                // 清理一个月前的数据
        //                var cutoffDate = DateTime.Now.AddMonths(-1);
        //                var deleteCommand = connection.CreateCommand();
        //                deleteCommand.Transaction = transaction;
        //                deleteCommand.CommandText = "DELETE FROM InspectionLock WHERE CreateTime < @CutoffDate";

        //                var parameter = deleteCommand.CreateParameter();
        //                parameter.ParameterName = "@CutoffDate";
        //                parameter.Value = cutoffDate;
        //                deleteCommand.Parameters.Add(parameter);

        //                deleteCommand.ExecuteNonQuery();

        //                // 提交事务
        //                transaction.Commit();
        //            }
        //            catch (Exception ex)
        //            {
        //                // 回滚事务
        //                transaction.Rollback();
        //                throw new ApplicationException("批量插入执行失败，已回滚", ex);
        //            }
        //            finally
        //            {
        //                if (!wasOpen && connection.State == System.Data.ConnectionState.Open)
        //                {
        //                    connection.Close();
        //                }
        //            }
        //        }
        //    }
        //}

        public void BulkInsertLock(List<InspectionLock> inspectionLocks)
        {
            using (var context = new CoreDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var timeTmp = DateTime.Now;
                        // 准备批量数据
                        var newLocks = inspectionLocks.Select(lockItem => new InspectionLock
                        {
                            LockName = lockItem.LockName,
                            IpAddress = lockItem.IpAddress,
                            PointAddress = lockItem.PointAddress,
                            LockValue = lockItem.LockValue,
                            CreateName = lockItem.CreateName,
                            CreateNo = lockItem.CreateNo,
                            CreateTime = timeTmp,
                            UpdateName = lockItem.CreateName,
                            UpdateNo = lockItem.CreateNo,
                            UpdateTime = timeTmp,
                            IsValid = true
                        }).ToList();

                        // 批量添加
                        context.InspectionLock.AddRange(newLocks);
                        context.SaveChanges();

                        // 清理旧数据
                        var cutoffDate = DateTime.Now.AddMonths(-1);
                        context.InspectionLock.RemoveRange(
                            context.InspectionLock.Where(e => e.CreateTime < cutoffDate)
                        );
                        context.SaveChanges();
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
