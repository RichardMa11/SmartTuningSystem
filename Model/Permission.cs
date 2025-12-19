using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    /// <summary>
    /// 权限表
    /// </summary>
    [Table("Permission")]
    public class Permission : CommEntity
    {
        [MaxLength(50)]
        public string PermissionCode { get; set; }

        [MaxLength(500)]
        public string PermissionName { get; set; }
    }

    public class UserPermissionDto
    {
        public string UserName { get; set; }
        public string UserNo { get; set; }
        public string PermissionCode { get; set; }
    }
}
