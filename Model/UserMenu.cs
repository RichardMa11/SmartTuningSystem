using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 人员-菜单
    /// </summary>
    [Table("UserMenu")]
    public class UserMenu : CommEntity
    {
        public int MenuId { get; set; }//系统菜单

        public int UserId { get; set; }//人员ID
    }
}
