using Panuon.UI.Silver;

namespace SmartTuningSystem.Utils
{
    public abstract class BaseMainWindow : WindowX
    {
        /// <summary>
        /// 当前主题模型数据  更改主题后需下次打开生效
        /// </summary>
        //public LocalSkin.SkinModel currSkin = LocalSkin.GetModelById(LocalSettings.settings.SkinId);

        /// <summary>
        /// 禁用窗体
        /// </summary>
        /// <param name="_enable"></param>
        public void EnableMainWindow(bool _enable)
        {
            IsEnabled = _enable;
        }

        /// <summary>
        /// 蒙层
        /// </summary>
        /// <param name="_visible"></param>
        public void MaskVisible(bool _visible)
        {
            IsMaskVisible = _visible;
        }

        public abstract void SetFrameSource(string _s);//设置Frame内页
        public abstract void ReLoadMenu();//刷新导航
        public abstract void WriteInfoOnBottom(string _info, string _color = "#000000");//在底部显示Log
    }
}
