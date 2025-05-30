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
    /// BasicDataView.xaml 的交互逻辑
    /// </summary>
    public partial class BasicDataView : Page
    {
        public int DeviceId = 0;
        public BasicDataView()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGridAsync();
            list.ItemsSource = Data;//绑定数据源
            listParam.ItemsSource = DataDetail;//绑定数据源

            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开基础数据维护界面成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开基础数据维护！", LogLevel.Operation);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;
            var hitResult = VisualTreeHelper.HitTest(element, e.GetPosition(element));
            var hit = hitResult != null ? hitResult.VisualHit : null;
            while (hit != null && !(hit is DataGridRow))
                hit = VisualTreeHelper.GetParent(hit);

            if (hit is DataGridRow row)
            {
                var id = ((dynamic)row.Item).Id;
                MessageBox.Show(string.Format("双击行ID: {0}", id));
            }
        }

        private void DataGridRow_PreviewMouseLeftButtonDown(
            object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            MessageBox.Show($"选中行ID: {((dynamic)row.Item).Id}");
        }

        private void SingleCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var currentCheckBox = (CheckBox)sender;
            var currentItem = (UIModel)currentCheckBox.DataContext;

            if (currentItem.IsSelected == false)
            {
                foreach (var item in list.Items.OfType<UIModel>())
                {
                    item.IsSelected = item == currentItem && currentCheckBox.IsChecked == true;
                }
            }

            list.Items.Refresh();
            DeviceId = currentItem.Id;
            UpdateGridDetailAsync(DeviceId);
        }

        #region 机台产品

        public readonly DeviceInfoManager DeviceInfoManager = new DeviceInfoManager();
        ObservableCollection<UIModel> Data = new ObservableCollection<UIModel>();//页面数据集合
        //int dataCount = 0;//数据总条数
        int pagerCount = 0;//总页数
        int pageSize = 20;//页数据量
        int currPage = 1;//当前页码
        bool running = false;//是否正在执行查询

        #region UI Models

        public class UIModel : BaseUIModel
        {
            public bool IsSelected { get; set; }
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
            DeviceId = 0;
            DataDetail.Clear();
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

        //删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int id = (sender as Button).Tag.ToString().AsInt();
            UIModel selectModel = Data.First(c => c.Id == id);

            var result = MessageBoxX.Show($"是否确认删除:机台[{selectModel.DeviceName}],产品品名[{selectModel.ProductName}]？",
                "删除提醒", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                DeviceInfoManager.RemoveDevice(new DeviceInfo
                {
                    Id = id,
                    DelName = UserGlobal.CurrUser.UserName,
                    DelNo = UserGlobal.CurrUser.UserNo
                });

                Data.Remove(selectModel);
            }
        }

        //添加
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.MaskVisible(true);
            AddDevice a = new AddDevice();
            a.ShowDialog();
            this.MaskVisible(false);
            if (a.Succeed)
            {
                Data.Insert(0, new UIModel
                {
                    Id = a.Model.Id,
                    DeviceName = a.Model.DeviceName,
                    IpAddress = a.Model.IpAddress,
                    ProductName = a.Model.ProductName
                });
            }
        }

        //编辑
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            int id = (sender as Button).Tag.ToString().AsInt();
            this.MaskVisible(true);
            AddDevice a = new AddDevice(id);
            a.ShowDialog();
            this.MaskVisible(false);
            if (a.Succeed)
            {
                UIModel selectModel = Data.First(c => c.Id == id);
                Data.Remove(selectModel);
                Data.Insert(0, new UIModel
                {
                    Id = a.Model.Id,
                    DeviceName = a.Model.DeviceName,
                    IpAddress = a.Model.IpAddress,
                    ProductName = a.Model.ProductName
                });
            }
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

        #region 机台参数

        public readonly DeviceDetailManager DeviceDetailManager = new DeviceDetailManager();
        ObservableCollection<UIModelDetail> DataDetail = new ObservableCollection<UIModelDetail>();//页面数据集合

        int pagerCountParam = 0;//总页数
        int pageSizeParam = 20;//页数据量
        int currPageParam = 1;//当前页码
        bool runningParam = false;//是否正在执行查询

        #region UI Models

        public class UIModelDetail : BaseUIModel
        {
            public int Id { get; set; }

            public int DeviceId { get; set; }

            private string pointName = "";
            public string PointName
            {
                get => pointName;
                set
                {
                    pointName = value;
                    NotifyPropertyChanged("PointName");
                }
            }

            private string pointPos = "";
            public string PointPos
            {
                get => pointPos;
                set
                {
                    pointPos = value;
                    NotifyPropertyChanged("PointPos");
                }
            }

            private string pointAddress = "";
            public string PointAddress
            {
                get => pointAddress;
                set
                {
                    pointAddress = value;
                    NotifyPropertyChanged("PointAddress");
                }
            }
        }

        #endregion

        /// <summary>
        /// 加载分页数据
        /// </summary>
        private async void UpdateGridDetailAsync(int id)
        {
            if (id == 0)
            {
                MessageBoxX.Show("没有选择机台产品！", "提示");
                return;
            }

            string searchText = txtSearchTextParam.Text;//按名称搜索

            ShowLoadingPanelParam();//显示Loading
            if (runningParam) return;
            runningParam = true;

            DataDetail.Clear();
            List<DeviceInfoDetail> models = new List<DeviceInfoDetail>();

            await Task.Run(() =>
            {
                var (data, total) = DeviceDetailManager.GetPagedDeviceDetail(searchText, currPageParam, pageSizeParam, id);
                //
                //页码
                //
                pagerCountParam = PagerUtils.GetPagerCount(total, pageSizeParam);
                if (currPageParam > pagerCountParam) currPageParam = pagerCountParam;
                //更新页码
                UiGlobal.RunUiAction(() =>
                {
                    gPagerParam.CurrentIndex = currPageParam;
                    gPagerParam.TotalIndex = pagerCountParam;
                });

                //生成分页数据
                models = data.OrderBy(c => c.PointName).ToList();
            });

            await Task.Delay(300);
            bNoDataParam.Visibility = models.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
            foreach (var item in models)
            {
                UIModelDetail _model = new UIModelDetail
                {
                    Id = item.Id,
                    PointName = item.PointName,
                    PointPos = item.PointPos,
                    PointAddress = item.PointAddress,
                    DeviceId = item.DeviceId
                };

                DataDetail.Add(_model);
            }

            HideLoadingPanelParam();
            runningParam = false;
        }

        #region Grid

        //搜索
        private void txtSearchTextParam_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UpdateGridDetailAsync(DeviceId);
        }

        //刷新
        private void btnRefParam_Click(object sender, RoutedEventArgs e)
        {
            UpdateGridDetailAsync(DeviceId);
        }

        //页码更改事件
        private void gPagerParam_CurrentIndexChanged(object sender, Panuon.UI.Silver.Core.CurrentIndexChangedEventArgs e)
        {
            currPageParam = gPagerParam.CurrentIndex;
            UpdateGridDetailAsync(DeviceId);
        }

        //删除
        private void btnDeleteParam_Click(object sender, RoutedEventArgs e)
        {
            int id = (sender as Button).Tag.ToString().AsInt();
            UIModelDetail selectModel = DataDetail.First(c => c.Id == id);

            var result = MessageBoxX.Show($"是否确认删除:点号（编号）[{selectModel.PointName}],夹序号（槽位）[{selectModel.PointPos}],参数地址[{selectModel.PointAddress}]？",
                "删除提醒", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                DeviceDetailManager.RemoveDeviceDetail(new DeviceInfoDetail
                {
                    Id = id,
                    DelName = UserGlobal.CurrUser.UserName,
                    DelNo = UserGlobal.CurrUser.UserNo
                });

                DataDetail.Remove(selectModel);
            }
        }

        //添加
        private void btnAddParam_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceId == 0)
            {
                MessageBoxX.Show("没有选择机台产品！", "提示");
                return;
            }

            this.MaskVisible(true);
            AddDeviceDetail a = new AddDeviceDetail(deviceId: DeviceId);
            a.ShowDialog();
            this.MaskVisible(false);
            if (a.Succeed)
            {
                DataDetail.Insert(0, new UIModelDetail
                {
                    Id = a.Model.Id,
                    PointName = a.Model.PointName,
                    PointPos = a.Model.PointPos,
                    PointAddress = a.Model.PointAddress,
                    DeviceId = a.Model.DeviceId
                });
            }
        }

        //编辑
        private void btnEditParam_Click(object sender, RoutedEventArgs e)
        {
            int id = (sender as Button).Tag.ToString().AsInt();
            this.MaskVisible(true);
            AddDeviceDetail a = new AddDeviceDetail(id, DeviceId);
            a.ShowDialog();
            this.MaskVisible(false);
            if (a.Succeed)
            {
                UIModelDetail selectModel = DataDetail.First(c => c.Id == id);
                DataDetail.Remove(selectModel);
                DataDetail.Insert(0, new UIModelDetail
                {
                    Id = a.Model.Id,
                    PointName = a.Model.PointName,
                    PointPos = a.Model.PointPos,
                    PointAddress = a.Model.PointAddress,
                    DeviceId = a.Model.DeviceId
                });
            }
        }

        #region Loading

        private void ShowLoadingPanelParam()
        {
            if (gLoadingParam.Visibility != Visibility.Visible)
            {
                gLoadingParam.Visibility = Visibility.Visible;
                listParam.IsEnabled = false;
                gPagerParam.IsEnabled = false;
                bNoDataParam.IsEnabled = false;
            }
        }

        private void HideLoadingPanelParam()
        {
            if (gLoadingParam.Visibility != Visibility.Collapsed)
            {
                gLoadingParam.Visibility = Visibility.Collapsed;
                listParam.IsEnabled = true;
                gPagerParam.IsEnabled = true;
                bNoDataParam.IsEnabled = true;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
