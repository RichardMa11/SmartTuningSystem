using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BLL;
using Microsoft.Win32;
using Model;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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

        private void btnImportIp_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel文件|*.xls;*.xlsx|所有文件|*.*",
                Title = "选择Excel文件",
                CheckFileExists = true,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() != true) return;

            if (string.IsNullOrEmpty(openFileDialog.FileName) || !File.Exists(openFileDialog.FileName))
            {
                MessageBoxX.Show("请先选择有效的Excel文件", "提示");
                return;
            }

            try
            {
                List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
                //解析Excel
                using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;
                        if (string.IsNullOrEmpty(row.GetCell(0)?.ToString())) break;

                        var temp = new DeviceInfo
                        {
                            DeviceName = row.GetCell(0).ToString(),
                            IpAddress = row.GetCell(1).ToString(),
                            ProductName = row.GetCell(2).ToString()
                        };
                        deviceInfos.Add(temp);
                    }
                }

                //insert 
                InsertIp(deviceInfos);
            }
            catch (Exception ex)
            {
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}导入机台IP文件报错；报错原因：{ex.Message + ex.StackTrace}", LogLevel.Error);
                MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName}导入机台IP文件报错；报错原因：{ex.Message + ex.StackTrace}", "提示");
            }
            finally
            {
                UpdateGridAsync();
            }
        }

        private void InsertIp(List<DeviceInfo> deviceInfoTemp)
        {
            foreach (var d in deviceInfoTemp)
            {
                #region 验证1

                if (!d.DeviceName.NotEmpty() || !d.IpAddress.NotEmpty() || !d.ProductName.NotEmpty())
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入IP：机台编号[{d.DeviceName}]、ip[{d.IpAddress}]以及产品品名[{d.ProductName}]有为空！",
                        LogLevel.Error);
                    continue;
                }

                Regex ipRegex = new Regex(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}$");
                if (!ipRegex.IsMatch(d.IpAddress))
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入IP：IP地址格式错误，ip[{d.IpAddress}]", LogLevel.Error);
                    continue;
                }

                var deviceModel = LogManager.QueryBySql<DeviceModel>(
                        @"select distinct DeviceName,IpAddress from DeviceInfo with(nolock) where IsValid=1 ").ToList();
                #endregion

                #region 验证2

                //验证机台、IP唯一
                if (deviceModel.Any(c => c.DeviceName == d.DeviceName && c.IpAddress != d.IpAddress))
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入IP：存在机台编号[{d.DeviceName}]并且IP不为[{d.IpAddress}]的数据，机台和ip必须唯一"
                        , LogLevel.Error);
                    continue;
                }

                if (deviceModel.Any(c => c.DeviceName != d.DeviceName && c.IpAddress == d.IpAddress))
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入IP：存在IP为[{d.IpAddress}]并且机台编号不为[{d.DeviceName}]的数据，机台和ip必须唯一"
                        , LogLevel.Error);
                    continue;
                }

                //机台、IP、产品品名唯一
                if (DeviceInfoManager.GetDeviceByParam(device: d.DeviceName, ip: d.IpAddress, product: d.ProductName).Count != 0)
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入IP：存在相同机台编号[{d.DeviceName}]、ip[{d.IpAddress}]以及产品品名[{d.ProductName}]"
                        , LogLevel.Error);
                    continue;
                }

                #endregion

                #region  添加状态

                DeviceInfoManager.AddDevice(new DeviceInfo
                {
                    DeviceName = d.DeviceName,
                    IpAddress = d.IpAddress,
                    ProductName = d.ProductName,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo
                });

                #endregion

                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：机台编号[{d.DeviceName}]、ip[{d.IpAddress}]以及产品品名[{d.ProductName}]添加成功！"
                    , LogLevel.Operation);
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

            private int deviceId = 0;
            public int DeviceId
            {
                get => deviceId;
                set
                {
                    deviceId = value;
                    NotifyPropertyChanged("DeviceId");
                }
            }

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
                models = data.OrderBy(c => c.PointName).ThenBy(c => c.PointPos).ToList();
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

        private void btnImportParam_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceId == 0)
            {
                MessageBoxX.Show("没有选择机台产品！", "提示");
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel文件|*.xls;*.xlsx|所有文件|*.*",
                Title = "选择Excel文件",
                CheckFileExists = true,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() != true) return;

            if (string.IsNullOrEmpty(openFileDialog.FileName) || !File.Exists(openFileDialog.FileName))
            {
                MessageBoxX.Show("请先选择有效的Excel文件", "提示");
                return;
            }

            try
            {
                List<DeviceInfoDetail> deviceDetails = new List<DeviceInfoDetail>();
                //解析Excel
                using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    IWorkbook workbook = new XSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;
                        if (string.IsNullOrEmpty(row.GetCell(0)?.ToString())) break;

                        var temp = new DeviceInfoDetail
                        {
                            PointName = row.GetCell(0).ToString(),
                            PointPos = row.GetCell(1).ToString(),
                            PointAddress = row.GetCell(2).ToString()
                        };
                        deviceDetails.Add(temp);
                    }
                }

                //insert 
                InsertParam(deviceDetails);
            }
            catch (Exception ex)
            {
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}导入机台参数地址文件报错；报错原因：{ex.Message + ex.StackTrace}", LogLevel.Error);
                MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName}导入机台参数地址文件报错；报错原因：{ex.Message + ex.StackTrace}", "提示");
            }
            finally
            {
                UpdateGridDetailAsync(DeviceId);
            }
        }

        private void InsertParam(List<DeviceInfoDetail> deviceDetailTemp)
        {
            foreach (var d in deviceDetailTemp)
            {
                #region 验证1

                if (!d.PointName.NotEmpty() || !d.PointPos.NotEmpty() || !d.PointAddress.NotEmpty())
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入参数地址：点号（编号）[{d.PointName}]、夹序号（槽位）[{d.PointPos}]和参数地址为[{d.PointAddress}]有为空！",
                        LogLevel.Error);
                    continue;
                }

                #endregion

                var deviceModel = LogManager.QueryBySql<DeviceInfoDetail>
                    ($@"select * from DeviceInfoDetail with(nolock) where IsValid=1 and DeviceId={DeviceId} ").ToList();

                #region 验证2

                //验证参数地址唯一
                if (deviceModel.Any(c => c.PointAddress == d.PointAddress))
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入参数地址：存在参数地址为[{d.PointAddress}]的数据，参数地址必须唯一",
                        LogLevel.Error);
                    continue;
                }

                //验证点号（编号）、夹序号、参数地址俩俩唯一
                if (deviceModel.Any(c => c.PointName == d.PointName && c.PointPos == d.PointPos))
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入参数地址：存在点号（编号）[{d.PointName}]和夹序号（槽位）为[{d.PointPos}]的数据，点号（编号）和夹序号（槽位）必须唯一",
                        LogLevel.Error);
                    continue;
                }

                if (deviceModel.Any(c => c.PointName == d.PointName && c.PointAddress == d.PointAddress))
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入参数地址：存在点号（编号）[{d.PointName}]和参数地址为[{d.PointAddress}]的数据，点号（编号）和参数地址必须唯一",
                        LogLevel.Error);
                    continue;
                }

                if (deviceModel.Any(c => c.PointPos == d.PointPos && c.PointAddress == d.PointAddress))
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入参数地址：存在夹序号（槽位）[{d.PointPos}]和参数地址为[{d.PointAddress}]的数据，夹序号（槽位）和参数地址必须唯一",
                        LogLevel.Error);
                    continue;
                }

                //点号（编号）、夹序号、参数地址唯一
                if (deviceModel.Any(c => c.PointName == d.PointName && c.PointPos == d.PointPos && c.PointAddress == d.PointAddress))
                {
                    LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作批量导入参数地址：存在点号（编号）[{d.PointName}]、夹序号（槽位）[{d.PointPos}]和参数地址为[{d.PointAddress}]的数据，" +
                                          $"点号（编号）、夹序号（槽位）和参数地址必须唯一", LogLevel.Error);
                    continue;
                }

                #endregion

                #region  添加状态

                DeviceDetailManager.AddDeviceDetail(new DeviceInfoDetail
                {
                    PointName = d.PointName,
                    PointPos = d.PointPos,
                    PointAddress = d.PointAddress,
                    DeviceId = DeviceId,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo
                });

                #endregion

                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：点号（编号）[{d.PointName}]、夹序号（槽位）[{d.PointPos}]和参数地址为[{d.PointAddress}]添加成功！"
                    , LogLevel.Operation);
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
