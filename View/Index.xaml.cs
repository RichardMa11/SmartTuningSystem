using System;
using System.Windows;
using System.Windows.Controls;
using BLL;
using LiveCharts;
using LiveCharts.Wpf;
using Model;
using Panuon.UI.Silver;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;
using Menu = Model.Menu;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// AdminIndex.xaml 的交互逻辑
    /// </summary>
    public partial class Index : Page
    {
        private int _pageSize = 20;
        public readonly LogManager LogManager = new LogManager();
        public readonly DeviceInfoManager DeviceInfoManager = new DeviceInfoManager();
        public readonly UserManager UserManager = new UserManager();
        public Index()
        {
            InitializeComponent();
            this.StartPageInAnimation();

            //PointLabel = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
            //DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开首页成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开首页！", LogLevel.Operation);
            UpdateDataCount();
            LoadLogData(); // 初始化加载数据
            LoadDevData();
            LoadProdData();
            LoadUserData();
        }

        /// <summary>
        /// 更新数量
        /// </summary>
        private void UpdateDataCount()
        {
            lblUserCount.Content = LogManager.QueryBySql<User>(@"select * from Users with(nolock)").Count;
            lblRoleCount.Content = LogManager.QueryBySql<Role>(@"select * from Roles with(nolock)").Count;
            lblMenuCount.Content = LogManager.QueryBySql<Menu>(@"select * from Menus with(nolock)").Count;
            lblDeviceCount.Content = LogManager.QueryBySql<DeviceInfo>(@"select * from DeviceInfo with(nolock)").Count;
        }

        //private async void LoadDataAsync()
        //{
        //    await Task.Run(LoadData);
        //}


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

        #region 机台列表

        private int _currentDevPage = 1;
        // 事件处理
        private void SearchDevi_Click(object sender, RoutedEventArgs e) => ResetAndLoadDev();
        private void BtnDevPrev_Click(object sender, RoutedEventArgs e) => ChangePageDev(-1);
        private void BtnDevNext_Click(object sender, RoutedEventArgs e) => ChangePageDev(1);
        private void LoadDevData()
        {
            // 机台列表
            var (data, total) = DeviceInfoManager.GetPagedDeviceInfo(txtSearchDevice.Text, _currentDevPage, _pageSize);

            dgDeviceLogs.ItemsSource = data;
            UpdatePagingDevUI(total);
        }

        // 更新分页界面
        private void UpdatePagingDevUI(int total)
        {
            int totalPages = (int)Math.Ceiling((double)total / _pageSize);
            txtDevPageInfo.Text = $"第 {_currentDevPage} 页 / 共 {totalPages} 页";
            btndDevPrev.IsEnabled = (_currentDevPage > 1);
            btnDevNext.IsEnabled = (_currentDevPage < totalPages);
        }

        // 重置到第一页并加载
        private void ResetAndLoadDev()
        {
            _currentDevPage = 1;
            LoadDevData();
        }

        // 切换页码
        private void ChangePageDev(int delta)
        {
            _currentDevPage += delta;
            LoadDevData();
        }

        #endregion

        #region 产品列表

        private int _currentProdPage = 1;
        // 事件处理
        private void SearchProd_Click(object sender, RoutedEventArgs e) => ResetAndLoadProd();
        private void BtnProdPrev_Click(object sender, RoutedEventArgs e) => ChangePageProd(-1);
        private void BtnProdNext_Click(object sender, RoutedEventArgs e) => ChangePageProd(1);
        private void LoadProdData()
        {
            // 产品列表
            var (data, total) = DeviceInfoManager.GetPagedDeviceInfo(txtSearchProduct.Text, _currentProdPage, _pageSize);

            dgProductLogs.ItemsSource = data;
            UpdatePagingProdUI(total);
        }

        // 更新分页界面
        private void UpdatePagingProdUI(int total)
        {
            int totalPages = (int)Math.Ceiling((double)total / _pageSize);
            txtProdPageInfo.Text = $"第 {_currentProdPage} 页 / 共 {totalPages} 页";
            btndProdPrev.IsEnabled = (_currentProdPage > 1);
            btnProdNext.IsEnabled = (_currentProdPage < totalPages);
        }

        // 重置到第一页并加载
        private void ResetAndLoadProd()
        {
            _currentProdPage = 1;
            LoadProdData();
        }

        // 切换页码
        private void ChangePageProd(int delta)
        {
            _currentProdPage += delta;
            LoadProdData();
        }

        #endregion

        #region 用户列表

        private int _currentUserPage = 1;
        // 事件处理
        private void SearchUser_Click(object sender, RoutedEventArgs e) => ResetAndLoadUser();
        private void BtnUserPrev_Click(object sender, RoutedEventArgs e) => ChangePageUser(-1);
        private void BtnUserNext_Click(object sender, RoutedEventArgs e) => ChangePageUser(1);
        private void LoadUserData()
        {
            // 用户列表
            var (data, total) = UserManager.GetPagedUser(txtSearchUser.Text, _currentUserPage, _pageSize);

            dgUserLogs.ItemsSource = data;
            UpdatePagingUserUI(total);
        }

        // 更新分页界面
        private void UpdatePagingUserUI(int total)
        {
            int totalPages = (int)Math.Ceiling((double)total / _pageSize);
            txtUserPageInfo.Text = $"第 {_currentUserPage} 页 / 共 {totalPages} 页";
            btndUserPrev.IsEnabled = (_currentUserPage > 1);
            btnUserNext.IsEnabled = (_currentUserPage < totalPages);
        }

        // 重置到第一页并加载
        private void ResetAndLoadUser()
        {
            _currentUserPage = 1;
            LoadUserData();
        }

        // 切换页码
        private void ChangePageUser(int delta)
        {
            _currentUserPage += delta;
            LoadUserData();
        }

        #endregion

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }
    }
}
