using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// PluginsMsg.xaml 的交互逻辑
    /// </summary>
    public partial class MenuView : Page
    {
        #region UI Models

        //public class UIModel : BaseUIModel
        //{
        //    public int Id { get; set; }
        //    public string Name { get; set; }
        //    public string DLLName { get; set; }
        //    public int ModuleCount { get; set; }
        //    public string ModuleNames { get; set; }
        //    public int Order { get; set; }
        //    public string UpdateTimeYear { get; set; }
        //    public string UpdateTime { get; set; }
        //    public string ImgSource { get; set; }
        //}

        #endregion 

        public MenuView()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

      

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //PluginsDeleteEventObserver.Instance.AddEventListener(Codes.PluginsDelete, OnPluginsDelete);
            ////绑定数据
            //lvPlugins.ItemsSource = PluginsData;
            //UpdateDataAsync();
        }

        //private void OnPluginsDelete(PluginsDeleteMessage p)
        //{
        //    using (CoreDBContext context = new CoreDBContext())
        //    {
        //        context.Plugins.Remove(context.Plugins.First(c => c.Id == p.Id));
        //        if (context.SaveChanges() > 0)//更新数据库
        //        {
        //            PluginsData.Remove(PluginsData.First(c => c.model.Id == p.Id));//更新UI 
        //        }
        //    }
        //}

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //PluginsDeleteEventObserver.Instance.RemoveEventListener(Codes.PluginsDelete, OnPluginsDelete);
        }

        //添加插件
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //using (CoreDBContext context = new CoreDBContext())
            //{
            //    var pluginInfo= context.Plugins.Add(new Plugins()
            //    {
            //        ConnectionName = "",
            //        ConnectionString = "",
            //        DependentIds = "",
            //        DLLName = "未设置",
            //        DLLs = "",
            //        IsEnable = false,
            //        LogoImage = "",
            //        Name = "新插件",
            //        Order = 0,
            //        UpdateTime = DateTime.Now,
            //        WebDownload = false
            //    });
            //    if (context.SaveChanges() > 0) 
            //    {
            //        var pluginsItem = new PluginsItem(pluginInfo) { Width = itemWidth, Height = itemHeight };
            //        PluginsData.Add(pluginsItem);
            //        pluginsItem.FlickerColor(Colors.Red);
            //    }
            //}
        }

        #region Pager

        /// <summary>
        /// 绑定数据
        /// </summary>
        private async void UpdateDataAsync()
        {
            //btnRef.IsEnabled = false;
            //PluginsData.Clear();
            //await Task.Delay(100);
            //List<Plugins> plugins = new List<Plugins>();
            //using (CoreDBContext context = new CoreDBContext())
            //{
            //    //按条件搜索
            //    string searchText = txtSearchText.Text.Trim();
            //    plugins = searchText.IsNullOrEmpty()
            //        ? context.Plugins.ToList()
            //        : context.Plugins.Where(c => c.Name.Contains(searchText) || c.DLLName.Contains(searchText)).ToList();

            //    bNoData.Visibility = plugins.Count > 0 ? Visibility.Collapsed : Visibility.Visible;//显示
            //    foreach (var pluginInfo in plugins)
            //    {
            //        var pluginsItem = new PluginsItem(pluginInfo) { Width = itemWidth, Height = itemHeight };
            //        PluginsData.Add(pluginsItem);
            //    }
            //}
            //btnRef.IsEnabled = true;
        }

        //刷新
        private void btnRef_Click(object sender, RoutedEventArgs e)
        {
            UpdateDataAsync();
        }

        #endregion

        #region Loading

        //private void ShowLoadingPanel()
        //{
        //    if (gLoading.Visibility != Visibility.Visible)
        //    {
        //        gLoading.Visibility = Visibility.Visible;
        //        lvPlugins.IsEnabled = false;
        //        bNoData.IsEnabled = false;

        //        OnLoadingShowComplate();
        //    }
        //}

        //private void HideLoadingPanel()
        //{
        //    if (gLoading.Visibility != Visibility.Collapsed)
        //    {
        //        gLoading.Visibility = Visibility.Collapsed;
        //        lvPlugins.IsEnabled = true;
        //        bNoData.IsEnabled = true;

        //        OnLoadingHideComplate();
        //    }
        //}

        private void OnLoadingHideComplate()
        {

        }

        private void OnLoadingShowComplate()
        {

        }

        #endregion

        //搜索
        private void txtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateDataAsync();
            }
        }


        //页码更改事件
        private void gPager_CurrentIndexChanged(object sender, Panuon.UI.Silver.Core.CurrentIndexChangedEventArgs e)
        {
            //currPage = gPager.CurrentIndex;
            //UpdateGridAsync();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //int id = (sender as Button).Tag.ToString().AsInt();
            //UIModel selectModel = Data.First(c => c.Id == id);

            //var result = MessageBoxX.Show($"是否确认删除账户[{selectModel.Name}]？", "删除提醒", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
            //if (result == MessageBoxResult.Yes)
            //{
            //    UserManager.RemoveUser(new User
            //    {
            //        Id = id,
            //        DelName = UserGlobal.CurrUser.UserName,
            //        DelNo = UserGlobal.CurrUser.UserNo
            //    });

            //    Data.Remove(selectModel);
            //}
        }

        //行加载事件 检查是否为超级管理员 
        //如果是超级管理员则不可修改
        private void list_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //UIModel userModel = e.Row.Item as UIModel;
            //if (userModel.Name == "admin")//只有系统最初的这个admin用户 信息不可修改
            //{
            //    e.Row.IsEnabled = false;
            //    e.Row.Background = new SolidColorBrush(Colors.LightBlue);//显示成灰色
            //}
        }
    }
}
