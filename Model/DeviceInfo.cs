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
}
