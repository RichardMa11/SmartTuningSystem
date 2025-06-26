using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    /// SmartTuningView.xaml 的交互逻辑
    /// </summary>
    public partial class SmartTuningView : Page
    {
        public int DeviceId = 0;
        public string Ip = "127.0.0.1";
        public string ProductName = "";
        public string DeviceName = "";

        public SmartTuningView()
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
                UserGlobal.MainWindow.WriteInfoOnBottom("打开智能调机（IPQC）界面成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开智能调机（IPQC）！", LogLevel.Operation);
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
            //UpdateGridDetailAsync(DeviceId);
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string befParam = "", sendParam = "";
                //foreach (var p in DataDetail)
                //{
                //    if (befParam == "")
                //        befParam += $@"地址：[{p.PointAddress}],值：[{p.ParamCurrValue}]|";
                //    else
                //        befParam += $@"{Environment.NewLine}地址：[{p.PointAddress}],值：[{p.ParamCurrValue}]|";

                //    if (sendParam == "")
                //        sendParam += $@"地址：[{p.PointAddress}],值：[{p.ParamModifyValue}]|";
                //    else
                //        sendParam += $@"{Environment.NewLine}地址：[{p.PointAddress}],值：[{p.ParamModifyValue}]|";

                //    CNCCommunicationHelps.SetCncValue(Ip, p.PointAddress, p.ParamModifyValue);
                //}

                LogHelps.WriteTuningRecord(DeviceName, ProductName, sendParam.TrimEnd('|'), befParam.TrimEnd('|'));
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

        private void BtnGenerateTuningRpt_Click(object sender, RoutedEventArgs e)
        {
            //            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            //            {
            //                MessageBoxX.Show("请先选择有效的Excel文件", "提示");
            //                return;
            //            }

            //            try
            //            {
            //                ShowLoadingPanel();//显示Loading
            //                if (_running) return;
            //                _running = true;

            //                Data.Clear();
            //                List<UIModel> dataList = new List<UIModel>();
            //                await Task.Run(() =>
            //                {
            //                    //解析Excel，生成调机报告
            //                    using (FileStream fs = new FileStream(_selectedFilePath, FileMode.Open))
            //                    {
            //                        IWorkbook workbook = new XSSFWorkbook(fs);
            //                        ISheet sheet = workbook.GetSheetAt(0);

            //                        for (int i = 1; i <= sheet.LastRowNum; i++)
            //                        {
            //                            IRow row = sheet.GetRow(i);
            //                            if (row == null) continue;

            //                            _productName = row.GetCell(0)?.ToString();
            //                            break;
            //                        }

            //                        _deviceInfoModels = LogManager.QueryBySql<DeviceInfoModel>($@"   select [DeviceName],[IpAddress],[ProductName],[PointName],[PointPos],[PointAddress] FROM [SmartTuningSystemDB].[dbo].[DeviceInfo] dev with(nolock) 
            //left join [SmartTuningSystemDB].[dbo].[DeviceInfoDetail] det with(nolock) on dev.Id=det.DeviceId and det.IsValid=1
            //where dev.IsValid=1 and ProductName='{_productName}'  ").ToList();

            //                        for (int i = 1; i <= sheet.LastRowNum; i++)
            //                        {
            //                            IRow row = sheet.GetRow(i);
            //                            if (row == null) continue;

            //                            var temp = _deviceInfoModels.First(x => x.DeviceName == row.GetCell(0)?.ToString() &&
            //                                                           x.PointName == row.GetCell(0)?.ToString()
            //                                                           && x.PointPos == row.GetCell(0)?.ToString());
            //                            var data = new UIModel
            //                            {
            //                                DeviceName = row.GetCell(0)?.ToString(),
            //                                NominalDim = GetDoubleValue(row.GetCell(1)),
            //                                TolMax = GetDoubleValue(row.GetCell(2)),
            //                                TolMin = GetDoubleValue(row.GetCell(4)),
            //                                USL = GetDoubleValue(row.GetCell(5)),
            //                                LSL = GetDoubleValue(row.GetCell(6)),
            //                                ParamCurrValue = Convert.ToDouble(CNCCommunicationHelps.GetCncValue(temp.IpAddress, temp.PointAddress))
            //                            };

            //                            data.CalculateFields();
            //                            dataList.Add(data);
            //                        }
            //                    }
            //                });

            //                await Task.Delay(300);
            //                bNoData.Visibility = dataList.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
            //                foreach (var d in dataList)
            //                {
            //                    Data.Add(d);
            //                }

            //                ApplyRowStyles();
            //                HideLoadingPanel();
            //                _running = false;
            //                GlobalData.Instance.IsDataValid = true;
            //            }
            //            catch (Exception ex)
            //            {
            //                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}生产调机报告报错；报错原因：{ex.Message + ex.StackTrace}", LogLevel.Error);
            //                MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName}生产调机报告报错；报错原因：{ex.Message + ex.StackTrace}", "提示");
            //            }
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

        #region 调机报告

        public readonly DeviceDetailManager DeviceDetailManager = new DeviceDetailManager();
        ObservableCollection<UIModelDetail> DataDetail = new ObservableCollection<UIModelDetail>();//页面数据集合
        bool runningParam = false;//是否正在执行查询

        #region UI Models

        public class UIModelDetail : BaseUIModel
        {
            public int DeviceId { get; set; }
            //机台
            public string DeviceName { get; set; }

            //点号（编号）
            public string PointName { get; set; }
            //夹序号（槽位）
            public string PointPos { get; set; }
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

            private double recommendedCompensation = 0;
            public double RecommendedCompensation
            {
                get => recommendedCompensation;
                set
                {
                    recommendedCompensation = value;
                    NotifyPropertyChanged("RecommendedCompensation");
                }
            }

            //变量当前值
            public double ParamCurrValue { get; set; }
            public Brush RowColor { get; set; }
            public void CalculateFields()
            {
                Deviation = MeasureValue - NominalDim;
                Tolerance = (USL - LSL) * 0.5 * 0.6;
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
                    RecommendedCompensation = 0;
                    RowColor = Brushes.LightGreen;
                }
                else if (Math.Abs(Deviation) >= Tolerance && Deviation > 0 && qualified != "NG")
                {
                    StatusDescription = "情况2:偏差值绝对值>=调机公差&&合格偏上";
                    CompensationDescription = "按照偏差值的50%推荐补偿值";
                    RecommendedCompensation = -Deviation * 0.5;
                    RowColor = Brushes.Gold;
                }
                else if (Math.Abs(Deviation) >= Tolerance && Deviation < 0 && qualified != "NG")
                {
                    StatusDescription = "情况3:偏差值绝对值>=调机公差&&合格偏下";
                    CompensationDescription = "按照偏差值的50%推荐补偿值";
                    //RecommendedCompensation = (-Deviation * 0.5).ToString("F4");
                    RecommendedCompensation = -Deviation * 0.5;
                    RowColor = Brushes.Gold;
                }
                else if (Math.Abs(Deviation) < NominalDim * 0.5 && qualified == "NG")
                {
                    StatusDescription = "情况4:超差NG&偏差值<标准值*0.5";
                    CompensationDescription = "按照偏差值的80%推荐补偿值";
                    RecommendedCompensation = -Deviation * 0.8;
                    RowColor = Brushes.Coral;
                }
                else if (Math.Abs(Deviation) >= NominalDim * 0.5 && qualified == "NG")
                {
                    StatusDescription = "情况5:超差NG&偏差值>=标准值*0.5";
                    CompensationDescription = "不推荐，报警";
                    RecommendedCompensation = 0;
                    RowColor = Brushes.Red;
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

            ShowLoadingPanelParam();//显示Loading
            if (runningParam) return;
            runningParam = true;

            DataDetail.Clear();
            List<DeviceInfoDetail> models = new List<DeviceInfoDetail>();

            await Task.Run(() =>
            {
                //var (data, total) = DeviceDetailManager.GetPagedDeviceDetail(searchText, currPageParam, pageSizeParam, id);

                ////生成分页数据
                //models = data.OrderBy(c => c.PointName).ToList();
            });

            await Task.Delay(300);
            bNoDataParam.Visibility = models.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
            foreach (var item in models)
            {
                UIModelDetail _model = new UIModelDetail
                {
                    //PointName = item.PointName,
                    //PointPos = item.PointPos,
                    //PointAddress = item.PointAddress,
                    //DeviceId = item.DeviceId,
                    //ParamCurrValue = CNCCommunicationHelps.GetCncValue(Ip, item.PointAddress)
                };

                DataDetail.Add(_model);
            }

            HideLoadingPanelParam();
            runningParam = false;
        }

        #region Grid

        #region Loading

        private void ShowLoadingPanelParam()
        {
            if (gLoadingParam.Visibility != Visibility.Visible)
            {
                gLoadingParam.Visibility = Visibility.Visible;
                listParam.IsEnabled = false;
                bNoDataParam.IsEnabled = false;
            }
        }

        private void HideLoadingPanelParam()
        {
            if (gLoadingParam.Visibility != Visibility.Collapsed)
            {
                gLoadingParam.Visibility = Visibility.Collapsed;
                listParam.IsEnabled = true;
                bNoDataParam.IsEnabled = true;
            }
        }

        #endregion

        #endregion

        #endregion
    }

    public class NumericValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo culture)
        {
            GlobalData.Instance.IsDataValid = double.TryParse(value?.ToString(),
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                culture, out _);
            return GlobalData.Instance.IsDataValid ? ValidationResult.ValidResult
                : new ValidationResult(false, "请输入有效数值（可含正负号和小数点）");
        }
    }
}
