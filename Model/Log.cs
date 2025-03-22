using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 日志
    /// </summary>
    [Table("Logs")]
    public class Log : CommEntity
    {
        public int LogType { get; set; }//日志类型
        public string LogStr { get; set; }//日志内容

    }
}
