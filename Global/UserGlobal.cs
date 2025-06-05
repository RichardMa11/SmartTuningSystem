using System.Collections.Generic;
using System.ComponentModel;
using Model.View;
using SmartTuningSystem.View;

namespace SmartTuningSystem.Global
{
    /// <summary>
    /// 全局用户数据
    /// </summary>
    public partial class UserGlobal
    {
        /// <summary>
        /// 数据表
        /// </summary>
        public static Model.User CurrUser { get; set; }

        /// <summary>
        /// 当前账户权限下的模块
        /// </summary>
        public static List<VUserRoleMenu> VUserRoleMenus = new List<VUserRoleMenu>();

        public static MainWindow MainWindow { get; set; }

        ///// <summary>
        ///// 当前账户权限下的插件
        ///// </summary>
        //public static List<Plugins> Plugins = new List<Plugins>();
        ///// <summary>
        ///// 当前账户权限下的模块
        ///// </summary>
        //public static List<PluginsModule> PluginsModules = new List<PluginsModule>();
        ///// <summary>
        ///// 当前账户权限下的页面
        ///// </summary>
        //public static List<ModulePage> ModulePages = new List<ModulePage>();

        //public static CoreSetting CoreSetting { get; set; }

        ///// <summary>
        ///// 设置当前用户的信息
        ///// </summary>
        //public static void SetCurrUser(User user, CoreSetting coreSetting)
        //{
        //    IsLogin = true;
        //    CurrUser = user;
        //    CoreSetting = coreSetting;
        //}


        /// <summary>
        /// 是否已经登录
        /// </summary>
        public static bool IsLogin = false;
    }


    public class GlobalData : INotifyPropertyChanged
    {
        public static GlobalData Instance { get; } = new GlobalData();
        private bool _isDataValid;
        public bool IsDataValid
        {
            get => _isDataValid;
            set { _isDataValid = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDataValid))); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
