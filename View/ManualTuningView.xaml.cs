using System;
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
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// ManualTuningView.xaml 的交互逻辑
    /// </summary>
    public partial class ManualTuningView : Page
    {
        public int DeviceId = 0;
        public string Ip = "127.0.0.1";
        public string ProductName = "";
        public string DeviceName = "";
        public readonly SysConfigManager SysConfigManager = new SysConfigManager();
        public double AllowedRange { get; set; } = 0.05;

        public List<UserPermissionDto> UserPermissions = new List<UserPermissionDto>();

        public ManualTuningView()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGridAsync();
            list.ItemsSource = Data;//绑定数据源
            listParam.ItemsSource = DataDetail;//绑定数据源
            if (!string.IsNullOrEmpty(SysConfigManager.GetSysConfigByKey("AllowedRange").FirstOrDefault()?.Value))
                AllowedRange = Convert.ToDouble(SysConfigManager.GetSysConfigByKey("AllowedRange").FirstOrDefault()?.Value);

            UserPermissions = LogManager.QueryBySql<UserPermissionDto>(@"   select PermissionCode,UserName,UserNo FROM [SmartTuningSystemDB].[dbo].[Users] users with(nolock) 
  left join [SmartTuningSystemDB].[dbo].[UserRole] ur with(nolock)  on users.Id=ur.UserId and ur.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[RolePermission] rp with(nolock)  on ur.RoleId=rp.RoleId and rp.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Permission] p with(nolock) on rp.PermissionId=p.Id and p.IsValid=1
  left join [SmartTuningSystemDB].[dbo].[Roles] r with(nolock) on ur.RoleId=r.Id and r.IsValid=1
  where users.IsValid=1   ").Where(c => c.UserName == UserGlobal.CurrUser.UserName && c.UserNo == UserGlobal.CurrUser.UserNo).OrderBy(c => c.PermissionCode).ToList();

            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开手动调机-IPQC界面成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开手动调机-IPQC！", LogLevel.Operation);
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
            Ip = currentItem.IpAddress;
            ProductName = currentItem.ProductName;
            DeviceName = currentItem.DeviceName;
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
            Ip = "127.0.0.1";
            ProductName = "";
            DeviceName = "";
            GlobalData.Instance.IsDataValid = false;
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

            private decimal? paramCurrValue;
            public decimal? ParamCurrValue
            {
                get => paramCurrValue;
                set
                {
                    paramCurrValue = value;
                    NotifyPropertyChanged("ParamCurrValue");
                }
            }

            private decimal? paramModifyValue;
            public decimal? ParamModifyValue
            {
                get => paramModifyValue;
                set
                {
                    paramModifyValue = value;
                    NotifyPropertyChanged("ParamModifyValue");
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
                models = data.OrderBy(c => c.Id).ThenBy(c => c.PointName).ThenBy(c => c.PointPos).ToList();
            });

            await Task.Delay(300);
            bNoDataParam.Visibility = models.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;

            //var tempConnect = CNCCommunicationHelps.ConnectCnc(new List<string> { Ip });//开启连接
            foreach (var item in models)
            {
                UIModelDetail _model = new UIModelDetail
                {
                    Id = item.Id,
                    PointName = item.PointName,
                    PointPos = item.PointPos,
                    PointAddress = item.PointAddress,
                    DeviceId = item.DeviceId,
                    ParamCurrValue = CNCCommunicationHelps.GetCncValue(Ip, item.PointAddress)
                    //ParamCurrValue = CNCCommunicationHelps.GetCncValue(tempConnect, Ip, item.PointAddress)
                };

                DataDetail.Add(_model);
            }
            //CNCCommunicationHelps.DisConnectCnc(tempConnect);//断开连接

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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBoxX.Show($"是否确认要下发数据给机台？", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.No) return;

                string befParam = "", sendParam = "";
                //var tempConnect = CNCCommunicationHelps.ConnectCnc(new List<string> { Ip });//开启连接
                foreach (var p in DataDetail)
                {
                    if (p.ParamModifyValue == null) continue;
                    if (befParam == "")
                        befParam += $@"FAI编号：[{p.PointName}],地址：[{p.PointAddress}],值：[{p.ParamCurrValue}]|";
                    else
                        befParam += $@"{Environment.NewLine}FAI编号：[{p.PointName}],地址：[{p.PointAddress}],值：[{p.ParamCurrValue}]|";

                    if (sendParam == "")
                        sendParam += $@"【手动调机】FAI编号：[{p.PointName}],地址：[{p.PointAddress}],值：[{p.ParamModifyValue}]|";
                    else
                        sendParam += $@"{Environment.NewLine}FAI编号：[{p.PointName}],地址：[{p.PointAddress}],值：[{p.ParamModifyValue}]|";

                    CNCCommunicationHelps.SetCncValue(Ip, p.PointAddress, Convert.ToDecimal(p.ParamModifyValue));
                    //CNCCommunicationHelps.SetCncValue(tempConnect, Ip, p.PointAddress, p.ParamModifyValue);
                }
                //CNCCommunicationHelps.DisConnectCnc(tempConnect);//断开连接

                LogHelps.WriteTuningRecord(DeviceName, ProductName, sendParam.TrimEnd('|'), befParam.TrimEnd('|'));
                UpdateGridDetailAsync(DeviceId);
                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：设置CNC机台数据：机台：[{DeviceName}],机台IP：[{Ip}],产品品名：[{ ProductName}]成功!", "IPQC调机");
            }
            catch (Exception ex)
            {
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台：[{DeviceName}],机台IP：[{Ip}],
产品品名：[{ProductName}],报错原因：{ex.Message + ex.StackTrace}", LogLevel.Error);
                MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：机台：[{DeviceName}],机台IP：[{Ip}],
产品品名：[{ProductName}],报错原因：{ex.Message + ex.StackTrace}", "提示");
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit || e.Column.Header.ToString() != "参数修改值") return;
            var textBox = e.EditingElement as TextBox;
            var model = (UIModelDetail)e.Row.Item;
            if (model.ParamCurrValue == null) return;

            GlobalData.Instance.IsDataValid = true;
            if (double.TryParse(textBox.Text, out double parsedValue))
            {
                var tempRecords = LogManager.QueryBySql<Model.TuningRecord>($@"   SELECT * 
                FROM [SmartTuningSystemDB].[dbo].[TuningRecord] r 
                WHERE 
                  r.DeviceName = '{DeviceName}'  -- 参数化设备名
                  AND r.ProductName = '{ProductName}'  -- 参数化产品名
                  AND r.IsValid=1
                  -- 筛选当天24小时：大于等于当天0点，小于次日0点（覆盖所有毫秒）
                  AND r.CreateTime >= CAST(GETDATE() AS DATE)  
                  AND r.CreateTime < DATEADD(DAY, 1, CAST(GETDATE() AS DATE));   ").Where(t => t.SendStr.Contains(model.PointAddress)).ToList();
                //先判断次数
                if (tempRecords.Count > 0 && UserPermissions.All(u => u.PermissionCode != "A001"))
                {
                    MessageBoxX.Show($"FAI编号：[{model.PointName}],地址：[{model.PointAddress}]今天已经调整过，" +
                                     $"手动调机一天只能调整一次,若是确实需要调整，请联系组长或者经理等更高权限的人员！", "验证错误");
                    e.Cancel = true; // 阻止编辑完成
                    GlobalData.Instance.IsDataValid = false;
                    UpdateGridDetailAsync(DeviceId);
                }
                else //再范围
                {
                    if (Math.Abs(parsedValue - Convert.ToDouble(model.ParamCurrValue)) > AllowedRange && UserPermissions.All(u => u.PermissionCode != "A002"))
                    {
                        MessageBoxX.Show($"调整范围不能超过±{AllowedRange},若是确实需要更改大数值，请联系组长或者经理等更高权限的人员！", "验证错误");
                        e.Cancel = true; // 阻止编辑完成
                        GlobalData.Instance.IsDataValid = false;
                    }
                }
            }
            else
            {
                MessageBoxX.Show("请输入有效数值（可含正负号和小数点）", "格式错误");
                e.Cancel = true;
                GlobalData.Instance.IsDataValid = false;
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
