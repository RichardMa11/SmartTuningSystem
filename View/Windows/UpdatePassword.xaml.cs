using System.Windows;
using BLL;
using Model;
using Panuon.UI.Silver;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;
using WindowExtensions = SmartTuningSystem.Extensions.WindowExtensions;

namespace SmartTuningSystem.View.Windows
{
    /// <summary>
    /// UpdatePassword.xaml 的交互逻辑
    /// </summary>
    public partial class UpdatePassword : Window
    {
        public readonly UserManager UserManager = new UserManager();
        public UpdatePassword()
        {
            InitializeComponent();
            WindowExtensions.UseCloseAnimation(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtOldPwd.Password = UserGlobal.CurrUser.UserPwd;
            txtNewPwd.Clear();
            txtNewPwdRef.Clear();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (txtOldPwd.Password.IsNullOrEmpty())
            {
                MessageBoxX.Show("请输入旧密码", "空值提醒");
                txtOldPwd.Focus();
                return;
            }
            if (txtNewPwd.Password.IsNullOrEmpty())
            {
                MessageBoxX.Show("请输入新密码", "空值提醒");
                txtNewPwd.Focus();
                return;
            }
            if (txtNewPwdRef.Password.IsNullOrEmpty())
            {
                MessageBoxX.Show("请输入重复密码", "空值提醒");
                txtNewPwdRef.Focus();
                return;
            }
            if (txtNewPwd.Password != txtNewPwdRef.Password)
            {
                MessageBoxX.Show("密码不一致", "错误");
                txtNewPwdRef.Focus();
                txtNewPwdRef.SelectAll();
                return;
            }

            if (UserGlobal.CurrUser.UserPwd != txtOldPwd.Password)
            {
                MessageBoxX.Show("旧密码错误", "错误");
                txtOldPwd.Focus();
                txtOldPwd.SelectAll();
                return;
            }

            UserManager.ModifyUser(new User
            {
                Id = UserGlobal.CurrUser.Id,
                UserPwd = txtNewPwd.Password,
                UpdateName = UserGlobal.CurrUser.UserName,
                UpdateNo = UserGlobal.CurrUser.UserNo
            }, false);
            UserGlobal.CurrUser.UserPwd = txtNewPwd.Password;

            MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 修改密码成功", "修改密码");
            UserGlobal.MainWindow.WriteInfoOnBottom("修改密码成功。");
            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}修改密码成功！", LogLevel.Operation);
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
