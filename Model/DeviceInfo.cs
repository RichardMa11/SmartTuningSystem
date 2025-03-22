using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    ///设备信息
    /// </summary>
    [Table("DeviceInfo")]
    public class DeviceInfo : CommEntity
    {
        public string DeviceName { get; set; }

        public string IpAddress { get; set; }
    }
}
