using System;
using System.Windows;
using System.Windows.Controls;
using BLL;
using Panuon.UI.Silver;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// AdminIndex.xaml 的交互逻辑
    /// </summary>
    public partial class TuningRecord : Page
    {
        private int _pageSize = 20;
        public readonly LogManager LogManager = new LogManager();
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
            LoadLogData(); // 初始化加载数据
        }


        #region 系统日志

        private int _currentLogPage = 1;
        // 事件处理
        private void SearchSystem_Click(object sender, RoutedEventArgs e) => ResetAndLoad();
        private void BtnPrev_Click(object sender, RoutedEventArgs e) => ChangePage(-1);
        private void BtnNext_Click(object sender, RoutedEventArgs e) => ChangePage(1);
        private void LoadLogData()
        {
            // 系统日志
            var (data, total) = LogManager.GetPagedLogs(dpStart.SelectedDate, dpEnd.SelectedDate,
                cmbLevel.SelectedValue != null ? Convert.ToInt32(cmbLevel.SelectedValue) : (int?)null,
                txtSearchSystem.Text, _currentLogPage, _pageSize);

            dgSystemLogs.ItemsSource = data;
            UpdatePagingUI(total);

            //          _systemLogs = LogManager.QueryBySql<LogModel>(@"SELECT 
            //     CASE 
            //        WHEN l.[LogType] = 0 THEN 'Info' 
            //        WHEN l.[LogType] = 1 THEN 'Error' 
            //        WHEN l.[LogType] = 2 THEN 'Warning' 
            //        ELSE 'Unknown' 
            //  END AS LogLevel
            //    ,[LogStr]
            //    ,[CreateTime]
            //FROM [SmartTuningSystemDB].[dbo].[Logs] l with(nolock) where l.LogType in (0,1,2) order by CreateTime desc").ToList();
            //using (var conn = new SqlConnection("YourConnectionString"))
            //{
            //    using (var multi = conn.QueryMultiple(sql, new
            //    {
            //        StartDate = startDate,
            //        EndDate = endDate,
            //        Level = level,
            //        Keyword = keyword,
            //        Offset = (_currentPage - 1) * _pageSize,
            //        PageSize = _pageSize
            //    }))
            //    {
            //        dgData.ItemsSource = multi.Read<Log>(); // 读取分页数据
            //        _totalRecords = multi.ReadSingle<int>(); // 读取总记录数
            //    }
            //}
            // 分页 SQL
            //var sql = @"
            //    SELECT * FROM Logs with(nolock)
            //    WHERE (@StartDate IS NULL OR CreateTime >= @StartDate)
            //      AND (@EndDate IS NULL OR CreateTime <= @EndDate)
            //      AND (@LogType IS NULL OR LogType = @LogType)
            //      AND (@Keyword IS NULL OR LogStr LIKE '%' + @Keyword + '%')
            //    ORDER BY CreateTime DESC
            //    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            //    SELECT COUNT(*) FROM Logs with(nolock)
            //    WHERE (@StartDate IS NULL OR CreateTime >= @StartDate)
            //      AND (@EndDate IS NULL OR CreateTime <= @EndDate)
            //      AND (@LogType IS NULL OR LogType = @LogType)
            //      AND (@Keyword IS NULL OR LogStr LIKE '%' + @Keyword + '%');";
        }

        // 更新分页界面
        private void UpdatePagingUI(int total)
        {
            int totalPages = (int)Math.Ceiling((double)total / _pageSize);
            txtPageInfo.Text = $"第 {_currentLogPage} 页 / 共 {totalPages} 页";
            btnPrev.IsEnabled = (_currentLogPage > 1);
            btnNext.IsEnabled = (_currentLogPage < totalPages);
        }

        // 重置到第一页并加载
        private void ResetAndLoad()
        {
            if (dpStart.SelectedDate > dpEnd.SelectedDate)
            {
                MessageBoxX.Show("结束时间不能早于开始时间！", "查询提醒");
                return;
            }
            _currentLogPage = 1;
            LoadLogData();
        }

        // 切换页码
        private void ChangePage(int delta)
        {
            _currentLogPage += delta;
            LoadLogData();
        }

        #endregion

    }
}
