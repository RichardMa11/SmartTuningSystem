using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BLL;
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
        public ObservableCollection<UIModel> tvMenus = new ObservableCollection<UIModel>();//页面数据集合
        public readonly MenuManager MenuManager = new MenuManager();
        Brush selectedMenuColor = null;//选中导航颜色

        #region UI Models

        public class UIModel : BaseUIModel
        {
            public int Id { get; set; }

            private string pageName = "";
            public string PageName
            {
                get => pageName;
                set
                {
                    pageName = value;
                    NotifyPropertyChanged("PageName");
                }
            }

            private string pagePath = "";
            public string PagePath
            {
                get => pagePath;
                set
                {
                    pagePath = value;
                    NotifyPropertyChanged("PagePath");
                }
            }

            private int order = 0;
            public int Order
            {
                get => order;
                set
                {
                    order = value;
                    NotifyPropertyChanged("Order");
                }
            }

            private string icon = "";
            public string Icon
            {
                get => icon;
                set
                {
                    icon = value;
                    NotifyPropertyChanged("Icon");
                }
            }

            private int fontSize = 15;
            public int FontSize //字体大小
            {
                get => fontSize;
                set
                {
                    fontSize = value;
                    NotifyPropertyChanged("FontSize");
                }
            }

            private Brush foreground = null;
            public Brush Foreground //前景色
            {
                get => foreground;
                set
                {
                    foreground = value;
                    NotifyPropertyChanged("Foreground");
                }
            }
            private Brush headerForeground = null;
            public Brush HeaderForeground //前景色
            {
                get => headerForeground;
                set
                {
                    headerForeground = value;
                    NotifyPropertyChanged("HeaderForeground");
                }
            }

            //public Model.Menu Tag { get; set; }//数据

            //public string Header { get; set; }//标题
        }

        #endregion 
        public MainWindow()
        {
            InitializeComponent();

            LblCurrUser.Text = UserGlobal.CurrUser.UserName;
            UserGlobal.MainWindow = Application.Current.MainWindow as MainWindow;
            Closing += WindowX_Closing;

            selectedMenuColor = new SolidColorBrush(Colors.Black);
            //if(UserGlobal.CurrUser.UserNo== "00001")
            //List<Model.Menu> menus = MenuManager.GetAllMenu();

            foreach (var item in UserGlobal.VUserRoleMenus)
            {
                if (item.MenuId != null)
                {
                    UIModel model = new UIModel()
                    {
                        Id = (int)item.MenuId,
                        PageName = item.PageName,
                        PagePath = item.PagePath,
                        Order = (int)item.Order,
                        Icon = FontAwesomeCommon.GetUnicode(item.Icon),
                        FontSize = 15,
                        Foreground = StyleHelper.ConvertToSolidColorBrush("#FFFFFFFF"),
                        HeaderForeground = StyleHelper.ConvertToSolidColorBrush("#FFFFFFFF")
                    };
                    tvMenus.Add(model);
                }
            }
            tvMenu.ItemsSource = tvMenus;
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
                UIModel targetItem = tvMenu.SelectedItem as UIModel;

                #region 更新导航字体颜色

                if (tvMenus.Any(c => c.Foreground == selectedMenuColor))
                {
                    var menuItem = tvMenus.Single(c => c.Foreground == selectedMenuColor);
                    menuItem.Foreground = StyleHelper.ConvertToSolidColorBrush("#FFFFFFFF");
                    menuItem.HeaderForeground = StyleHelper.ConvertToSolidColorBrush("#FFFFFFFF");
                    menuItem.FontSize = 15;
                }
                var menuItemSelection = tvMenus.Single(c => c.Id == targetItem.Id);
                menuItemSelection.Foreground = selectedMenuColor;
                menuItemSelection.HeaderForeground = StyleHelper.ConvertToSolidColorBrush("#FF003B67");//#FF003B67
                menuItemSelection.FontSize = 25;

                #endregion 

                foreach (var ur in UserGlobal.VUserRoleMenus)
                {
                    if (targetItem.PageName.ToString() == ur.PageName || UserGlobal.CurrUser.UserNo == "00001")
                    {
                        isPermission = true;
                        break;
                    }
                }
                if (isPermission)
                    mainFrame.Source = new Uri(targetItem.PagePath.ToString(), UriKind.RelativeOrAbsolute);
                else
                {
                    MessageBoxX.Show($@"您没有{targetItem.PageName.ToString()}的权限，如果需要，找管理员开通！！！", "权限提醒", this, MessageBoxButton.OK);
                }

                //ModulePage page = targetItem.Tag as ModulePage;
                //if (page == null) return;
                //mainFrame.Source = new Uri(page.FullPath, UriKind.RelativeOrAbsolute);
                //TreeViewItem targetItem = tvMenu.SelectedItem as TreeViewItem;
                //foreach (var ur in UserGlobal.VUserRoleMenus)
                //{
                //    if (targetItem.Header.ToString() == ur.PageName || ur.RoleNo == "00001")
                //    {
                //        isPermission = true;
                //        break;
                //    }
                //}
                //if (isPermission)
                //    mainFrame.Source = new Uri(targetItem.Tag.ToString(), UriKind.RelativeOrAbsolute);
                //else
                //{
                //    MessageBoxX.Show($@"您没有{targetItem.Header.ToString()}的权限，如果需要，找管理员开通！！！", "权限提醒", this, MessageBoxButton.OK);
                //}
            }
        }

        private void tvMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tvMenu.SelectedItem != null)
            {
                UIModel targetItem = tvMenu.SelectedItem as UIModel;

                #region 更新导航字体颜色

                if (tvMenus.Any(c => c.Foreground == new SolidColorBrush(Colors.Black)))
                {
                    var menuItem = tvMenus.Single(c => c.Foreground == new SolidColorBrush(Colors.Black));
                    menuItem.Foreground = StyleHelper.ConvertToSolidColorBrush("#FFFFFFFF");
                    menuItem.HeaderForeground = StyleHelper.ConvertToSolidColorBrush("#FFFFFFFF");
                    menuItem.FontSize = 15;
                }
                var menuItemSelection = tvMenus.Single(c => c.Id == targetItem.Id);
                menuItemSelection.Foreground = new SolidColorBrush(Colors.Black);
                menuItemSelection.HeaderForeground = StyleHelper.ConvertToSolidColorBrush("#FF003B67");//#FF003B67
                menuItemSelection.FontSize = 25;

                #endregion 

                //ModulePage page = targetItem.Tag as ModulePage;
                //if (page == null) return;
                //mainFrame.Source = new Uri(page.FullPath, UriKind.RelativeOrAbsolute);
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
