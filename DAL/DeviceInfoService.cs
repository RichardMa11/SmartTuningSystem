using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace DAL
{
    public class DeviceInfoService
    {
        public int InsertDevice(DeviceInfo device)
        {
            DeviceInfo tmp;
            using (CoreDbContext context = new CoreDbContext())
            {
                //加入数据库
                var timeTmp = DateTime.Now;
                tmp = context.DeviceInfo.Add(new DeviceInfo
                {
                    DeviceName = device.DeviceName,
                    IpAddress = device.IpAddress,
                    ProductName = device.ProductName,
                    CreateName = device.CreateName,
                    CreateNo = device.CreateNo,
                    CreateTime = timeTmp,
                    UpdateName = device.CreateName,
                    UpdateNo = device.CreateNo,
                    UpdateTime = timeTmp,
                    IsValid = true
                });

                context.SaveChanges();
            }

            return tmp.Id;
        }

        public void UpdateDevice(DeviceInfo device)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.DeviceInfo.Single(c => c.Id == device.Id);
                model.DeviceName = device.DeviceName;
                model.IpAddress = device.IpAddress;
                model.ProductName = device.ProductName;
                model.UpdateName = device.UpdateName;
                model.UpdateNo = device.UpdateNo;
                model.UpdateTime = DateTime.Now;

                context.SaveChanges();
            }
        }

        public void DeleteDevice(int id)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var device = context.DeviceInfo.FirstOrDefault(c => c.Id == id);
                if (device == null) return;
                context.DeviceInfo.Remove(device);
                context.SaveChanges();
            }
        }

        public void LogicDeleteDevice(DeviceInfo device)
        {
            using (CoreDbContext context = new CoreDbContext())
            {
                var model = context.DeviceInfo.Single(c => c.Id == device.Id);
                model.DelName = device.DelName;
                model.DelNo = device.DelNo;
                model.DelTime = DateTime.Now;
                model.IsValid = false;

                context.SaveChanges();
            }
        }

        public (List<DeviceInfo>, int) QueryPagedDeviceInfo(string keyword, int pageIndex, int pageSize)
        {
            using (var db = new CoreDbContext())
            {
                var query = db.DeviceInfo.AsQueryable();
                query = query.Where(x => x.IsValid);

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(x =>
                        x.DeviceName.Contains(keyword) ||
                        x.IpAddress.Contains(keyword) ||
                        x.ProductName.Contains(keyword));

                // 分页处理
                int total = query.Count();
                var data = query.OrderByDescending(x => x.CreateTime)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return (data, total);
            }
        }

        public DeviceInfo SelectDeviceById(int deviceId)
        {
            DeviceInfo device;
            using (CoreDbContext context = new CoreDbContext())
            {
                device = context.DeviceInfo.First(c => c.Id == deviceId && c.IsValid);
            }

            return device;
        }

        public List<DeviceInfo> SelectDevice(int id = 0, string device = "", string ip = "", string product = "")
        {
            List<DeviceInfo> devices;
            using (CoreDbContext context = new CoreDbContext())
            {
                var query = context.DeviceInfo.AsQueryable();
                query = query.Where(x => x.IsValid);

                if (id != 0)
                    query = query.Where(x => x.Id == id);

                if (!string.IsNullOrEmpty(device))
                    query = query.Where(x => x.DeviceName == device);

                if (!string.IsNullOrEmpty(ip))
                    query = query.Where(x => x.IpAddress == ip);

                if (!string.IsNullOrEmpty(product))
                    query = query.Where(x => x.ProductName == product);

                devices = query.OrderByDescending(x => x.DeviceName).ToList();
            }

            return devices;
        }
    }
}
