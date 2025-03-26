using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using SmartTuningSystem.Global;

namespace SmartTuningSystem.Extensions
{
    public static class PageExtensions
    {
        static PageExtensions()
        {

        }
        public static void StartPageInAnimation(this Page _page)
        {
            Storyboard sb = new Storyboard();
            ThicknessAnimation margin = new ThicknessAnimation();
            DoubleAnimation opacity = new DoubleAnimation();
            margin.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 450));
            opacity.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 350));
            margin.From = new Thickness(100, 0, -100, 0);
            opacity.From = 0;
            margin.To = new Thickness(0);
            margin.DecelerationRatio = 0.9;
            opacity.To = 1;

            Storyboard.SetTarget(margin, _page);
            Storyboard.SetTarget(opacity, _page);
            Storyboard.SetTargetProperty(margin, new PropertyPath("Margin", new object[] { }));
            Storyboard.SetTargetProperty(opacity, new PropertyPath("Opacity", new object[] { }));
            sb.Children.Add(margin);
            sb.Children.Add(opacity);
            sb.Begin();
        }

        public static void MaskVisible(this Page page, bool v)
        {
            var parentWindow = UserGlobal.MainWindow;
            if (parentWindow != null)
            {
                parentWindow.IsMaskVisible = v;
            }
        }

        //public static void Log(this Page _page, string _logStr)
        //{
        //    MainWindowGlobal.MainWindow.Log(_logStr);
        //}

        /// <summary>
        /// 扩展Windows的Log
        /// </summary>
        /// <param name="_window"></param>
        /// <param name="_logStr"></param>
        //public static void Log(this Window _window, string _logStr)
        //{
        //    MainWindowGlobal.MainWindow.WriteInfoOnBottom(_logStr);
        //}
    }
}
