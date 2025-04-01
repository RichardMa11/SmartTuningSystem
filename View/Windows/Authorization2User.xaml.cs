using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BLL;
using Model;
using Model.View;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View.Windows
{
    /// <summary>
    /// Authorization2User.xaml 的交互逻辑
    /// </summary>
    public partial class Authorization2User : Window
    {
        public bool Succeed = false;
        int _userId = 0;
        List<int> currUserPages;//当前用户的页面权限 Id
        List<ListBox> listBoxes;//当前页面的所有ListBox
        public readonly UserManager UserManager = new UserManager();
        public readonly MenuManager MenuManager = new MenuManager();

        public Authorization2User(int userId)
        {
            InitializeComponent();
            this.UseCloseAnimation();

            _userId = userId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPagesInfoByUserIdAsync(_userId);
        }

        public async void LoadPagesInfoByUserIdAsync(int userId)
        {
            atPages.Items.Clear();
            currUserPages = new List<int>();//初始化当前页面数据
            listBoxes = new List<ListBox>();
            List<int> rolePages = new List<int>();

            await Task.Run(() =>
             {
                 #region 此处逻辑 与 登录加载权限 部分相同

                 var user = UserManager.GetUserById(userId);
                 var tempRoleMenus = LogManager.QueryBySql<VUserRoleMenu>(@"  select UserName,UserNo,PageName,PagePath,Icon,m.[Order],m.Id as MenuId FROM [SmartTuningSystemDB].[dbo].[Users] users with(nolock) 
  left join [SmartTuningSystemDB].[dbo].[UserRole] ur with(nolock)  on users.Id=ur.UserId and ur.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[RoleMenu] rm with(nolock)  on ur.RoleId=rm.RoleId and rm.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Menus] m with(nolock) on rm.MenuId=m.Id and m.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Roles] r with(nolock) on ur.RoleId=r.Id and r.IsValid=1
  where users.IsValid=1 ").Where(c => c.UserName == user.UserName && c.UserNo == user.UserNo).OrderBy(c => c.Order).ToList();
                 foreach (var r in tempRoleMenus)
                 {
                     if (r.MenuId != null)
                         rolePages.Add((int)r.MenuId);
                 }
                 UserMenu userMenus = MenuManager.GetAllUserMenu().FirstOrDefault(c => c.UserId == userId);//获取用户自定义权限

                 if (userMenus != null && userMenus.Id > 0)
                 {
                     if (userMenus.IncreaseMenus.NotEmpty())
                     {
                         //在角色权限基础上的增加页面
                         string[] increasePages = userMenus.IncreaseMenus.Split(',');
                         foreach (var iPage in increasePages)
                         {
                             int pageId = 0;
                             if (int.TryParse(iPage, out pageId))
                             {
                                 //string _iStr = rolePluginsStr.NotEmpty() ? $",{pageId}" : pageId.ToString();
                                 //rolePluginsStr += _iStr;//将字符串追加到末尾
                                 currUserPages.Add(pageId);
                             }
                             else { continue; }
                         }
                     }

                     if (userMenus.DecrementMenus.NotEmpty())
                     {
                         //在角色权限基础上的减少页面
                         string[] decrementPages = userMenus.DecrementMenus.Split(',');
                         //bool currRolesUpdate = false;//当前所有角色是否更新
                         foreach (var iPage in decrementPages)
                         {
                             int pageId = 0;
                             if (int.TryParse(iPage, out pageId))
                             {
                                 if (rolePages.Contains(pageId))
                                     rolePages.Remove(pageId); //如果有这一项 移除

                             }
                             else { continue; }
                         }

                         //if (currRolesUpdate)
                         //    rolePluginsStr = string.Join(",", _currRoles); //如果有更改，重新整理移除后的字符串
                     }
                 }
                 currUserPages.AddRange(rolePages);

                 //if (rolePluginsStr.NotEmpty())
                 //    currUserPages = rolePluginsStr.Split(',').ToList().String2Int();//将List<string>转为List<int>

                 #endregion
             });

            TabItem mTabItem = new TabItem { Header = "菜单列表" };
            Grid mGrid = new Grid();
            ListBox mListBox = new ListBox();
            mGrid.Children.Add(mListBox);

            listBoxes.Add(mListBox);
            mTabItem.Content = mGrid;
            atPages.Items.Add(mTabItem);

            foreach (var m in MenuManager.GetAllMenu().OrderBy(c => c.Order))
            {
                CheckBox pCheckBox = new CheckBox
                {
                    Content = m.PageName,
                    Margin = new Thickness(5),
                    Tag = m.Id,
                    IsChecked = currUserPages.Contains(m.Id)
                };
                mListBox.Items.Add(pCheckBox);
            }

            if (atPages.Items.Count > 0) atPages.SelectedIndex = 0;
        }

        private void edit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            List<string> result = GetResult();//获取选中的页面Id集合
            List<string> rolePages = new List<string>();

            #region 在角色权限中查找 如果角色中有但结果中没有 加入减量 如果角色中没有 但结果中有 加入增量

            var user = UserManager.GetUserById(_userId);
            //所有角色权限
            var tempRoleMenus = LogManager.QueryBySql<VUserRoleMenu>(@"  select UserName,UserNo,PageName,PagePath,Icon,m.[Order],m.Id as MenuId FROM [SmartTuningSystemDB].[dbo].[Users] users with(nolock) 
  left join [SmartTuningSystemDB].[dbo].[UserRole] ur with(nolock)  on users.Id=ur.UserId and ur.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[RoleMenu] rm with(nolock)  on ur.RoleId=rm.RoleId and rm.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Menus] m with(nolock) on rm.MenuId=m.Id and m.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Roles] r with(nolock) on ur.RoleId=r.Id and r.IsValid=1
  where users.IsValid=1 ").Where(c => c.UserName == user.UserName && c.UserNo == user.UserNo).OrderBy(c => c.Order).ToList();
            foreach (var r in tempRoleMenus)
            {
                rolePages.Add(r.MenuId.ToString());
            }

            List<string> _res1 = rolePages.Where(a => !result.Exists(t => t == a)).ToList();//查找角色中存在 但结果中不存在的数据 此为减少的数据
            List<string> _res2 = result.Where(a => !rolePages.Exists(t => t == a)).ToList();//查找结果中存在 但角色中不存在的数据 此为添加的数据

            #endregion

            string increasePages = string.Join(",", _res2);//增量
            string decrementPages = string.Join(",", _res1);//减量

            if (increasePages.NotEmpty() || decrementPages.NotEmpty())
            {
                if (MenuManager.GetAllUserMenu().Any(c => c.UserId == _userId))
                {
                    //之前存在数据 修改
                    MenuManager.ModifyUserMenu(new UserMenu
                    {
                        UserId = _userId,
                        IncreaseMenus = increasePages,
                        DecrementMenus = decrementPages,
                        UpdateName = UserGlobal.CurrUser.UserName,
                        UpdateNo = UserGlobal.CurrUser.UserNo
                    });
                }
                else
                {
                    //之前不存在数据 新增
                    MenuManager.AddUserMenu(new UserMenu
                    {
                        UserId = _userId,
                        IncreaseMenus = increasePages,
                        DecrementMenus = decrementPages,
                        CreateName = UserGlobal.CurrUser.UserName,
                        CreateNo = UserGlobal.CurrUser.UserNo
                    });
                }

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：用户授权成功", "授权账户");
                UserGlobal.MainWindow.WriteInfoOnBottom($"用户授权成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：用户授权成功！", LogLevel.Operation);
            }

            btnClose_Click(null, null);//模拟点击关闭
        }

        /// <summary>
        /// 获得选中结果集
        /// </summary>
        public List<string> GetResult()
        {
            List<string> rights = new List<string>();
            foreach (var lb in listBoxes)
            {
                foreach (var che in lb.Items)
                {
                    var _checkBox = che as CheckBox;
                    if ((bool)_checkBox.IsChecked)
                        rights.Add(_checkBox.Tag.ToString());
                }
            }
            return rights;
        }
    }
}
