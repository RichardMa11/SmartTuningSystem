using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 用户-角色
    /// </summary>
    [Table("UserRole")]
    public class UserRole : CommEntity
    {
        [Column("RoleId")]
        public int RoleId { get; set; }//系统角色

        public int UserId { get; set; }//系统用户
    }
}
