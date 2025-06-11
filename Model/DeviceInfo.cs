using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    ///设备信息
    /// </summary>
    [Table("DeviceInfo")]
    public class DeviceInfo : CommEntity
    {
        //机台号
        public string DeviceName { get; set; }

        public string IpAddress { get; set; }

        //产品品名
        public string ProductName { get; set; }
    }

    public class DeviceModel
    {
        //机台号
        public string DeviceName { get; set; }

        public string IpAddress { get; set; }
    }

    public class DeviceInfoModel
    {
        //机台号
        public string DeviceName { get; set; }

        public string IpAddress { get; set; }

        //产品品名
        public string ProductName { get; set; }

        //点号（编号）
        public string PointName { get; set; }

        //夹序号（槽位）
        public string PointPos { get; set; }

        //参数地址
        public string PointAddress { get; set; }
    }
}
