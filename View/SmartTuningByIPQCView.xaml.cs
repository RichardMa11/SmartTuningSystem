using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BLL;
using Model;
using NPOI.SS.UserModel;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// SmartTuningByIPQCView.xaml 的交互逻辑
    /// </summary>
    public partial class SmartTuningByIPQCView : Page
    {
        ObservableCollection<UIModel> Data = new ObservableCollection<UIModel>();//页面数据集合
        bool _running = false;//是否正在执行查询
        private List<DeviceInfoModel> _deviceInfoModels = new List<DeviceInfoModel>();
        public readonly SysConfigManager SysConfigManager = new SysConfigManager();
        public readonly InspectionLockManager InspectionLockManager = new InspectionLockManager();
        public readonly DeviceInfoManager DeviceInfoManager = new DeviceInfoManager();
        private static ApiClient _apiClient;
        public string MeasureApi = "/IPQC_API/QuerySizeMeasurementData";
        //public ObservableCollection<string> Items { get; set; }
        public double AllowedRange { get; set; } = 0.15;

        #region UI Models

        public class UIModel : BaseUIModel
        {
            //机台
            public string DeviceName { get; set; }
            //点号（编号）
            public string PointName { get; set; }
            //夹序号（槽位）
            public string PointPos { get; set; }
            //参数地址
            public string PointAddress { get; set; }

            //送检锁定值
            public decimal? LockValue { get; set; }
            //标准值
            public double NominalDim { get; set; }
            //+Tol
            public double TolMax { get; set; }
            //-Tol
            public double TolMin { get; set; }
            //上公差最大值
            public double USL { get; set; }
            //下公差最大值
            public double LSL { get; set; }
            //测量值
            public double MeasureValue { get; set; }
            //偏差值
            public double Deviation { get; set; }
            //调机公差
            public double Tolerance { get; set; }
            //情况说明
            public string StatusDescription { get; set; }
            //推荐补偿值说明
            public string CompensationDescription { get; set; }
            //推荐补偿值

            private double? recommendedCompensation;
            public double? RecommendedCompensation
            {
                get => recommendedCompensation;
                set
                {
                    recommendedCompensation = value;
                    NotifyPropertyChanged("RecommendedCompensation");
                }
            }

            public double? ReferenceValue { get; set; }

            //变量当前值
            public string ParamCurrValue { get; set; }
            public Brush RowColor { get; set; }
            public void CalculateFields()
            {
                USL = NominalDim + TolMax;
                LSL = NominalDim - TolMin;
                Tolerance = Math.Round((USL - LSL) * 0.5 * 0.6, 4);

                if (MeasureValue == 0)
                {
                    RowColor = Brushes.Gainsboro;
                    return;
                }

                Deviation = Math.Round(MeasureValue - NominalDim, 4);
                var qualified = "";
                if (Deviation > 0)
                {
                    if (Math.Abs(Deviation) > TolMax)
                        qualified = "NG";
                }
                else
                {
                    if (Math.Abs(Deviation) > TolMin)
                        qualified = "NG";
                }

                if (Math.Abs(Deviation) < Tolerance || Deviation == 0)
                {
                    StatusDescription = "情况1:偏差值绝对值<调机公差";
                    CompensationDescription = "不推荐补偿值";
                    //RecommendedCompensation = 0;
                    RowColor = Brushes.LightGreen;
                }
                else if (Math.Abs(Deviation) >= Tolerance && Deviation > 0 && qualified != "NG")
                {
                    StatusDescription = "情况2:偏差值绝对值>=调机公差&&合格偏上";
                    CompensationDescription = "按照偏差值的50%推荐补偿值";
                    if (ParamCurrValue == "没有维护参数地址值，请先去基础数据维护！！！")
                        RecommendedCompensation = Math.Round(-Deviation * 0.5, 4);
                    else
                    {
                        if (LockValue == null)
                        {
                            if (!string.IsNullOrEmpty(ParamCurrValue))
                                RecommendedCompensation = Math.Round(-Deviation * 0.5, 4) + Convert.ToDouble(ParamCurrValue);
                            else
                                RecommendedCompensation = Math.Round(-Deviation * 0.5, 4);
                        }
                        else
                            RecommendedCompensation = Math.Round(-Deviation * 0.5, 4) + Convert.ToDouble(LockValue);
                    }

                    RowColor = Brushes.Gold;
                }
                else if (Math.Abs(Deviation) >= Tolerance && Deviation < 0 && qualified != "NG")
                {
                    StatusDescription = "情况3:偏差值绝对值>=调机公差&&合格偏下";
                    CompensationDescription = "按照偏差值的50%推荐补偿值";
                    //RecommendedCompensation = (-Deviation * 0.5).ToString("F4");
                    if (ParamCurrValue == "没有维护参数地址值，请先去基础数据维护！！！")
                        RecommendedCompensation = Math.Round(-Deviation * 0.5, 4);
                    else
                    {
                        if (LockValue == null)
                        {
                            if (!string.IsNullOrEmpty(ParamCurrValue))
                                RecommendedCompensation = Math.Round(-Deviation * 0.5, 4) + Convert.ToDouble(ParamCurrValue);
                            else
                                RecommendedCompensation = Math.Round(-Deviation * 0.5, 4);
                        }
                        else
                            RecommendedCompensation = Math.Round(-Deviation * 0.5, 4) + Convert.ToDouble(LockValue);
                    }

                    RowColor = Brushes.Gold;
                }
                else if (Math.Abs(Deviation) < NominalDim * 0.5 && qualified == "NG")
                {
                    StatusDescription = "情况4:超差NG&偏差值<标准值*0.5";
                    CompensationDescription = "按照偏差值的80%推荐补偿值";
                    if (ParamCurrValue == "没有维护参数地址值，请先去基础数据维护！！！")
                        RecommendedCompensation = Math.Round(-Deviation * 0.8, 4);
                    else
                    {
                        if (LockValue == null)
                        {
                            if (!string.IsNullOrEmpty(ParamCurrValue))
                                RecommendedCompensation = Math.Round(-Deviation * 0.8, 4) + Convert.ToDouble(ParamCurrValue);
                            else
                                RecommendedCompensation = Math.Round(-Deviation * 0.8, 4);
                        }
                        else
                            RecommendedCompensation = Math.Round(-Deviation * 0.8, 4) + Convert.ToDouble(LockValue);
                    }

                    RowColor = Brushes.Coral;
                }
                else if (Math.Abs(Deviation) >= NominalDim * 0.5 && qualified == "NG")
                {
                    StatusDescription = "情况5:超差NG&偏差值>=标准值*0.5";
                    CompensationDescription = "不推荐，报警";
                    //RecommendedCompensation = 0;
                    RowColor = Brushes.Red;
                }

                ReferenceValue = RecommendedCompensation;
            }
        }

        #endregion 

        public SmartTuningByIPQCView()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalData.Instance.IsDataValid = true;
            list.ItemsSource = Data;//绑定数据源
            comboLock.ItemsSource = InspectionLockManager.GetAllLock().OrderByDescending(t => t.Id).Select(t => t.LockName).ToList().Distinct();
            //foreach (var t in InspectionLockManager.GetAllLock().Select(t => t.LockName).ToList().Distinct())
            //{
            //    basicCombo.ItemsSource.Add(t);
            //}
            if (!string.IsNullOrEmpty(SysConfigManager.GetSysConfigByKey("AllowedRange").FirstOrDefault()?.Value))
                AllowedRange = Convert.ToDouble(SysConfigManager.GetSysConfigByKey("AllowedRange").FirstOrDefault()?.Value);

            string ipqcHttp = "http://awase1ipqc81:8080";
            if (!string.IsNullOrEmpty(SysConfigManager.GetSysConfigByKey("IPQC_HTTP").FirstOrDefault()?.Value))
                ipqcHttp = SysConfigManager.GetSysConfigByKey("IPQC_HTTP").FirstOrDefault()?.Value;

            if (!string.IsNullOrEmpty(SysConfigManager.GetSysConfigByKey("MEASURE_API").FirstOrDefault()?.Value))
                MeasureApi = SysConfigManager.GetSysConfigByKey("MEASURE_API").FirstOrDefault()?.Value;

            _apiClient = new ApiClient(ipqcHttp)
            {
                LogRequestResponse = msg =>
                {
                    LogHelps.Info($"[API] {DateTime.Now:HH:mm:ss} {msg}");
                }
            };

            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开智能调机成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开智能调机界面！", LogLevel.Operation);
        }

        private async void BtnGenerateTuningRpt_Click(object sender, RoutedEventArgs e)
        {
            if (comboLock.SelectedValue == null)
            {
                MessageBoxX.Show("请先选择送检锁定时间", "提示");
                return;
            }
            string lockName = comboLock.SelectedValue.ToString();
            List<Model.InspectionLock> lockNameTemp;
            List<UIModel> dataList = new List<UIModel>();
            //连接句柄
            Dictionary<string, ushort> tempConnect = new Dictionary<string, ushort>();

            try
            {
                ShowLoadingPanel(); //显示Loading
                if (_running) return;
                _running = true;

                try
                {
                    DateTime.TryParseExact(lockName.Substring(0, 8), "yyyyMMdd", null,
                        System.Globalization.DateTimeStyles.None, out DateTime date);
                    lockNameTemp = InspectionLockManager.GetLockByLockName(lockName).ToList();
                    List<DeviceInfo> deviceInfos = DeviceInfoManager.GetAllDevice();
                    List<string> ipList = lockNameTemp.Select(t => t.IpAddress).Distinct().ToList();
                    // 一步完成：查询→去重→过滤空值→拼接
                    string devName = string.Join(",",
                        deviceInfos
                            .Where(t => ipList.Contains(t.IpAddress))
                            .Select(t => t.DeviceName)
                            .Distinct()
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                    );

                    var requestData = new Request
                    {
                        DeviceName = devName,
                        Date = date.ToString("yyyy/MM/dd")
                    };

                    MockDataService mockDataService = new MockDataService();
                    Response rst = await mockDataService.GetMockResponseDataAsync();
                    //Response rst = await _apiClient.PostAsync<Response>(MeasureApi, requestData);

                    if (rst.returnResult.Count == 0)
                    {
                        MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName} IPQC没有对应的数据", "提示");
                        return;
                    }

                    //返回的device
                    List<string> rstDevices = rst.returnResult.Select(t => t.deviceName).Distinct().ToList();
                    string rstIp = string.Join(",",
                        deviceInfos.Where(t => rstDevices.Contains(t.DeviceName))
                            .Select(t => t.IpAddress).Distinct()
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                    );

                    if (string.IsNullOrEmpty(rstIp))
                    {
                        MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName} IPQC返回的数据在系统中没有对应的IP地址", "提示");
                        return;
                    }

                    _deviceInfoModels = LogManager.QueryBySql<DeviceInfoModel>(
                        $@"   select [DeviceName],[IpAddress],[ProductName],[PointName],[PointPos],[PointAddress] FROM [SmartTuningSystemDB].[dbo].[DeviceInfo] dev with(nolock) 
left join [SmartTuningSystemDB].[dbo].[DeviceInfoDetail] det with(nolock) on dev.Id=det.DeviceId and det.IsValid=1 and det.IsUsedSmart=1
where dev.IsValid=1 and dev.IpAddress in ('{rstIp}')  ").ToList();

                    tempConnect = CNCCommunicationHelps.ConnectCnc(_deviceInfoModels.Where(t => !string.IsNullOrWhiteSpace(t.PointName))
                        .Select(t => t.IpAddress).Distinct().ToList());//开启连接
                    foreach (var device in rst.returnResult)
                    {
                        foreach (var d in device.data)
                        {
                            foreach (var m in d.measureData)
                            {
                                var data = new UIModel
                                {
                                    DeviceName = device.deviceName,
                                    PointName = d.fai,
                                    PointPos = m.posNo,
                                    NominalDim = Convert.ToDouble(d.nominal),
                                    TolMax = Convert.ToDouble(d.max),
                                    TolMin = Convert.ToDouble(d.min),
                                    MeasureValue = Convert.ToDouble(m.measureValue)
                                };

                                var temp = _deviceInfoModels.FirstOrDefault(x => x.DeviceName == data.DeviceName &&
                                    x.PointName == data.PointName && x.PointPos == data.PointPos);
                                //data.ParamCurrValue = temp == null
                                //    ? "没有维护参数地址值，请先去基础数据维护！！！"
                                //    : CNCCommunicationHelps.GetCncValue(temp.IpAddress, temp.PointAddress).ToString();
                                data.ParamCurrValue = temp == null
                                    ? "没有维护参数地址值，请先去基础数据维护！！！"
                                    : CNCCommunicationHelps.GetCncValue(tempConnect, temp.IpAddress, temp.PointAddress).ToString();

                                data.PointAddress = temp?.PointAddress;
                                var lockTemp = lockNameTemp.FirstOrDefault(l =>
                                    l.IpAddress == temp?.IpAddress && l.PointAddress == temp?.PointAddress);
                                data.LockValue = lockTemp?.LockValue;

                                data.CalculateFields();
                                dataList.Add(data);
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    LogHelps.Error($@"网络请求错误: {ex.Message}");
                    MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName} 生成调机报告报错；报错原因：网络请求错误: {ex.Message}", "提示");
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    LogHelps.Error($@"数据处理错误: {ex.Message}");
                    MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName} 生成调机报告报错；报错原因：数据处理错误: {ex.Message}", "提示");
                    return;
                }

                Data.Clear();
                await Task.Delay(300);
                bNoData.Visibility = dataList.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
                if (!string.IsNullOrEmpty(txtDeviceName.Text))
                    dataList = dataList.Where(t => t.DeviceName == txtDeviceName.Text).ToList();

                if (!string.IsNullOrEmpty(txtPointName.Text))
                    dataList = dataList.Where(t => t.PointName == txtPointName.Text).ToList();

                dataList = dataList.Where(t => t.ParamCurrValue != "没有维护参数地址值，请先去基础数据维护！！！").ToList();

                foreach (var d in dataList)
                {
                    Data.Add(d);
                }

                ApplyRowStyles();
                //txtProductName.Text = _productName ?? "";
            }
            catch (Exception ex)
            {
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName} 生成调机报告报错；报错原因：{ex.Message + ex.StackTrace}",
                    LogLevel.Error);
                MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName} 生成调机报告报错；报错原因：{ex.Message + ex.StackTrace}", "提示");
            }
            finally
            {
                CNCCommunicationHelps.DisConnectCnc(tempConnect);//断开连接
                HideLoadingPanel();
                _running = false;
                GlobalData.Instance.IsDataValid = true;
            }
        }

        //刷新
        private void btnRef_Click(object sender, RoutedEventArgs e)
        {
            BtnGenerateTuningRpt_Click(null, null);
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (Data.Count == 0)
            {
                MessageBoxX.Show("请先生成调机报告", "提示");
                return;
            }

            if (MessageBoxX.Show($"是否确认要下发数据给机台？", "提示", System.Windows.Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.No) return;

            try
            {
                string befParam = "", sendParam = "";
                List<string> deviceNames = new List<string>();
                if (Data.Any(x => x.RecommendedCompensation != null && x.ParamCurrValue.Contains("维护参数地址值")))
                {
                    MessageBoxX.Show($"有要推荐补偿值，但是参数基础数据没有维护的数据存在！", "提示");
                    return;
                }

                //var tempConnect = CNCCommunicationHelps.ConnectCnc(_deviceInfoModels.Select(t => t.IpAddress).ToList());//开启连接
                foreach (var tempData in Data.GroupBy(p => p.DeviceName))
                {
                    foreach (var t in tempData)
                    {
                        if (t.RecommendedCompensation == null) continue;
                        var temp = _deviceInfoModels.First(x => x.DeviceName == t.DeviceName && x.PointName == t.PointName && x.PointPos == t.PointPos);
                        if (befParam == "")
                            befParam += $@"FAI编号：[{temp.PointName}],地址：[{temp.PointAddress}],值：[{t.ParamCurrValue}]|";
                        else
                            befParam += $@"{Environment.NewLine}FAI编号：[{temp.PointName}],地址：[{temp.PointAddress}],值：[{t.ParamCurrValue}]|";

                        //if (sendParam == "")
                        //    sendParam += $@"【智能调机】地址：[{temp.PointAddress}],值：[{t.RecommendedCompensation}]|";
                        //else
                        //    sendParam += $@"{Environment.NewLine}地址：[{temp.PointAddress}],值：[{t.RecommendedCompensation}]|";

                        if (sendParam == "")
                        {
                            if (Math.Abs(Convert.ToDouble(t.ReferenceValue) - Convert.ToDouble(t.RecommendedCompensation)) == 0)
                                sendParam += $@"【智能调机】FAI编号：[{temp.PointName}],地址：[{temp.PointAddress}],值：[{t.RecommendedCompensation}]|";
                            else
                                sendParam += $@"【智能调机】FAI编号：[{temp.PointName}],地址：[{temp.PointAddress}],值：[{t.RecommendedCompensation}]|,推荐值:[{t.ReferenceValue}]|";
                        }
                        else
                        {
                            if (Math.Abs(Convert.ToDouble(t.ReferenceValue) - Convert.ToDouble(t.RecommendedCompensation)) == 0)
                                sendParam += $@"{Environment.NewLine}FAI编号：[{temp.PointName}],地址：[{temp.PointAddress}],值：[{t.RecommendedCompensation}]|";
                            else
                                sendParam += $@"{Environment.NewLine}FAI编号：[{temp.PointName}],地址：[{temp.PointAddress}],值：[{t.RecommendedCompensation}]|,推荐值:[{t.ReferenceValue}]|";
                        }

                        CNCCommunicationHelps.SetCncValue(temp.IpAddress, temp.PointAddress, Convert.ToDecimal(t.RecommendedCompensation));
                        //CNCCommunicationHelps.SetCncValue(tempConnect, temp.IpAddress, temp.PointAddress, Convert.ToDecimal(t.RecommendedCompensation));
                    }

                    if (string.IsNullOrEmpty(befParam)) continue;
                    LogHelps.WriteTuningRecord(tempData.Key, _deviceInfoModels.Where(t => t.DeviceName == tempData.Key).Select(t => t.ProductName).Distinct()
                        .FirstOrDefault(), sendParam.TrimEnd('|'), befParam.TrimEnd('|'));
                    deviceNames.Add(tempData.Key);
                    befParam = ""; sendParam = "";
                }
                //CNCCommunicationHelps.DisConnectCnc(tempConnect);//断开连接

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：设置CNC机台数据：机台：[{string.Join(",", deviceNames)}]成功!", "智能调机");
            }
            catch (Exception ex)
            {
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：报错原因：{ex.Message + ex.StackTrace}", LogLevel.Error);
            }
        }

        #region Loading

        private void ShowLoadingPanel()
        {
            if (gLoading.Visibility != Visibility.Visible)
            {
                gLoading.Visibility = Visibility.Visible;
                list.IsEnabled = false;
                //gPager.IsEnabled = false;
                bNoData.IsEnabled = false;
            }
        }

        private void HideLoadingPanel()
        {
            if (gLoading.Visibility != Visibility.Collapsed)
            {
                gLoading.Visibility = Visibility.Collapsed;
                list.IsEnabled = true;
                //gPager.IsEnabled = true;
                bNoData.IsEnabled = true;
            }
        }

        #endregion

        private void ApplyRowStyles()
        {
            list.LoadingRow += (sender, e) =>
            {
                if (e.Row.Item is UIModel item && item.RowColor != null)
                {
                    e.Row.Background = item.RowColor;
                }
            };
        }

        private double GetDoubleValue(ICell cell)
        {
            return cell?.CellType == CellType.Numeric ? cell.NumericCellValue : 0;
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit || e.Column.Header.ToString() != "推荐补偿值") return;
            var textBox = e.EditingElement as TextBox;
            var model = (UIModel)e.Row.Item;
            if (model.ReferenceValue == null) return;

            GlobalData.Instance.IsDataValid = true;
            if (double.TryParse(textBox.Text, out double parsedValue))
            {
                if (Math.Abs(parsedValue - Convert.ToDouble(model.ReferenceValue)) > AllowedRange)
                {
                    MessageBoxX.Show($"调整范围不能超过±{AllowedRange}", "验证错误");
                    e.Cancel = true; // 阻止编辑完成
                    GlobalData.Instance.IsDataValid = false;
                }
            }
            else
            {
                MessageBoxX.Show("请输入有效数值（可含正负号和小数点）", "格式错误");
                e.Cancel = true;
                GlobalData.Instance.IsDataValid = false;
            }
        }

    }
}
