using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 设备明细
    /// </summary>
    [Table("DeviceInfoDetail")]
    public class DeviceInfoDetail : CommEntity
    {
        public string PointName { get; set; }

        public string PointDescription { get; set; }

        public string PointPos { get; set; }

        public int DeviceId { get; set; }
    }
}
