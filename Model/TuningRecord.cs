using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 调机记录
    /// </summary>
    [Table("TuningRecord")]
    public class TuningRecord : CommEntity
    {
        public string DeviceName { get; set; }

        public string ProductName { get; set; }

        public string SendStr { get; set; }//发送参数，内容

        public string BefParam { get; set; }//机台发送前参数
    }
}
