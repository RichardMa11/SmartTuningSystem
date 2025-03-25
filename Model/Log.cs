using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 日志
    /// </summary>
    [Table("Logs")]
    public class Log : CommEntity
    {
        public LogLevel LogType { get; set; }//日志类型
        public string LogStr { get; set; }//日志内容

        public enum LogLevel { Info = 0, Error = 1, Warning = 2, Authorization = 3, Operation = 4 }//0:系统日志 1:系统错误  2:系统警告  3:授权日志  4:操作记录
    }
}
