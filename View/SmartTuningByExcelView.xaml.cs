using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// SmartTuningByExcelView.xaml 的交互逻辑
    /// </summary>
    public partial class SmartTuningByExcelView : Page
    {
        ObservableCollection<UIModel> Data = new ObservableCollection<UIModel>();//页面数据集合
        bool _running = false;//是否正在执行查询
        private string _selectedFilePath;
        private string _productName;
        private List<DeviceInfoModel> _deviceInfoModels = new List<DeviceInfoModel>();

        #region UI Models

        public class UIModel : BaseUIModel
        {
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

        public SmartTuningByExcelView()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalData.Instance.IsDataValid = true;
            list.ItemsSource = Data;//绑定数据源

            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开智能调机成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开智能调机界面！", LogLevel.Operation);
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel文件|*.xls;*.xlsx|所有文件|*.*",
                Title = "选择Excel文件",
                CheckFileExists = true,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() != true) return;
            _selectedFilePath = openFileDialog.FileName;
            txtFilePath.Text = _selectedFilePath;
        }

        private async void BtnGenerateTuningRpt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                MessageBoxX.Show("请先选择有效的Excel文件", "提示");
                return;
            }

            try
            {
                ShowLoadingPanel();//显示Loading
                if (_running) return;
                _running = true;

                Data.Clear();
                List<UIModel> dataList = new List<UIModel>();
                await Task.Run(() =>
                {
                    //解析Excel，生成调机报告
                    using (FileStream fs = new FileStream(_selectedFilePath, FileMode.Open))
                    {
                        IWorkbook workbook = new XSSFWorkbook(fs);
                        ISheet sheet = workbook.GetSheetAt(0);

                        for (int i = 1; i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue;

                            _productName = row.GetCell(0)?.ToString();
                            break;
                        }

                        _deviceInfoModels = LogManager.QueryBySql<DeviceInfoModel>($@"   select [DeviceName],[IpAddress],[ProductName],[PointName],[PointPos],[PointAddress] FROM [SmartTuningSystemDB].[dbo].[DeviceInfo] dev with(nolock) 
left join [SmartTuningSystemDB].[dbo].[DeviceInfoDetail] det with(nolock) on dev.Id=det.DeviceId and det.IsValid=1
where dev.IsValid=1 and ProductName='{_productName}'  ").ToList();

                        for (int i = 1; i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue;

                            var temp = _deviceInfoModels.First(x => x.DeviceName == row.GetCell(0)?.ToString() &&
                                                           x.PointName == row.GetCell(0)?.ToString()
                                                           && x.PointPos == row.GetCell(0)?.ToString());
                            var data = new UIModel
                            {
                                DeviceName = row.GetCell(0)?.ToString(),
                                NominalDim = GetDoubleValue(row.GetCell(1)),
                                TolMax = GetDoubleValue(row.GetCell(2)),
                                TolMin = GetDoubleValue(row.GetCell(4)),
                                USL = GetDoubleValue(row.GetCell(5)),
                                LSL = GetDoubleValue(row.GetCell(6)),
                                ParamCurrValue = Convert.ToDouble(CNCCommunicationHelps.GetCncValue(temp.IpAddress, temp.PointAddress))
                            };

                            data.CalculateFields();
                            dataList.Add(data);
                        }
                    }
                });

                await Task.Delay(300);
                bNoData.Visibility = dataList.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
                foreach (var d in dataList)
                {
                    Data.Add(d);
                }

                ApplyRowStyles();
                HideLoadingPanel();
                _running = false;
                GlobalData.Instance.IsDataValid = true;
            }
            catch (Exception ex)
            {
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}生产调机报告报错；报错原因：{ex.Message + ex.StackTrace}", LogLevel.Error);
                MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName}生产调机报告报错；报错原因：{ex.Message + ex.StackTrace}", "提示");
            }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (Data.Count == 0)
            {
                MessageBoxX.Show("请先生成调机报告", "提示");
                return;
            }

            try
            {
                string befParam = "", sendParam = "", deviceNames = "";
                foreach (var tempData in Data.GroupBy(p => p.DeviceName))
                {
                    foreach (var t in tempData)
                    {
                        var temp = _deviceInfoModels.First(x => x.DeviceName == t.DeviceName && x.PointName == t.PointName && x.PointPos == t.PointPos);
                        if (befParam == "")
                            befParam += $@"地址：[{temp.PointAddress}],值：[{t.ParamCurrValue}]|";
                        else
                            befParam += $@"{Environment.NewLine}地址：[{temp.PointAddress}],值：[{t.ParamCurrValue}]|";

                        if (sendParam == "")
                            sendParam += $@"地址：[{temp.PointAddress}],值：[{t.RecommendedCompensation}]|";
                        else
                            sendParam += $@"{Environment.NewLine}地址：[{temp.PointAddress}],值：[{t.RecommendedCompensation}]|";

                        CNCCommunicationHelps.SetCncValue(temp.IpAddress, temp.PointAddress, Convert.ToDecimal(t.RecommendedCompensation));
                    }
                    LogHelps.WriteTuningRecord(tempData.Key, _productName, sendParam.TrimEnd('|'), befParam.TrimEnd('|'));
                    deviceNames = string.Join(",", tempData.Key);
                }

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：设置CNC机台数据：机台：[{deviceNames}],产品品名：[{ _productName}]成功!", "IPQC调机");
            }
            catch (Exception ex)
            {
                LogHelps.WriteLogToDb($@"{UserGlobal.CurrUser.UserName}设置CNC机台数据报错：
                产品品名：[{_productName}],报错原因：{ex.Message + ex.StackTrace}", LogLevel.Error);
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
    }
}
