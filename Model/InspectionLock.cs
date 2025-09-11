using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 送检锁定
    /// </summary>
    [Table("InspectionLock")]
    public class InspectionLock : CommEntity
    {
        public string LockName { get; set; }

        public string IpAddress { get; set; }

        public string PointAddress { get; set; }

        public decimal? LockValue { get; set; }
    }
}
