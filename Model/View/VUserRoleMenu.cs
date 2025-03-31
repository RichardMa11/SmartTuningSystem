namespace Model.View
{
    public class VUserRoleMenu
    {
        public string UserName { get; set; }
        public string UserNo { get; set; }
        public string PageName { get; set; }//页面名称
        public string PagePath { get; set; }//页面路径
        public string Icon { get; set; }
        public int? Order { get; set; }//排序
        public int? MenuId { get; set; }
    }
}
