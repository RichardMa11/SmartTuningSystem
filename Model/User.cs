using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    // 用户
    [Table("Users")]
    public class User : CommEntity
    {
        [MaxLength(50)]
        public string UserName { get; set; }

        [MaxLength(50)]
        public string UserNo { get; set; }
        [MaxLength(100)]
        public string UserPwd { get; set; }

        public bool CanLogin { get; set; }//是否允许登录
    }

    public class UserRoleDto
    {
        public string UserName { get; set; }
        public string UserNo { get; set; }
        public string RoleName { get; set; }
        public string RoleNo { get; set; }
        public DateTime CreateTime { get; set; }//创建时间
    }
}
