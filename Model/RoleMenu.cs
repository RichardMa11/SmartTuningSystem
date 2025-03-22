using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 角色-菜单
    /// </summary>
    [Table("RoleMenu")]
    public class RoleMenu : CommEntity
    {
        public int RoleId { get; set; }//系统角色

        public int MenuId { get; set; }//系统菜单
    }
}
