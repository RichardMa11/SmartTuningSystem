using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            txtAllowedRange.Text = SysConfigManager.GetSysConfigByKey("AllowedRange").FirstOrDefault()?.Value ?? string.Empty;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string version = txtVersion.Text;
            string updateUrl = txtUpdateUrl.Text;
            string allowedRange = txtAllowedRange.Text;//Convert.ToDouble(txtAllowedRange.Text);

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

            if (string.IsNullOrEmpty(allowedRange))
            {
                MessageBoxX.Show("请输入智能调机推荐值范围", "空值提醒");
                txtAllowedRange.Focus();
                return;
            }

            var tempVersion = SysConfigManager.GetSysConfigByKey("UpdateVersion").FirstOrDefault();
            var tempUrl = SysConfigManager.GetSysConfigByKey("UpdateUrl").FirstOrDefault();
            var tempRange = SysConfigManager.GetSysConfigByKey("AllowedRange").FirstOrDefault();
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

            if (tempRange == null)
            {
                SysConfigManager.AddSysConfig(new SysConfig
                {
                    Key = "AllowedRange",
                    Value = allowedRange,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo,
                    Remark = "智能调机推荐值范围"
                });
            }
            else
            {
                SysConfigManager.ModifySysConfig(new SysConfig
                {
                    Id = tempRange.Id,
                    Key = "AllowedRange",
                    Value = allowedRange,
                    UpdateName = UserGlobal.CurrUser.UserName,
                    UpdateNo = UserGlobal.CurrUser.UserNo
                });
            }

            Notice.Show($"{UserGlobal.CurrUser.UserName}设置更新完成！", "成功", 3, MessageBoxIcon.Success);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //e.Handled = !char.IsDigit(e.Text, 0); // 直接判断是否为数字字符
            //Regex regex = new Regex("^[0-9]+$");
            //e.Handled = !regex.IsMatch(e.Text);
            var textBox = (TextBox)sender;
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // 允许数字、小数点、负号
            bool isAllowedChar = char.IsDigit(e.Text, 0) || e.Text == "." || e.Text == "-";

            // 验证负号位置
            if (e.Text == "-" && textBox.SelectionStart != 0)
            {
                e.Handled = true;
                return;
            }

            // 验证小数点唯一性
            if (e.Text == "." && textBox.Text.Contains('.'))
            {
                e.Handled = true;
                return;
            }

            e.Handled = !isAllowedChar || !double.TryParse(fullText, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    e.CancelCommand();
                    Notice.Show($"只能粘贴数字格式内容","提示", 3, MessageBoxIcon.Warning);
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space; // 拦截空格键
            // 允许退格、删除、方向键等控制键
            //if (e.Key == Key.Back || e.Key == Key.Delete ||
            //    e.Key == Key.Left || e.Key == Key.Right)
            //{
            //    return;
            //}

            //// 阻止其他非数字键
            //if (!((e.Key >= Key.D0 && e.Key <= Key.D9) ||
            //      (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
            //      e.Key == Key.OemPeriod || e.Key == Key.Decimal ||
            //      e.Key == Key.OemMinus || e.Key == Key.Subtract))
            //{
            //    e.Handled = true;
            //}
        }
    }
}
