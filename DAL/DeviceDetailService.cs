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
