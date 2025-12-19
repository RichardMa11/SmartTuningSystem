using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    public class DeviceDetailService
    {
        public int InsertDeviceDetail(DeviceInfoDetail detail)
        {
            DeviceInfoDetail tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.DeviceInfoDetail.Add(new DeviceInfoDetail
                {
                    PointName = detail.PointName,
                    PointPos = detail.PointPos,
                    PointDescription = detail.PointDescription,
                    PointAddress = detail.PointAddress,
                    DeviceId = detail.DeviceId,
                    CreateName = detail.CreateName,
                    CreateNo = detail.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = detail.CreateName,
                    UpdateNo = detail.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        public void UpdateDeviceDetail(DeviceInfoDetail detail)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.DeviceInfoDetail.Single(c => c.Id == detail.Id);
                model.PointName = detail.PointName;
                model.PointPos = detail.PointPos;
                model.PointDescription = detail.PointDescription;
                model.PointAddress = detail.PointAddress;
                model.UpdateName = detail.UpdateName;
                model.UpdateNo = detail.UpdateNo;
                model.UpdateTime = DateTime.Now;

                context.SaveChanges();
            }
        }

        public void UpdateDeviceDetail(List<DeviceInfoDetail> details)
        {
            // 1. 空值/空列表校验
            if (details == null || !details.Any())
            {
                throw new ArgumentException("待更新的设备详情列表不能为空", nameof(details));
            }

            // 2. 校验列表中实体的ID有效性（避免ID为0/负数等无效值）
            var invalidEntities = details.Where(d => d.Id <= 0).ToList();
            if (invalidEntities.Any())
            {
                var invalidIds = string.Join(",", invalidEntities.Select(d => d.Id));
                throw new ArgumentException($"以下实体的ID无效（ID需大于0）：{invalidIds}", nameof(details));
            }

            using (CoreDbContext context = new CoreDbContext())
            {
                // 3. 提取所有待更新的ID，查询数据库中对应的实体（确保基于数据库最新数据更新）
                var ids = details.Select(d => d.Id).ToList();
                var dbModels = context.DeviceInfoDetail
                    .Where(c => ids.Contains(c.Id))
                    .ToList();

                // 4. 校验是否存在「数据库中不存在的ID」（贴合原逻辑的异常行为）
                var dbIds = dbModels.Select(m => m.Id).ToList();
                var notExistIds = ids.Except(dbIds).ToList();
                if (notExistIds.Any())
                {
                    throw new InvalidOperationException($"以下ID不存在于DeviceInfoDetail表，无法更新：{string.Join(",", notExistIds)}");
                }

                // 5. 批量更新属性（IsUsedSmart设为true）
                // 若需要更新传入实体的其他属性，可在此处扩展映射逻辑
                foreach (var dbModel in dbModels)
                {
                    var inputModel = details.First(d => d.Id == dbModel.Id);
                    // 核心更新：IsUsedSmart设为true（和原逻辑一致）
                    dbModel.IsUsedSmart = inputModel.IsUsedSmart;

                    // 【可选扩展】若需同步传入实体的其他属性（比如备注、操作人等）
                    // var inputModel = details.First(d => d.Id == dbModel.Id);
                    // dbModel.Remark = inputModel.Remark; // 示例：同步备注
                    // dbModel.Operator = inputModel.Operator; // 示例：同步操作人
                }

                // 6. 提交更改到数据库
                context.SaveChanges();
            }
        }

        public void DeleteDetail(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var detail = context.DeviceInfoDetail.FirstOrDefault(c => c.Id == id);
                if (detail == null) return;
                context.DeviceInfoDetail.Remove(detail);
                context.SaveChanges();
            }
        }

        public void LogicDeleteDeviceDetail(DeviceInfoDetail detail)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.DeviceInfoDetail.Single(c => c.Id == detail.Id);
                model.DelName = detail.DelName;
                model.DelNo = detail.DelNo;
                model.DelTime = DateTime.Now;
                model.IsValid = false;

                context.SaveChanges();
            }
        }

        public DeviceInfoDetail SelectDeviceDetailById(int detailId)
        {
            DeviceInfoDetail deviceDetail;
            using (CoreDbContext context = new CoreDbContext())
            {
                deviceDetail = context.DeviceInfoDetail.First(c => c.Id == detailId && c.IsValid);
            }

            return deviceDetail;
        }

        public List<DeviceInfoDetail> SelectDeviceDetailByDevId(int devId)
        {
            List<DeviceInfoDetail> deviceDetails;
            using (CoreDbContext context = new CoreDbContext())
            {
                deviceDetails = context.DeviceInfoDetail.Where(c => c.DeviceId == devId && c.IsValid).ToList();
            }

            return deviceDetails;
        }

        public (List<DeviceInfoDetail>, int) QueryPagedDeviceDetail(string keyword, int pageIndex, int pageSize, int deviceId = 0)
        {
            using (var db = new CoreDbContext())
            {
                var query = db.DeviceInfoDetail.AsQueryable();
                query = query.Where(x => x.IsValid);

                if (deviceId != 0)
                    query = query.Where(x => x.DeviceId == deviceId);

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(x =>
                        x.PointName.Contains(keyword) ||
                        x.PointPos.Contains(keyword) ||
                        x.PointAddress.Contains(keyword));

                // 分页处理
                int total = query.Count();
                var data = query.OrderBy(x => x.CreateTime)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return (data, total);
            }
        }

    }
}
