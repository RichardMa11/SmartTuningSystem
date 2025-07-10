using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BLL;
using Model;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// GeneralSetting.xaml 的交互逻辑
    /// </summary>
    public partial class GeneralSetting : Page
    {
        public readonly SysConfigManager SysConfigManager = new SysConfigManager();
        public GeneralSetting()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开系统管理成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开系统管理成功！", LogLevel.Operation);

            txtVersion.Text = SysConfigManager.GetSysConfigByKey("UpdateVersion").FirstOrDefault()?.Value ?? string.Empty;
            txtUpdateUrl.Text = SysConfigManager.GetSysConfigByKey("UpdateUrl").FirstOrDefault()?.Value ?? string.Empty;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string version = txtVersion.Text;
            string updateUrl = txtUpdateUrl.Text;

            if (string.IsNullOrEmpty(version))
            {
                MessageBoxX.Show("请输入服务器上的版本号", "空值提醒");
                txtVersion.Focus();
                return;
            }

            if (string.IsNullOrEmpty(updateUrl))
            {
                MessageBoxX.Show("请输入升级压缩包的路径", "空值提醒");
                txtUpdateUrl.Focus();
                return;
            }

            var tempVersion = SysConfigManager.GetSysConfigByKey("UpdateVersion").FirstOrDefault();
            var tempUrl = SysConfigManager.GetSysConfigByKey("UpdateUrl").FirstOrDefault();
            if (tempVersion == null)
            {
                SysConfigManager.AddSysConfig(new SysConfig
                {
                    Key = "UpdateVersion",
                    Value = version,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo,
                    Remark = "服务器上的版本号"
                });
            }
            else
            {
                SysConfigManager.ModifySysConfig(new SysConfig
                {
                    Id = tempVersion.Id,
                    Key = "UpdateVersion",
                    Value = version,
                    UpdateName = UserGlobal.CurrUser.UserName,
                    UpdateNo = UserGlobal.CurrUser.UserNo
                });
            }

            if (tempUrl == null)
            {
                SysConfigManager.AddSysConfig(new SysConfig
                {
                    Key = "UpdateUrl",
                    Value = updateUrl,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo,
                    Remark = "升级压缩包的URL"
                });
            }
            else
            {
                SysConfigManager.ModifySysConfig(new SysConfig
                {
                    Id = tempUrl.Id,
                    Key = "UpdateUrl",
                    Value = updateUrl,
                    UpdateName = UserGlobal.CurrUser.UserName,
                    UpdateNo = UserGlobal.CurrUser.UserNo
                });
            }

            Notice.Show($"{UserGlobal.CurrUser.UserName}设置更新完成！", "成功", 3, MessageBoxIcon.Success);
        }
    }
}
