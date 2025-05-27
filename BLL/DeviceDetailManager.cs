using System.Collections.Generic;
using DAL;
using Model;

namespace BLL
{
    public class DeviceDetailManager
    {
        public readonly DeviceDetailService DeviceDetailService = new DeviceDetailService();

        public int AddDeviceDetail(DeviceInfoDetail detail)
        {
            return DeviceDetailService.InsertDeviceDetail(detail);
        }

        public void ModifyDeviceDetail(DeviceInfoDetail detail)
        {
            DeviceDetailService.UpdateDeviceDetail(detail);
        }

        public void RemoveDeviceDetail(DeviceInfoDetail detail)
        {
            DeviceDetailService.LogicDeleteDeviceDetail(detail);
        }

        public void DeleteDetail(int id)
        {
            DeviceDetailService.DeleteDetail(id);
        }

        public (List<DeviceInfoDetail>, int) GetPagedDeviceDetail(string keyword, int pageIndex, int pageSize)
        {
            return DeviceDetailService.QueryPagedDeviceDetail(keyword, pageIndex, pageSize);
        }
    }
}
