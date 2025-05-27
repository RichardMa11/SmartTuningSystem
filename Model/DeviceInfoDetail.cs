using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 设备明细
    /// </summary>
    [Table("DeviceInfoDetail")]
    public class DeviceInfoDetail : CommEntity
    {
        //点号（编号）
        public string PointName { get; set; }
        //夹序号（槽位）
        public string PointPos { get; set; }

        public string PointDescription { get; set; }
        //参数地址
        public string PointAddress { get; set; }

        public int DeviceId { get; set; }
    }
}
