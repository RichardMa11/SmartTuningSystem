﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    /// RoleAuthorization.xaml 的交互逻辑
    /// </summary>
    public partial class RoleAuthorization : Page
    {
        public readonly RoleManager RoleManager = new RoleManager();
        public readonly MenuManager MenuManager = new MenuManager();
        ObservableCollection<RoleUIModel> RoleData = new ObservableCollection<RoleUIModel>();//角色页面数据集合
        List<ListBox> _listBoxes;//当前页面的所有ListBox
        public RoleAuthorization()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        #region Models

        public class RoleUIModel : BaseUIModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            private int pageCount = 0;
            public int PageCount
            {
                get => pageCount;
                set
                {
                    pageCount = value;
                    NotifyPropertyChanged("PageCount");
                    NotifyPropertyChanged("PageColor");
                }
            }
            public Brush PageColor
            {
                get
                {
                    return pageCount > 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                }
            }
        }

        #endregion

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开角色授权成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开角色授权！", LogLevel.Operation);
            lbRoles.ItemsSource = RoleData;//绑定角色数据源
            UpdateRoles();
            HideLoading();
            ShowEmpty();
        }

        #region Roles

        //更新角色列表
        private void UpdateRoles()
        {
            btnRefRoles.IsEnabled = true;
            RoleData.Clear();
            List<Role> roles = RoleManager.GetAllRole();//获取角色列表
            List<RoleMenu> roleMenus = MenuManager.GetAllRoleMenu();
            foreach (var r in roles)
            {
                RoleData.Add(new RoleUIModel
                {
                    Id = r.Id,
                    //Name = r.RoleName + "(" + r.RoleNo + ")",
                    Name = r.RoleName,
                    PageCount = roleMenus.Count(c => c.RoleId == r.Id)
                });
            }
            //btnRefRoles.IsEnabled = false;
        }

        //角色切换事件
        private async void lbRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RoleUIModel selectedModel = lbRoles.SelectedItem as RoleUIModel;//选中的项
            if (selectedModel == null)
            {
                ShowEmpty();
                return;
            }
            ShowLoading();
            LoadPageInfoByRoleIdAsync(selectedModel.Id);
            await Task.Delay(200);
            HideLoading();
            ShowEmpty(false);
        }

        public async void LoadPageInfoByRoleIdAsync(int roleId)
        {
            atPages.Items.Clear();
            List<int> currUserPages = new List<int>();//初始化当前页面数据
            _listBoxes = new List<ListBox>();

            await Task.Run(() =>
            {
                foreach (var r in MenuManager.GetAllRoleMenu().Where(c => c.RoleId == roleId).ToList())
                {
                    currUserPages.Add(r.MenuId);
                }
            });

            TabItem mTabItem = new TabItem { Header = "菜单列表" };
            Grid mGrid = new Grid();
            ListBox mListBox = new ListBox();
            //mListBox.Foreground = new SolidColorBrush(Colors.Blue);
            mGrid.Children.Add(mListBox);

            _listBoxes.Add(mListBox);
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

                //var listBoxItem = new ListBoxItem
                //{
                //    Content = pCheckBox,
                //    // 设置默认颜色
                //    Foreground = new SolidColorBrush(Colors.Blue)
                //};
            }

            if (atPages.Items.Count > 0) atPages.SelectedIndex = 0;
        }

        //刷新
        private void btnRef_Click(object sender, RoutedEventArgs e)
        {
            UpdateRoles();
        }
        #endregion

        #region Loading

        private void ShowLoading()
        {
            gLoading.Visibility = Visibility.Visible;
            atPages.IsEnabled = false;
        }

        private void HideLoading()
        {
            gLoading.Visibility = Visibility.Collapsed;
            atPages.IsEnabled = true;
        }

        #endregion

        #region Empty

        private void ShowEmpty(bool _show = true)
        {
            bNoData.Visibility = _show ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        //保存
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            RoleUIModel selectedModel = lbRoles.SelectedItem as RoleUIModel;//选中的角色
            if (selectedModel == null) return;
            List<int> result = GetResult().String2Int();//获取选中的页面Id集合
            List<RoleMenu> roleMenus = new List<RoleMenu>();

            //
            foreach (var re in result)
            {
                roleMenus.Add(new RoleMenu
                {
                    RoleId = selectedModel.Id,
                    MenuId = re,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo
                });
            }

            if (roleMenus.Count == 0) return;
            RoleManager.AddRoleMenu(roleMenus);
            //更新UI
            RoleData.Single(c => c.Id == selectedModel.Id).PageCount = result.Count;

            MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：角色授权成功", "角色授权");
            UserGlobal.MainWindow.WriteInfoOnBottom($"角色授权成功。");
            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：角色授权成功！", LogLevel.Operation);
        }

        /// <summary>
        /// 获得选中结果集
        /// </summary>
        public List<string> GetResult()
        {
            List<string> rights = new List<string>();
            foreach (var lb in _listBoxes)
            {
                foreach (var che in lb.Items)
                {
                    var checkBox = che as CheckBox;
                    if ((bool)checkBox.IsChecked)
                        rights.Add(checkBox.Tag.ToString());
                }
            }
            return rights;
        }
    }
}
