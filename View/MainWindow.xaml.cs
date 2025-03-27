using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Panuon.UI.Silver;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using SmartTuningSystem.View.Windows;
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowX
    {
        private bool _isClose = false;
        public MainWindow()
        {
            InitializeComponent();

            LblCurrUser.Text = UserGlobal.CurrUser.UserName;
            UserGlobal.MainWindow = Application.Current.MainWindow as MainWindow;
            Closing += WindowX_Closing;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            reSizeGrid.Visibility = Visibility.Collapsed;
        }

        #region UI Method

        private void TreeView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            bool isPermission = false;
            if (tvMenu.SelectedItem != null)
            {
                TreeViewItem targetItem = tvMenu.SelectedItem as TreeViewItem;
                foreach (var ur in UserGlobal.VUserRoleMenus)
                {
                    if (targetItem.Header.ToString() == ur.PageName || ur.RoleNo == "00001")
                    {
                        isPermission = true;
                        break;
                    }
                }
                if (isPermission)
                    mainFrame.Source = new Uri(targetItem.Tag.ToString(), UriKind.RelativeOrAbsolute);
                else
                {
                    MessageBoxX.Show($@"您没有{targetItem.Header.ToString()}的权限，如果需要，找管理员开通！！！", "权限提醒", this, MessageBoxButton.OK);
                }
            }
        }

        #endregion

        #region 窗体改变事件

        double newWindowWidth = 0;
        double newWindowHeight = 0;

        /// <summary>
        /// 窗体大小更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BaseMainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (reSizeGrid.Visibility == Visibility.Collapsed && !_isClose) reSizeGrid.Visibility = Visibility.Visible;//如果隐藏 显示出来
            lblNewSizeInfo.Content = $"当前界面宽高：{e.NewSize.Width} * {e.NewSize.Height}";

            newWindowWidth = e.NewSize.Width;
            newWindowHeight = e.NewSize.Height;
        }

        //private void btnSaveSize_Click(object sender, RoutedEventArgs e)
        //{
        //    reSizeGrid.Visibility = Visibility.Collapsed;//隐藏
        //    //LocalSettings.UpdateSize(newWindowWidth, newWindowHeight);//更新本地数据
        //    windowWidth = newWindowWidth;
        //    windowHeight = newWindowHeight;
        //}

        private void btnOmit_Click(object sender, RoutedEventArgs e)
        {
            reSizeGrid.Visibility = Visibility.Collapsed;//隐藏
        }

        /// <summary>
        /// 恢复设置的大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReSize_Click(object sender, RoutedEventArgs e)
        {
            Width = 1400;
            Height = 759;
        }

        #endregion

        #region 完全关闭窗体

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            var result = MessageBoxX.Show("是否退出？", "退出提醒", this, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}退出登录！", LogLevel.Authorization);
                _isClose = true;
                Closing -= WindowX_Closing;
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation dh = new DoubleAnimation();
            DoubleAnimation dw = new DoubleAnimation();
            dh.Duration = dw.Duration = sb.Duration = new Duration(new TimeSpan(0, 0, 1));
            dh.To = dw.To = 0;
            Storyboard.SetTarget(dh, this);
            Storyboard.SetTarget(dw, this);
            Storyboard.SetTargetProperty(dh, new PropertyPath("Height", new object[] { }));
            Storyboard.SetTargetProperty(dw, new PropertyPath("Width", new object[] { }));
            sb.Children.Add(dh);
            sb.Children.Add(dw);
            sb.Completed += AnimationCompleted;
            sb.Begin();
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region 注销

        private void btnReLogin_Click(object sender, RoutedEventArgs e)
        {
            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}注销成功！", LogLevel.Authorization);
            //System.Windows.Forms.Application.Restart();
            //Application.Current.Shutdown();
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        #endregion

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            IsMaskVisible = true;
            UpdatePassword updatePassword = new UpdatePassword();
            updatePassword.ShowDialog();
            IsMaskVisible = false;
        }

        /// <summary>
        /// 在窗体底部显示文字
        /// </summary>
        /// <param name="info"></param>
        /// <param name="color"></param>

        public void WriteInfoOnBottom(string info, string color = "#000000")
        {
            // 确保操作在UI线程执行
            Dispatcher.Invoke(() =>
            {
                lblInfo.Content = info;
                lblInfo.Foreground = ColorHelper.ConvertToSolidColorBrush(color);
            });
        }
    }
}
