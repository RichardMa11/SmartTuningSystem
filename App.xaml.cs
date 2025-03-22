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
    }
}
