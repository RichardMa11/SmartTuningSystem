using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BLL;
using Microsoft.Win32;
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
    /// TuningRecord.xaml 的交互逻辑
    /// </summary>
    public partial class TuningRecord : Page
    {
        private int _pageSize = 20;
        public readonly TuningRecordManager TuningRecordManager = new TuningRecordManager();
        public TuningRecord()
        {
            InitializeComponent();
            this.StartPageInAnimation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开调机记录成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开调机记录！", LogLevel.Operation);
            LoadTuningRecord(); // 初始化加载数据
        }


        #region 调机日志

        private int _currentPage = 1;
        // 事件处理
        private void Search_Click(object sender, RoutedEventArgs e) => ResetAndLoad();
        private void BtnPrev_Click(object sender, RoutedEventArgs e) => ChangePage(-1);
        private void BtnNext_Click(object sender, RoutedEventArgs e) => ChangePage(1);
        private async void LoadTuningRecord()
        {
            ShowLoadingPanel();//显示Loading
            var (data, total) = TuningRecordManager.GetPagedTuningRecords(dpStart.SelectedDate, dpEnd.SelectedDate,
                txtSearch.Text, _currentPage, _pageSize);

            dgTuningRecords.ItemsSource = data;
            UpdatePagingUI(total);
            await Task.Delay(300);
            bNoData.Visibility = data.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            HideLoadingPanel();
        }

        private void ShowLoadingPanel()
        {
            if (gLoading.Visibility != Visibility.Visible)
            {
                gLoading.Visibility = Visibility.Visible;
                btnSearch.IsEnabled = false;
                dgTuningRecords.IsEnabled = false;
                gPager.IsEnabled = false;
                bNoData.IsEnabled = false;
            }
        }

        private void HideLoadingPanel()
        {
            if (gLoading.Visibility != Visibility.Collapsed)
            {
                gLoading.Visibility = Visibility.Collapsed;
                btnSearch.IsEnabled = true;
                dgTuningRecords.IsEnabled = true;
                gPager.IsEnabled = true;
                bNoData.IsEnabled = true;
            }
        }

        // 更新分页界面
        private void UpdatePagingUI(int total)
        {
            int totalPages = (int)Math.Ceiling((double)total / _pageSize);
            txtPageInfo.Text = $"第 {_currentPage} 页 / 共 {totalPages} 页";
            btnPrev.IsEnabled = (_currentPage > 1);
            btnNext.IsEnabled = (_currentPage < totalPages);
        }

        // 重置到第一页并加载
        private void ResetAndLoad()
        {
            if (dpStart.SelectedDate > dpEnd.SelectedDate)
            {
                MessageBoxX.Show("结束时间不能早于开始时间！", "查询提醒");
                return;
            }
            _currentPage = 1;
            LoadTuningRecord();
        }

        // 切换页码
        private void ChangePage(int delta)
        {
            _currentPage += delta;
            LoadTuningRecord();
        }

        #endregion

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 空引用检查
                if (dgTuningRecords == null || dgTuningRecords.Columns.Count == 0)
                {
                    MessageBoxX.Show("没有可导出的调机数据", "警告");
                    return;
                }
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Excel文件|*.xlsx",
                    Title = "保存Excel文件",
                    FileName = $"调机日志_{DateTime.Now:yyyyMMddHHmmss}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet sheet = workbook.CreateSheet("调机日志");

                    // 创建标题行
                    IRow headerRow = sheet.CreateRow(0);
                    // 获取DataView确保数据源有效
                    //var dataView = dgTuningRecords.ItemsSource as DataView;
                    //if (dataView == null) return;

                    //DataTable dt = dataView.Table;
                    // 遍历DataGrid列获取显示的表头文本
                    for (int i = 0; i < dgTuningRecords.Columns.Count; i++)
                    {
                        var column = dgTuningRecords.Columns[i];
                        string headerText = column.Header?.ToString() ?? $"列{i + 1}";

                        // 创建单元格并设置值
                        ICell cell = headerRow.CreateCell(i);
                        cell.SetCellValue(headerText);

                        // 设置表头样式
                        ICellStyle headerStyle = workbook.CreateCellStyle();
                        headerStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
                        headerStyle.FillPattern = FillPattern.SolidForeground;

                        IFont font = workbook.CreateFont();
                        font.IsBold = true;
                        headerStyle.SetFont(font);

                        cell.CellStyle = headerStyle;
                    }

                    // 导出数据内容
                    for (int rowIdx = 0; rowIdx < dgTuningRecords.Items.Count; rowIdx++)
                    {
                        IRow dataRow = sheet.CreateRow(rowIdx + 1);
                        var item = dgTuningRecords.Items[rowIdx];

                        for (int colIdx = 0; colIdx < dgTuningRecords.Columns.Count; colIdx++)
                        {
                            var column = dgTuningRecords.Columns[colIdx];
                            var cellValue = column.GetCellContent(item);
                            string value = (cellValue as TextBlock)?.Text ?? cellValue?.ToString() ?? "";
                            dataRow.CreateCell(colIdx).SetCellValue(value);
                        }
                    }

                    // 自动调整列宽
                    for (int i = 0; i < dgTuningRecords.Columns.Count; i++)
                    {
                        sheet.AutoSizeColumn(i);
                    }
                    // 设置标题样式
                    //ICellStyle headerStyle = workbook.CreateCellStyle();
                    //IFont headerFont = workbook.CreateFont();
                    //headerFont.IsBold = true;
                    //headerStyle.SetFont(headerFont);

                    //// 写入列标题
                    //for (int i = 0; i < dt.Columns.Count; i++)
                    //{
                    //    ICell cell = headerRow.CreateCell(i);
                    //    cell.SetCellValue(dt.Columns[i].ColumnName);
                    //    cell.CellStyle = headerStyle;
                    //}

                    //// 写入数据行
                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //{
                    //    IRow dataRow = sheet.CreateRow(i + 1);
                    //    for (int j = 0; j < dt.Columns.Count; j++)
                    //    {
                    //        dataRow.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                    //    }
                    //}

                    //// 自动调整列宽
                    //for (int i = 0; i < dt.Columns.Count; i++)
                    //{
                    //    sheet.AutoSizeColumn(i);
                    //}

                    // 保存文件
                    using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create))
                    {
                        workbook.Write(fs);
                    }

                    MessageBoxX.Show("Excel导出成功！", "提示");
                }
            }
            catch (Exception ex)
            {
                MessageBoxX.Show($@"{UserGlobal.CurrUser.UserName}导出失败；报错原因：{ex.Message + ex.StackTrace}", "提示");
            }
        }
    }
}
