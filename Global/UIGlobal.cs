using System;
using System.Windows;

namespace SmartTuningSystem.Global
{
    public partial class UiGlobal
    {
        /// <summary>
        /// 运行UI线程
        /// </summary>
        /// <param name="action"></param>
        public static void RunUiAction(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }
    }
}
