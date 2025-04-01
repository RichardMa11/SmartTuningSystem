using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 人员-菜单
    /// </summary>
    [Table("UserMenu")]
    public class UserMenu : CommEntity
    {
        public int UserId { get; set; }//人员ID
        public string IncreaseMenus { get; set; }//在基础权限上增加的页面Id 以，分隔
        public string DecrementMenus { get; set; }//在基础权限上减少的页面Id 以，分隔
    }
}
