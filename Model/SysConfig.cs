using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 系统配置
    /// </summary>
    [Table("SysConfig")]
    public class SysConfig : CommEntity
    {
        [MaxLength(500)]
        public string Key { get; set; }

        [MaxLength(4000)]
        public string Value { get; set; }
    }
}
