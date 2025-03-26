using System.Windows;
using SmartTuningSystem.Utils;

namespace SmartTuningSystem
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ProcessManager.GetProcessLock();
        }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    // 注册未处理异常事件
        //    DispatcherUnhandledException += App_DispatcherUnhandledException;
        //    base.OnStartup(e);
        //}

        //private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        //{
        //    // 处理所有未捕获的异常
        //    MessageBoxX.Show($"全局异常: {e.Exception.Message}", "系统错误");
        //    e.Handled = true; // 标记为已处理，阻止程序崩溃
        //    Application.Current.Shutdown();
        //}
    }
}
