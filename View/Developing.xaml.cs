using System.Windows.Controls;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View
{
    /// <summary>
    /// PluginsMsg.xaml 的交互逻辑
    /// </summary>
    public partial class Developing : Page
    {
        public Developing()
        {
            InitializeComponent();
            this.StartPageInAnimation();

            if (UserGlobal.MainWindow != null)
                UserGlobal.MainWindow.WriteInfoOnBottom("打开测试页成功。");

            LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}打开测试页！", LogLevel.Operation);
        }
    }
}
