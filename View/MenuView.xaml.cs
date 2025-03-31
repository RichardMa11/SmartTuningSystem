using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BLL;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using SmartTuningSystem.View.Windows;
using static Model.Log;
using Menu = Model.Menu;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// PluginsMsg.xaml 的交互逻辑
    /// </summary>
    public partial class MenuView : Page
    {
        public readonly MenuManager MenuManager = new MenuManager();
        ObservableCollection<UIModel> Data = new ObservableCollection<UIModel>();//页面数据集合
        int dataCount = 0;//数据总条数
        int pagerCount = 0;//总页数
        int pageSize = 20;//页数据量
        int currPage = 1;//当前页码
        bool running = false;//是否正在执行查询

        #region UI Models

        public class UIModel : BaseUIModel
        {
            public int Id { get; set; }

            private string pageName = "";
            public string PageName
            {
                get => pageName;
                set
                {
                    pageName = value;
                    NotifyPropertyChanged("PageName");
                }
            }

            private string pagePath = "";
            public string PagePath
            {
                get => pagePath;
                set
                {
                    pagePath = value;
                    NotifyPropertyChanged("PagePath");
                }
            }

            private int order = 0;
            public int Order
            {
                get => order;
                set
                {
                    order = value;
                    NotifyPropertyChanged("Order");
                }
            }

            private string icon = "";
            public string Icon
            {
                get => icon;
                set
                {
                    icon = value;
                    NotifyPropertyChanged("Icon");
                }
            }

            private string createNo = "";
            public string CreateNo
            {
                get => createNo;
                set
                {
                    createNo = value;
                    NotifyPropertyChanged("CreateNo");
                }
            }

            private string createName = "";
            public string CreateName
            {
                get => createName;
                set
                {
                    createName = value;
                    NotifyPropertyChanged("CreateName");
                }
            }

            public int CreateYear { get; set; }
            public string CreateTime { get; set; }
        }

        #endregion 

        public MenuView()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGridAsync();
            list.ItemsSource = Data;//绑定数据源

            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开菜单管理成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开菜单管理！", LogLevel.Operation);
        }

        /// <summary>
        /// 加载分页数据
        /// </summary>
        private async void UpdateGridAsync()
        {
            string searchText = txtSearchText.Text;//按名称搜索

            ShowLoadingPanel();//显示Loading
            if (running) return;
            running = true;

            Data.Clear();
            List<Model.Menu> models = new List<Model.Menu>();

            await Task.Run(() =>
            {
                List<Model.Menu> menus = MenuManager.GetAllMenu();
                if (searchText.NotEmpty())
                    menus = menus.Where(c => c.PageName.Contains(searchText) || c.PagePath.Contains(searchText)).ToList();

                dataCount = menus.Count();
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
                models = menus.OrderBy(c => c.Order).ThenByDescending(c => c.CreateTime).Skip(pageSize * (currPage - 1)).Take(pageSize).ToList();
            });

            await Task.Delay(300);
            bNoData.Visibility = models.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
            foreach (var item in models)
            {
                UIModel _model = new UIModel()
                {
                    CreateYear = item.CreateTime.Year,
                    CreateTime = item.CreateTime.ToString("MM-dd HH:mm"),
                    Id = item.Id,
                    PageName = item.PageName,
                    PagePath = item.PagePath,
                    Order = item.Order,
                    CreateName = item.CreateName,
                    CreateNo = item.CreateNo,
                    Icon = FontAwesomeCommon.GetUnicode(item.Icon)
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
        //如果不是超级管理员则不可修改
        private void list_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (UserGlobal.CurrUser.UserNo != "00001")//菜单信息不可修改，只有开发者可以
            {
                e.Row.IsEnabled = false;
                e.Row.Background = new SolidColorBrush(Colors.LightBlue);//显示成灰色
            }
        }

        //删除账号
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int id = (sender as Button).Tag.ToString().AsInt();
            UIModel selectModel = Data.First(c => c.Id == id);

            var result = MessageBoxX.Show($"是否确认删除菜单[{selectModel.PageName}]？", "删除提醒", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                MenuManager.RemoveMenu(new Menu
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
            if (UserGlobal.CurrUser.UserNo == "00001") //菜单信息不可修改，只有开发者可以
            {
                this.MaskVisible(true);
                AddMenu a = new AddMenu();
                a.ShowDialog();
                this.MaskVisible(false);
                if (a.Succeed)
                {
                    Data.Insert(0, new UIModel()
                    {
                        CreateYear = a.Model.CreateTime.Year,
                        CreateTime = a.Model.CreateTime.ToString("MM-dd HH:mm"),
                        Id = a.Model.Id,
                        PageName = a.Model.PageName,
                        PagePath = a.Model.PagePath,
                        Order = a.Model.Order,
                        CreateName = a.Model.CreateName,
                        CreateNo = a.Model.CreateNo,
                        Icon = FontAwesomeCommon.GetUnicode(a.Model.Icon)
                    });
                }
            }
            else
            {
                MessageBoxX.Show($"只有程序员可以新增菜单......", "权限管理");
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            int id = (sender as Button).Tag.ToString().AsInt();
            this.MaskVisible(true);
            AddMenu a = new AddMenu(id);
            a.ShowDialog();
            this.MaskVisible(false);
            if (a.Succeed)
            {
                UIModel selectModel = Data.First(c => c.Id == id);
                Data.Remove(selectModel);
                Data.Insert(0, new UIModel()
                {
                    CreateYear = a.Model.CreateTime.Year,
                    CreateTime = a.Model.CreateTime.ToString("MM-dd HH:mm"),
                    Id = a.Model.Id,
                    PageName = a.Model.PageName,
                    PagePath = a.Model.PagePath,
                    Order = a.Model.Order,
                    CreateName = a.Model.CreateName,
                    CreateNo = a.Model.CreateNo,
                    Icon = FontAwesomeCommon.GetUnicode(a.Model.Icon)
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
    }
}
