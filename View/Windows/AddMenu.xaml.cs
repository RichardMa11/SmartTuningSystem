using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using BLL;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;
using Menu = Model.Menu;

namespace SmartTuningSystem.View.Windows
{
    /// <summary>
    /// AddUser.xaml 的交互逻辑
    /// </summary>
    public partial class AddMenu : Window
    {
        public bool Succeed = false;
        public readonly MenuManager MenuManager = new MenuManager();
        public Menu Model = new Menu();
        bool IsEdit
        {
            get { return editId > 0; }
        }
        int editId = 0;

        public AddMenu(int _userId = 0)
        {
            InitializeComponent();
            this.UseCloseAnimation();

            editId = _userId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsEdit)
                InitMenuInfo();
            else
                txtMenuOrder.Text = (MenuManager.GetAllMenu().Count + 1).ToString();
        }

        /// <summary>
        /// 编辑时初始化菜单信息
        /// </summary>
        private void InitMenuInfo()
        {
            GroupBoxMenu.Header = "菜单编辑";
            Menu menu = MenuManager.GetAllMenu().First(c => c.Id == editId);
            txtMenuName.Text = menu.PageName;
            txtMenuPath.Text = menu.PagePath;
            txtMenuOrder.Text = menu.Order.ToString();
            txtIcon.Text = FontAwesomeCommon.GetUnicode(menu.Icon);
            lblIcon.Content = menu.Icon;
        }

        #region UI Method

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Succeed = false;
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

            if (!txtMenuName.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            if (!txtMenuPath.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            if (!txtIcon.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            if (string.IsNullOrEmpty(lblIcon.Content.ToString()))
            {
                tab.SelectedIndex = 0;
                return;
            }

            if (!txtMenuOrder.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            string name = txtMenuName.Text;
            string path = txtMenuPath.Text;
            string icon = lblIcon.Content.ToString();
            int order = txtMenuOrder.Text.AsInt();

            #endregion

            List<Menu> menus = MenuManager.GetAllMenu();
            if (IsEdit)
            {
                #region 验证

                if (menus.Any(c => c.PagePath == path && c.PageName == name && c.Id != editId))
                {
                    //存在
                    MessageBoxX.Show($"存在相同菜单名[{name}]和菜单路径[{path}]", "数据存在");
                    return;
                }

                #endregion

                #region  编辑状态

                MenuManager.ModifyMenu(new Menu
                {
                    Id = editId,
                    PageName = name,
                    PagePath = path,
                    Icon = icon,
                    Order = order,
                    UpdateName = UserGlobal.CurrUser.UserName,
                    UpdateNo = UserGlobal.CurrUser.UserNo
                });

                #endregion

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：菜单名[{name}]和菜单路径[{path}]编辑成功", "编辑菜单");
                UserGlobal.MainWindow.WriteInfoOnBottom($"菜单名[{name}]和菜单路径[{path}]编辑成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：菜单名[{name}]和菜单路径[{path}]编辑成功！", LogLevel.Operation);
            }
            else
            {
                #region 验证

                if (menus.Any(c => c.PageName == name && c.PagePath == path))
                {
                    //存在
                    MessageBoxX.Show($"存在相同菜单名[{name}]和菜单路径[{path}]", "数据存在");
                    return;
                }

                #endregion

                #region  添加状态

                MenuManager.AddMenu(new Menu
                {
                    PageName = name,
                    PagePath = path,
                    Icon = icon,
                    Order = order,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo
                });

                #endregion

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：菜单名[{name}]和菜单路径[{path}]添加成功", "新增菜单");
                UserGlobal.MainWindow.WriteInfoOnBottom($"菜单名[{name}]和菜单路径[{path}]添加成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：菜单名[{name}]和菜单路径[{path}]添加成功！", LogLevel.Operation);
            }

            Model = MenuManager.GetAllMenu().First(c => c.PageName == name && c.PagePath == path);
            //btnClose_Click(null, null);//模拟关闭
            Succeed = true;
            Close();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            IconSelectorDialog iconWindow = new IconSelectorDialog();
            if (iconWindow.ShowDialog() == true)
            {
                var icon = iconWindow.SelectorModel.SelectedIcon;
                var iconText = iconWindow.SelectorModel.SelectedText;
                txtIcon.Text = FontAwesomeCommon.GetUnicode(iconText);
                lblIcon.Content = iconText;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0); // 直接判断是否为数字字符
            Regex regex = new Regex("^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space; // 拦截空格键
        }
    }
}
