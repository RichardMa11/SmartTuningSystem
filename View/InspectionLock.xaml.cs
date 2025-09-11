using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BLL;
using Model;
using Panuon.UI.Silver;
using Panuon.UI.Silver.Core;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// InspectionLock.xaml 的交互逻辑
    /// </summary>
    public partial class InspectionLock : Page
    {
        public InspectionLock()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGridAsync();
            list.ItemsSource = Data;//绑定数据源

            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开送检锁定界面成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开送检锁定界面！", LogLevel.Operation);
        }

        private void MultiCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var currentCheckBox = (CheckBox)sender;
            var currentItem = (UIModel)currentCheckBox.DataContext;

            // 直接绑定CheckBox状态到IsSelected属性
            currentItem.IsSelected = currentCheckBox.IsChecked == true;

            //if (currentItem.IsSelected == false)
            //{
            //    foreach (var item in list.Items.OfType<UIModel>())
            //    {
            //        item.IsSelected = item == currentItem && currentCheckBox.IsChecked == true;
            //    }
            //}

            //list.Items.Refresh();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list.Items.OfType<UIModel>())
            {
                item.IsSelected = true;
            }
        }

        private void InvertSelection_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list.Items.OfType<UIModel>())
            {
                item.IsSelected = !item.IsSelected;
            }
        }

        #region 机台产品

        public readonly DeviceInfoManager DeviceInfoManager = new DeviceInfoManager();
        public readonly DeviceDetailManager DeviceDetailManager = new DeviceDetailManager();
        public readonly InspectionLockManager InspectionLockManager = new InspectionLockManager();
        ObservableCollection<UIModel> Data = new ObservableCollection<UIModel>();//页面数据集合
        //int dataCount = 0;//数据总条数
        int pagerCount = 0;//总页数
        int pageSize = 50;//页数据量
        int currPage = 1;//当前页码
        bool running = false;//是否正在执行查询

        #region UI Models

        public class UIModel : BaseUIModel
        {
            public bool isSelected = false;

            public bool IsSelected
            {
                get => isSelected;
                set
                {
                    isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
            public int Id { get; set; }

            private string deviceName = "";
            public string DeviceName
            {
                get => deviceName;
                set
                {
                    deviceName = value;
                    NotifyPropertyChanged("DeviceName");
                }
            }

            private string ipAddress = "";
            public string IpAddress
            {
                get => ipAddress;
                set
                {
                    ipAddress = value;
                    NotifyPropertyChanged("IpAddress");
                }
            }

            private string productName = "";
            public string ProductName
            {
                get => productName;
                set
                {
                    productName = value;
                    NotifyPropertyChanged("ProductName");
                }
            }
        }

        #endregion

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
            //刷新参数grid
            List<DeviceInfo> models = new List<DeviceInfo>();

            await Task.Run(() =>
            {
                var (data, total) = DeviceInfoManager.GetPagedDeviceInfo(searchText, currPage, pageSize);
                //
                //页码
                //
                pagerCount = PagerUtils.GetPagerCount(total, pageSize);
                if (currPage > pagerCount) currPage = pagerCount;
                //更新页码
                UiGlobal.RunUiAction(() =>
                {
                    gPager.CurrentIndex = currPage;
                    gPager.TotalIndex = pagerCount;
                });

                //生成分页数据
                models = data.OrderBy(c => c.DeviceName).ToList();
            });

            await Task.Delay(300);
            bNoData.Visibility = models.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
            foreach (var item in models)
            {
                UIModel _model = new UIModel()
                {
                    Id = item.Id,
                    DeviceName = item.DeviceName,
                    IpAddress = item.IpAddress,
                    ProductName = item.ProductName
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
                UpdateGridAsync();
        }

        //刷新
        private void btnRef_Click(object sender, RoutedEventArgs e)
        {
            UpdateGridAsync();
        }

        //页码更改事件
        private void gPager_CurrentIndexChanged(object sender, Panuon.UI.Silver.Core.CurrentIndexChangedEventArgs e)
        {
            currPage = gPager.CurrentIndex;
            UpdateGridAsync();
        }

        //锁定
        private async void btnLock_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxX.Show($"是否确认锁定送检数据？", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.No) return;

            var handler = PendingBox.Show("送检锁定中...", "请等待", false, Application.Current.MainWindow, new PendingBoxConfigurations()
            {
                LoadingForeground = "#5DBBEC".ToColor().ToBrush(),
                ButtonBrush = "#5DBBEC".ToColor().ToBrush(),
            });
            string lockNameTemp = DateTime.Now.ToString("yyyyMMddHHmmss");
            foreach (var item in list.Items.OfType<UIModel>())
            {
                if (!item.IsSelected) continue;
                foreach (var d in DeviceDetailManager.SelectDeviceDetailByDevId(item.Id))
                {
                    InspectionLockManager.AddLock(new Model.InspectionLock
                    {
                        LockName = lockNameTemp,
                        IpAddress = item.IpAddress,
                        PointAddress = d.PointAddress,
                        LockValue = CNCCommunicationHelps.GetCncValue(item.IpAddress, d.PointAddress),
                        CreateName = UserGlobal.CurrUser.UserName,
                        CreateNo = UserGlobal.CurrUser.UserNo
                    });
                }
            }

            handler.UpdateMessage("锁定成功。");
            await Task.Delay(1000);
            handler.Close();
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
