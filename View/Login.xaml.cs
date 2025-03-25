using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BLL;
using Model;
using Model.View;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public readonly UserManager UserManager = new UserManager();
        public readonly RoleManager RoleManager = new RoleManager();

        public Login()
        {
            InitializeComponent();

            this.Loaded += Window_Loaded;
            this.Unloaded += Window_Unloaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitLoginData();//所有数据库操作在此操作后执行
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            #region 解除事件监听

            Loaded -= Window_Loaded;
            Unloaded -= Window_Unloaded;

            #endregion
        }

        private async void InitLoginData()
        {
            IsEnabled = false;
            var handler = PendingBox.Show("连接数据库...", "请等待", false, Application.Current.MainWindow, new PendingBoxConfigurations()
            {
                LoadingForeground = "#5DBBEC".ToColor().ToBrush(),
                ButtonBrush = "#5DBBEC".ToColor().ToBrush(),
            });
            bool connectionSucceed = false;
            //检查数据
            await Task.Run(() =>
            {
                connectionSucceed = NullDataCheck();
            });
            await Task.Delay(200);
            if (connectionSucceed)
            {
                handler.UpdateMessage("连接成功,请登录...");
            }
            await Task.Delay(200);
            handler.Close();

            IsEnabled = true;
            if (!connectionSucceed)
            {
                MessageBoxX.Show("数据库连接失败", "连接错误");
                await Task.Delay(1000);
                Application.Current.Shutdown();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();//自定义窗口拖拽事件
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string userName = TxtUserName.Text.Trim();
                string userPwd = TxtPassword.Password.Trim();

                #region 验证

                if (string.IsNullOrEmpty(userName))
                {
                    MessageBoxX.Show("账户不能为空", "空值判断");
                    TxtUserName.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(userPwd))
                {
                    MessageBoxX.Show("密码不能为空", "空值判断");
                    TxtPassword.Focus();
                    return;
                }

                #endregion

                var handler = PendingBox.Show("正在登录...", "请等待", false, Application.Current.MainWindow, new PendingBoxConfigurations()
                {
                    LoadingForeground = "#5DBBEC".ToColor().ToBrush(),
                    ButtonBrush = "#5DBBEC".ToColor().ToBrush(),
                });

                #region 登录

                var users = LogManager.QueryBySql<User>(@"select * from Users with(nolock)");
                if (users.Any(c => c.UserName == userName))
                {
                    if (users.Any(c => c.UserName == userName && c.UserPwd == userPwd))
                    {
                        //登录成功
                        var userModel = users.First(c => c.UserName == userName && c.UserPwd == userPwd && c.CanLogin);
                        if (userModel.Id <= 0)
                        {
                            Panuon.UI.Silver.Notice.Show($"未知错误！", "登录失败", 5, Panuon.UI.Silver.MessageBoxIcon.Error);
                            handler.Close();
                            return;
                        }
                        else
                        {
                            //继续装载数据
                            handler.UpdateMessage("登录成功,正在装载用户数据...");
                            UserGlobal.CurrUser = userModel;

                            //LogHelps.Info($"{userName}登录成功！");
                            LogHelps.WriteLogToDb($"{userName}登录成功！", LogLevel.Authorization);

                            #region 加载权限，主页
                            UserGlobal.VUserRoleMenus = LogManager.QueryBySql<VUserRoleMenu>(@"  select UserName,UserNo,PageName,PagePath,RoleNo FROM [SmartTuningSystemDB].[dbo].[Users] users with(nolock) 
  left join [SmartTuningSystemDB].[dbo].[UserRole] ur with(nolock)  on users.Id=ur.UserId and ur.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[RoleMenu] rm with(nolock)  on ur.RoleId=rm.RoleId and rm.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Menus] m with(nolock) on rm.MenuId=m.Id and m.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Roles] r with(nolock) on ur.RoleId=r.Id and r.IsValid=1
  where users.IsValid=1  ");

                            #endregion

                            await Task.Delay(500);
                            handler.UpdateMessage("数据装载成功！");
                            await Task.Delay(300);
                            handler.Close();
                            Panuon.UI.Silver.Notice.Show($"{UserGlobal.CurrUser.UserName}登录", "欢迎", 3, MessageBoxIcon.Success);

                            //主页
                            this.Close();
                            new MainWindow().ShowDialog();
                        }
                    }
                    else
                    {
                        Panuon.UI.Silver.Notice.Show($"密码错误！", "登录失败", 5, Panuon.UI.Silver.MessageBoxIcon.Error);
                        handler.Close();
                        return;
                    }
                }
                else
                {
                    Panuon.UI.Silver.Notice.Show($"账户不存在！", "登录失败", 5, Panuon.UI.Silver.MessageBoxIcon.Error);
                    handler.Close();
                    return;
                }
                #endregion
            }
            catch (Exception ex)
            {
                UiGlobal.RunUiAction(() =>
                {
                    MessageBoxX.Show(ex.Message, "系统错误");
                });

                LogHelps.Error($@"系统错误：{ex.Message + ex.StackTrace}");
            }
        }

        /// <summary>
        /// 填充管理员数据
        /// </summary>
        public bool NullDataCheck()
        {
            try
            {
                var roles = LogManager.QueryBySql<Role>(@"select top 1 * from Roles where RoleNo='00001'");
                //var roles1 = RoleManager.GetRoleBySql(@"select top 1 * from Roles where RoleNo='00001'");
                var users = LogManager.QueryBySql<User>(@"select top 1 * from Users");

                int roleId = roles.Count == 0
                    ? RoleManager.AddRole(new Role
                    {
                        RoleName = "超级管理员",
                        RoleNo = "00001",
                        CreateName = "系统",
                        CreateNo = "0"
                    }) : roles.First().Id;

                if (users.Count == 0)
                {
                    var userId = UserManager.AddUser(new User
                    {
                        UserName = "admin",
                        UserNo = "00001",
                        UserPwd = "123456",
                        //RoleId = roleId,
                        CanLogin = true,
                        CreateName = "系统",
                        CreateNo = "0"
                    });

                    RoleManager.AddUserRole(new UserRole
                    {
                        RoleId = roleId,
                        UserId = userId,
                        CreateName = "系统",
                        CreateNo = "0"
                    });
                }

                if (roles.Count != 0 && users.Count == 1)
                {
                    LogHelps.Info($@"数据库初始化成功。");
                    //LogManager.AddLog(new Log
                    //{
                    //    LogStr = $@"数据库初始化成功。",
                    //    LogType = LogType.System,
                    //    CreateName = "系统",
                    //    CreateNo = "0"
                    //});
                }

                return true;
            }
            catch (Exception ex)
            {
                UiGlobal.RunUiAction(() =>
                {
                    MessageBoxX.Show(ex.Message, "数据连接错误");
                });

                //Common.LogManager.Debug($"5555登录成功！");
                LogHelps.Error($@"数据库初始化失败：{ex.Message + ex.StackTrace}");

                return false;
            }
        }
    }
}
