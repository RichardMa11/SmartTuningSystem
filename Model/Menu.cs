using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    //菜单
    [Table("Menus")]
    public class Menu : CommEntity
    {
        [MaxLength(50)]
        public string PageName { get; set; }//页面名称
        [MaxLength(500)]
        public string PagePath { get; set; }//页面路径
        [MaxLength(20)]
        public string Icon { get; set; }
        public int Order { get; set; }//排序

        [NotMapped]
        public string FullPath { get; set; }
    }
}
