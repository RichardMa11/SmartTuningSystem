using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 角色表
    /// </summary>
    [Table("Roles")]
    public class Role : CommEntity
    {
        [MaxLength(50)]
        public string RoleName { get; set; }

        [MaxLength(50)]
        public string RoleNo { get; set; }
    }
}
