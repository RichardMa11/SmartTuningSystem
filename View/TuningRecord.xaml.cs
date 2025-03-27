using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BLL;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;
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

    }
}
