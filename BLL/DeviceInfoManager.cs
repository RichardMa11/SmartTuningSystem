using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class DeviceInfoManager
    {
        public readonly DeviceInfoService DeviceInfoService = new DeviceInfoService();

        public int AddDevice(DeviceInfo device)
        {
            return DeviceInfoService.InsertDevice(device);
        }

        public void ModifyDevice(DeviceInfo device)
        {
            DeviceInfoService.UpdateDevice(device);
        }

        public void RemoveDevice(DeviceInfo device)
        {
            DeviceInfoService.LogicDeleteDevice(device);
        }

        public void DeleteMenu(int id)
        {
            DeviceInfoService.DeleteDevice(id);
        }

        public (List<DeviceInfo>, int) GetPagedDeviceInfo(string keyword, int pageIndex, int pageSize)
        {
            return DeviceInfoService.QueryPagedDeviceInfo(keyword, pageIndex, pageSize);
        }

        public DeviceInfo GetDeviceById(int deviceId)
        {
            return DeviceInfoService.SelectDeviceById(deviceId);
        }

        public List<DeviceInfo> GetDeviceByParam(int id = 0, string device = "", string ip = "", string product = "")
        {
            return DeviceInfoService.SelectDevice(id, device, ip, product);
        }

        public List<DeviceInfo> GetAllDevice()
        {
            return DeviceInfoService.SelectAllDevice();
        }
    }
}
