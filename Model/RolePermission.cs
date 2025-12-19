using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 角色-权限
    /// </summary>
    [Table("RolePermission")]
    public class RolePermission : CommEntity
    {
        public int RoleId { get; set; }//系统角色

        public int PermissionId { get; set; }//系统权限
    }
}
