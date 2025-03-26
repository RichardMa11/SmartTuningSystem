using System.Windows;
using System.Windows.Input;
using BLL;
using Model;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View.Windows
{
    /// <summary>
    /// AddRole.xaml 的交互逻辑
    /// </summary>
    public partial class EditRole : Window
    {
        public readonly RoleManager RoleManager = new RoleManager();
        int editId = 0;
        bool IsEdit
        {
            get { return editId != 0; }
        }
        public EditRole(int _editId = 0)
        {
            InitializeComponent();
            editId = _editId;

            if (IsEdit) InitRole();
        }

        private void InitRole()
        {
            txtRoleName.Text = RoleManager.GetRoleById(editId).RoleName;
            txtRoleNo.Text = RoleManager.GetRoleById(editId).RoleNo;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!txtRoleName.NotEmpty() || !txtRoleNo.NotEmpty()) return;
            string roleName = txtRoleName.Text;
            string roleNo = txtRoleNo.Text;
            if (IsEdit)
            {
                //编辑模式
                RoleManager.ModifyRole(new Role
                {
                    Id = editId,
                    RoleName = roleName,
                    UpdateName = UserGlobal.CurrUser.UserName,
                    UpdateNo = UserGlobal.CurrUser.UserNo
                });

                UserGlobal.MainWindow.WriteInfoOnBottom("编辑角色成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}编辑角色成功！", LogLevel.Operation);
            }
            else
            {
                //添加模式
                RoleManager.AddRole(new Role
                {
                    RoleName = roleName,
                    RoleNo = roleNo,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo
                });

                UserGlobal.MainWindow.WriteInfoOnBottom("添加角色成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}添加角色成功！", LogLevel.Operation);
            }
            DialogResult = true;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
