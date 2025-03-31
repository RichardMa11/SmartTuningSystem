using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BLL;
using Model;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using SmartTuningSystem.View.Windows;
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// Index.xaml 的交互逻辑
    /// </summary>
    public partial class UserView : Page
    {
        public readonly RoleManager RoleManager = new RoleManager();
        public readonly UserManager UserManager = new UserManager();
        public UserView()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        #region Models

        public class UIModel : BaseUIModel
        {
            public int Id { get; set; }

            private string name = "";
            public string Name
            {
                get => name;
                set
                {
                    name = value;
                    NotifyPropertyChanged("UserName");
                }
            }

            private string userNo = "";
            public string UserNo
            {
                get => userNo;
                set
                {
                    userNo = value;
                    NotifyPropertyChanged("UserNo");
                }
            }
            public int RoleId { get; set; }
            private string roleName = "";
            public string RoleName //角色名称
            {
                get => roleName;
                set
                {
                    roleName = value;
                    NotifyPropertyChanged("RoleName");
                }
            }


            private bool canLogin = false;
            public bool CanLogin
            {
                get => canLogin;
                set
                {
                    canLogin = value;
                    NotifyPropertyChanged("CanLogin");
                }
            }

            private int pageCount = 0;
            public int PageCount
            {
                get => pageCount;
                set
                {
                    pageCount = value;
                    NotifyPropertyChanged("PageCount");
                }
            }

            public int CreateYear { get; set; }
            public string CreateTime { get; set; }
        }

        #endregion

        //页面初始化
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateRoles();//加载角色
            list.ItemsSource = Data;//绑定数据源

            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开人员管理成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开人员管理！", LogLevel.Operation);
        }

        #region 左侧角色列表

        #region 角色属性

        bool _allChecking = false;//是否在进行全选或反选操作

        #endregion 

        //获取选中的角色
        private List<int> GetSelectedRoleIds()
        {
            List<int> ids = new List<int>();
            foreach (var item in wpRoles.Children)
            {
                CheckBox target = (item as CheckBox);
                if ((bool)target.IsChecked)
                    ids.Add(target.Tag.ToString().AsInt());
            }
            return ids;
        }

        //初始化角色
        private void UpdateRoles()
        {
            List<Role> roles = RoleManager.GetAllRole();
            wpRoles.Children.Clear();
            foreach (var r in roles)
            {
                //添加角色
                CheckBox checkBox = new CheckBox
                {
                    Height = 30,
                    Content = r.RoleName,
                    Background = new SolidColorBrush(Colors.Gray),
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(5),
                    Tag = r.Id
                };
                checkBox.Checked += RoleItem_Checked;
                checkBox.Unchecked += RoleItem_Unchecked;
                CheckBoxHelper.SetCheckBoxStyle(checkBox, CheckBoxStyle.Button);
                CheckBoxHelper.SetCheckedBackground(checkBox, new SolidColorBrush(Colors.Black));
                CheckBoxHelper.SetCornerRadius(checkBox, new CornerRadius(5));

                wpRoles.Children.Add(checkBox);
            }
        }

        //角色不选中
        private void RoleItem_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_allChecking) return;//全选或反选操作中不做任何动作
            //CheckBox currRoleCheckBox = sender as CheckBox;
            if (btnSelectedAllRoles.Content.ToString() == "\xf058")
            {
                btnSelectedAllRoles.Content = "\xf05d";
                btnSelectedAllRoles.Foreground = ColorHelper.ConvertToSolidColorBrush("#EAEAEA");
            }
            //暂时简便直接刷新
            UpdateGridAsync();
        }

        //角色选中
        private void RoleItem_Checked(object sender, RoutedEventArgs e)
        {
            if (_allChecking) return;//全选或反选操作中不做任何动作//CheckBox currRoleCheckBox = sender as CheckBox;//暂时简便直接刷新
            UpdateGridAsync();
        }

        //添加角色
        private void btnAddRole_Click(object sender, RoutedEventArgs e)
        {
            this.MaskVisible(true);
            EditRole editRole = new EditRole();
            if (editRole.ShowDialog() == true)
            {
                UpdateRoles();
            }
            this.MaskVisible(false);
        }

        //全选、反选
        private void btnSelectedAllRoles_Click(object sender, RoutedEventArgs e)
        {
            _allChecking = true;

            if (btnSelectedAllRoles.Content.ToString() == "\xf058")
            {
                #region 取消选择

                btnSelectedAllRoles.Content = "\xf05d";
                btnSelectedAllRoles.Foreground = ColorHelper.ConvertToSolidColorBrush("#EAEAEA");
                foreach (var item in wpRoles.Children)
                {
                    (item as CheckBox).IsChecked = false;
                }

                #endregion
            }
            else
            {
                #region 选择

                btnSelectedAllRoles.Content = "\xf058";
                btnSelectedAllRoles.Foreground = new SolidColorBrush(Colors.Black);
                foreach (var item in wpRoles.Children)
                {
                    (item as CheckBox).IsChecked = true;
                }

                #endregion
            }

            _allChecking = false;
            UpdateGridAsync();
        }

        private void cbCanLogin_Click(object sender, RoutedEventArgs e)
        {
            int userId = (sender as CheckBox).Tag.ToString().AsInt();//获取用户Id
            var selectedUser = Data.First(c => c.Id == userId);//选中的User
            var result = MessageBoxX.Show($"是否确认更改[{selectedUser.Name}]登录权限？", "权限更新提醒", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                UpdateCanLogin(userId, !selectedUser.CanLogin);
            else (sender as CheckBox).IsChecked = selectedUser.CanLogin;
        }

        //更新用户是否可登录
        private void UpdateCanLogin(int userId, bool canLogin)
        {
            UserManager.ModifyUser(new User
            {
                Id = userId,
                CanLogin = canLogin,
                UpdateName = UserGlobal.CurrUser.UserName,
                UpdateNo = UserGlobal.CurrUser.UserNo
            }, true);
            //更新UI
            Data.Single(c => c.Id == userId).CanLogin = canLogin;

            UserGlobal.MainWindow.WriteInfoOnBottom("登录权限更新成功！");
            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}登录权限更新成功！", LogLevel.Operation);
        }

        #endregion

        #region 右侧用户列表

        #region 分页属性

        ObservableCollection<UIModel> Data = new ObservableCollection<UIModel>();//页面数据集合
        int dataCount = 0;//数据总条数
        int pagerCount = 0;//总页数
        int pageSize = 20;//页数据量
        int currPage = 1;//当前页码
        bool running = false;//是否正在执行查询

        #endregion

        /// <summary>
        /// 加载分页数据
        /// </summary>
        private async void UpdateGridAsync()
        {
            var selectedRoleIds = GetSelectedRoleIds();//选择的角色
            string searchText = txtSearchText.Text;//按名称搜索

            ShowLoadingPanel();//显示Loading
            if (running) return;
            running = true;

            Data.Clear();
            List<UserRoleDto2> models = new List<UserRoleDto2>();

            await Task.Run(() =>
            {
                List<UserRoleDto2> users = LogManager.QueryBySql<UserRoleDto2>(@"   select UserName,UserNo,RoleNo,users.Id UserId,r.Id RoleId,RoleName,CanLogin,users.CreateTime,UserPwd
 FROM [SmartTuningSystemDB].[dbo].[Users] users with(nolock) 
  left join [SmartTuningSystemDB].[dbo].[UserRole] ur with(nolock)  on users.Id=ur.UserId and ur.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Roles] r with(nolock) on ur.RoleId=r.Id and r.IsValid=1
  where users.IsValid=1   ");
                if (selectedRoleIds.Count > 0)
                    users = users.Where(c => selectedRoleIds.Contains(c.RoleId)).ToList();

                if (searchText.NotEmpty())
                    users = users.Where(c => c.UserName.Contains(searchText) || c.UserNo.Contains(searchText)).ToList();

                dataCount = users.Count();
                //
                //页码
                //
                pagerCount = PagerUtils.GetPagerCount(dataCount, pageSize);
                if (currPage > pagerCount) currPage = pagerCount;
                //更新页码
                UiGlobal.RunUiAction(() =>
                {
                    gPager.CurrentIndex = currPage;
                    gPager.TotalIndex = pagerCount;
                });

                //生成分页数据
                models = users.OrderByDescending(c => c.CreateTime).Skip(pageSize * (currPage - 1)).Take(pageSize).ToList();
            });

            await Task.Delay(300);
            bNoData.Visibility = models.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
            foreach (var item in models)
            {
                UIModel _model = new UIModel()
                {
                    CreateYear = item.CreateTime.Year,
                    CreateTime = item.CreateTime.ToString("MM-dd HH:mm"),
                    Id = item.UserId,
                    Name = item.UserName,
                    RoleId = item.RoleId,
                    RoleName = item.RoleName,
                    CanLogin = item.CanLogin,
                    UserNo = item.UserNo
                };

                Data.Add(_model);
            }

            HideLoadingPanel();
            running = false;
        }

        #region Grid

        //搜索
        private void txtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateGridAsync();
            }
        }

        //行加载事件 检查是否为超级管理员 
        //如果是超级管理员则不可修改
        private void list_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            UIModel userModel = e.Row.Item as UIModel;
            if (userModel.Name == "admin")//只有系统最初的这个admin用户 信息不可修改
            {
                e.Row.IsEnabled = false;
                e.Row.Background = new SolidColorBrush(Colors.LightBlue);//显示成灰色
            }
        }

        //重置密码为123456
        private void btnRePwd_Click(object sender, RoutedEventArgs e)
        {
            int id = (sender as Button).Tag.ToString().AsInt();
            var selectModel = Data.First(c => c.Id == id);
            var result = MessageBoxX.Show($"是否确认重置[{selectModel.Name}]密码？", "密码初始化提醒", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                UserManager.ModifyUser(new User
                {
                    Id = id,
                    UserPwd = "123456",
                    UpdateName = UserGlobal.CurrUser.UserName,
                    UpdateNo = UserGlobal.CurrUser.UserNo
                }, false);

                if (id == UserGlobal.CurrUser.Id)
                    UserGlobal.CurrUser.UserPwd = "123456";

                UserGlobal.MainWindow.WriteInfoOnBottom("初始密码123456设置成功！");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}初始密码123456设置成功", LogLevel.Operation);
                MessageBoxX.Show("初始密码\"123456\"", "密码初始化成功");
            }


        }

        //账号授权
        private void btnAuthorization_Click(object sender, RoutedEventArgs e)
        {
            int userId = (sender as Button).Tag.ToString().AsInt();
            this.MaskVisible(true);

            Authorization2User a = new Authorization2User(userId);
            a.ShowDialog();

            this.MaskVisible(false);

            if (a.Succeed)
            {
                MessageBoxX.Show("请被授权用户重新登录", "授权成功");
                UserGlobal.MainWindow.WriteInfoOnBottom($"授权成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：授权成功！", LogLevel.Authorization);
            }
        }

        //删除账号
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int id = (sender as Button).Tag.ToString().AsInt();
            UIModel selectModel = Data.First(c => c.Id == id);

            var result = MessageBoxX.Show($"是否确认删除账户[{selectModel.Name}]？", "删除提醒", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                UserManager.RemoveUser(new User
                {
                    Id = id,
                    DelName = UserGlobal.CurrUser.UserName,
                    DelNo = UserGlobal.CurrUser.UserNo
                });

                Data.Remove(selectModel);
            }
        }

        //添加账号
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.MaskVisible(true);
            AddUser a = new AddUser();
            a.ShowDialog();
            this.MaskVisible(false);
            if (a.Succeed)
            {
                Data.Insert(0, new UIModel()
                {
                    CreateYear = a.Model.CreateTime.Year,
                    CreateTime = a.Model.CreateTime.ToString("MM-dd HH:mm"),
                    Id = a.Model.UserId,
                    Name = a.Model.UserName,
                    RoleId = a.Model.RoleId,
                    RoleName = a.Model.RoleName,
                    CanLogin = a.Model.CanLogin,
                    UserNo = a.Model.UserNo
                });
            }
        }

        //页码更改事件
        private void gPager_CurrentIndexChanged(object sender, Panuon.UI.Silver.Core.CurrentIndexChangedEventArgs e)
        {
            currPage = gPager.CurrentIndex;
            UpdateGridAsync();
        }

        //刷新
        private void btnRef_Click(object sender, RoutedEventArgs e)
        {
            UpdateGridAsync();
        }

        #region Loading

        private void ShowLoadingPanel()
        {
            if (gLoading.Visibility != Visibility.Visible)
            {
                gLoading.Visibility = Visibility.Visible;
                list.IsEnabled = false;
                gPager.IsEnabled = false;
                bNoData.IsEnabled = false;
            }
        }

        private void HideLoadingPanel()
        {
            if (gLoading.Visibility != Visibility.Collapsed)
            {
                gLoading.Visibility = Visibility.Collapsed;
                list.IsEnabled = true;
                gPager.IsEnabled = true;
                bNoData.IsEnabled = true;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
