using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using SmartTuningSystem.Global;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// AdminIndex.xaml 的交互逻辑
    /// </summary>
    public partial class Index : Page
    {
        // 日志数据集合
        private ObservableCollection<LogEntry> _systemLogs = new ObservableCollection<LogEntry>();
        private ObservableCollection<LogEntry> _authLogs = new ObservableCollection<LogEntry>();
        public Index()
        {
            InitializeComponent();
            this.StartPageInAnimation();

            PointLabel = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
            DataContext = this;

            LoadDemoData();

            // 绑定数据源
            dgSystemLogs.DataContext = _systemLogs;
            dgAuthLogs.DataContext = _authLogs;
        }

        // 加载示例数据
        private void LoadDemoData()
        {
            // 系统日志示例
            _systemLogs.Add(new LogEntry
            {
                Timestamp = DateTime.Now.AddMinutes(-5),
                Level = "INFO",
                Message = "系统启动成功"
            });

            // 授权日志示例
            _authLogs.Add(new LogEntry
            {
                Timestamp = DateTime.Now.AddMinutes(-3),
                User = "admin",
                Message = "用户登录成功"
            });
        }

        // 系统日志搜索
        private void SearchSystem_Click(object sender, RoutedEventArgs e)
        {
            var keyword = txtSearchSystem.Text.ToLower();
            var filtered = _systemLogs.Where(l =>
                l.Message.ToLower().Contains(keyword) ||
                l.Level.ToLower().Contains(keyword)
            ).ToList();

            dgSystemLogs.DataContext = new ObservableCollection<LogEntry>(filtered);
        }

        // 授权日志搜索
        private void SearchAuth_Click(object sender, RoutedEventArgs e)
        {
            var keyword = txtSearchAuth.Text.ToLower();
            var filtered = _authLogs.Where(l =>
                l.User.ToLower().Contains(keyword) ||
                l.Message.ToLower().Contains(keyword)
            ).ToList();

            dgAuthLogs.DataContext = new ObservableCollection<LogEntry>(filtered);
        }

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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDataCount();

            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开首页成功。");
        }

        /// <summary>
        /// 更新数量
        /// </summary>
        private void UpdateDataCount()
        {
            //using (CoreDBContext context = new CoreDBContext())
            //{
            //    lblUserCount.Content = context.User.Any() ? context.User.Where(c => !c.IsDel).Count() : 0;
            //    lblRoleCount.Content = context.Role.Any() ? context.Role.Count() : 0;
            //    lblPluginsCount.Content = context.Plugins.Count();
            //    lblPositionCount.Content = context.DepartmentPosition.Any(c => !c.IsDel) ? context.DepartmentPosition.Count(c => !c.IsDel) : 0;
            //}
        }
    }

    // 日志实体类
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }    // 用于系统日志
        public string User { get; set; }     // 用于授权记录
        public string Message { get; set; }
    }
}
