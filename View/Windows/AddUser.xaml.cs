using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BLL;
using Model;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View.Windows
{
    /// <summary>
    /// AddUser.xaml 的交互逻辑
    /// </summary>
    public partial class AddUser : Window
    {
        public bool Succeed = false;
        public readonly RoleManager RoleManager = new RoleManager();
        public UserRoleDto2 Model = new UserRoleDto2();
        bool IsEdit
        {
            get { return editId > 0; }
        }
        int editId = 0;

        public AddUser(int _userId = 0)
        {
            InitializeComponent();
            this.UseCloseAnimation();

            editId = _userId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRolesComobox();
            if (IsEdit)
                InitUserInfo();
        }

        //加载角色下拉
        private void LoadRolesComobox()
        {
            List<Role> roles = RoleManager.GetAllRole();
            cbRoles.ItemsSource = roles;
            cbRoles.DisplayMemberPath = "RoleName";
            cbRoles.SelectedValuePath = "Id";
            cbRoles.SelectedIndex = 0;
        }

        /// <summary>
        /// 编辑时初始化用户信息
        /// </summary>
        private void InitUserInfo()
        {
            List<UserRoleDto2> users = LogManager.QueryBySql<UserRoleDto2>(@"   select UserName,UserNo,RoleNo,users.Id UserId,r.Id RoleId,RoleName,CanLogin,users.CreateTime,UserPwd
 FROM [SmartTuningSystemDB].[dbo].[Users] users with(nolock) 
  left join [SmartTuningSystemDB].[dbo].[UserRole] ur with(nolock)  on users.Id=ur.UserId and ur.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Roles] r with(nolock) on ur.RoleId=r.Id and r.IsValid=1
  where users.IsValid=1   ");

            UserRoleDto2 userModel = users.First(c => c.UserId == editId);
            cbRoles.SelectedValue = userModel.RoleId;
            txtAdminName.Text = userModel.UserName;
            txtAdminNo.Text = userModel.UserNo;
            txtAdminPwd.Password = userModel.UserPwd;
            txtReAdminPwd.Password = userModel.UserPwd;
        }

        #region UI Method

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Succeed = false;
            Close();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            int roleId = 0;// lblRole.Tag.ToString().AsInt();
            if (roleId <= 0)
            {
                MessageBoxX.Show("请选择角色", "空值提醒");
                return;
            }
            if (string.IsNullOrEmpty(txtAdminName.Text))
            {
                MessageBoxX.Show("账号不能为空", "空值提醒");
                txtAdminName.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtAdminPwd.Password))
            {
                MessageBoxX.Show("密码不能为空", "空值提醒");
                txtAdminName.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtReAdminPwd.Password))
            {
                MessageBoxX.Show("确认密码不能为空", "空值提醒");
                txtReAdminPwd.Focus();
                return;
            }
            if (txtAdminPwd.Password != txtReAdminPwd.Password)
            {
                MessageBoxX.Show("两次密码不一致", "数据异常");
                txtReAdminPwd.Focus();
                txtReAdminPwd.SelectAll();
                return;
            }


            //using (CoreDBContext context = new CoreDBContext())
            //{
            //    if (IsEdit)
            //    {
            //        if (context.User.Any(c => c.Name == txtAdminName.Text && c.Id != editId))
            //        {
            //            MessageBoxX.Show("账户名已存在", "数据异常");
            //            txtAdminName.Focus();
            //            txtAdminName.SelectAll();
            //            return;
            //        }
            //        //编辑
            //        Model = context.User.Single(c => c.Id == editId);
            //        Model.CreateTime = DateTime.Now;
            //        Model.Creator = UserGlobal.CurrUser.Id;
            //        Model.Name = txtAdminName.Text;
            //        Model.Pwd = txtAdminPwd.Password;
            //        Model.RoleId = roleId;
            //    }
            //    else
            //    {
            //        if (context.User.Any(c => c.Name == txtAdminName.Text))
            //        {
            //            MessageBoxX.Show("账户名已存在", "数据异常");
            //            txtAdminName.Focus();
            //            txtAdminName.SelectAll();
            //            return;
            //        }
            //        //添加
            //        Model.IsDel = false;
            //        Model.DelUser = 0;
            //        Model.DelTime = DateTime.Now;
            //        //Model.StaffId = "";
            //        Model.CanLogin = true;
            //        Model.CreateTime = DateTime.Now;
            //        Model.Creator = UserGlobal.CurrUser.Id;
            //        Model.Name = txtAdminName.Text;
            //        Model.Pwd = txtAdminPwd.Password;
            //        Model.RoleId = roleId;

            //        Model = context.User.Add(Model);

            //    }
            //    context.SaveChanges();
            //}

            Succeed = true;
            Close();
        }

        #endregion

        #region 拖动窗体

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #endregion 

        //编辑
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            #region 验证

            if (!txtAdminNo.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            if (!txtAdminName.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            string name = txtAdminName.Text;
            string userNo = txtAdminNo.Text;
            if (txtAdminPwd.Password != txtReAdminPwd.Password)
            {
                MessageBoxX.Show("两次密码输入不一致", "密码验证错误");
                return;
            }
            string password = txtAdminPwd.Password;
            int roleId = cbRoles.SelectedValue.ToString().AsInt();//角色Id

            #endregion

            List<UserRoleDto2> users = LogManager.QueryBySql<UserRoleDto2>(@"   select UserName,UserNo,RoleNo,users.Id UserId,r.Id RoleId,RoleName,CanLogin,users.CreateTime,UserPwd
 FROM [SmartTuningSystemDB].[dbo].[Users] users with(nolock) 
  left join [SmartTuningSystemDB].[dbo].[UserRole] ur with(nolock)  on users.Id=ur.UserId and ur.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Roles] r with(nolock) on ur.RoleId=r.Id and r.IsValid=1
  where users.IsValid=1   ");
            if (IsEdit)
            {
                #region 验证

                if (users.Any(c => c.UserName == name && c.UserId != editId))
                {
                    //存在
                    MessageBoxX.Show($"存在相同账户名[{name}]", "数据存在");
                    return;
                }

                #endregion

                #region  编辑状态

                //Model = users.Single(c => c.UserId == editId);
                //Model.Name = name;
                //Model.Pwd = password;
                //Model.RoleId = roleId;

                #endregion

                //this.Log("账户编辑成功！");
            }
            else
            {
                #region 验证

                if (users.Any(c => c.UserName == name && c.UserNo == userNo))
                {
                    //存在
                    MessageBoxX.Show($"存在相同账户名[{name}]和编码[{userNo}]", "数据存在");
                    return;
                }

                #endregion

                #region  添加状态

                RoleManager.AddUserRole(new UserRoleDto2
                {
                    UserName = name,
                    UserNo = userNo,
                    UserPwd = password,
                    RoleId = roleId
                }, UserGlobal.CurrUser);

                #endregion
                var tmp = LogManager.QueryBySql<UserRoleDto2>(@"   select UserName,UserNo,RoleNo,users.Id UserId,r.Id RoleId,RoleName,CanLogin,users.CreateTime,UserPwd
 FROM [SmartTuningSystemDB].[dbo].[Users] users with(nolock) 
  left join [SmartTuningSystemDB].[dbo].[UserRole] ur with(nolock)  on users.Id=ur.UserId and ur.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Roles] r with(nolock) on ur.RoleId=r.Id and r.IsValid=1
  where users.IsValid=1   ");
                Model = tmp.First(c => c.UserName == name && c.UserNo == userNo);

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：账户名[{name}]和编码[{userNo}]添加成功", "新增账户");
                UserGlobal.MainWindow.WriteInfoOnBottom($"账户名[{name}]和编码[{userNo}]添加成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：账户名[{name}]和编码[{userNo}]添加成功！", LogLevel.Operation);
            }

            //btnClose_Click(null, null);//模拟关闭
            Succeed = true;
            Close();
        }
    }
}
