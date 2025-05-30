using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BLL;
using Model;
using Panuon.UI.Silver;
using SmartTuningSystem.Extensions;
using SmartTuningSystem.Global;
using SmartTuningSystem.Utils;
using static Model.Log;

namespace SmartTuningSystem.View.Windows
{
    /// <summary>
    /// AddDeviceDetail.xaml 的交互逻辑
    /// </summary>
    public partial class AddDeviceDetail : Window
    {
        public bool Succeed = false;
        public readonly DeviceDetailManager DeviceDetailManager = new DeviceDetailManager();
        public readonly DeviceInfoManager DeviceInfoManager = new DeviceInfoManager();
        public DeviceInfoDetail Model = new DeviceInfoDetail();

        private int _deviceId = 0;
        int editId = 0;
        bool IsEdit
        {
            get { return editId > 0; }
        }

        public AddDeviceDetail(int id = 0, int deviceId = 0)
        {
            InitializeComponent();
            this.UseCloseAnimation();

            editId = id;
            _deviceId = deviceId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitDevice();
            if (IsEdit)
                InitDeviceDetail();
        }

        /// <summary>
        /// 编辑时初始化信息
        /// </summary>
        private void InitDevice()
        {
            DeviceInfo device = DeviceInfoManager.GetDeviceById(_deviceId);
            txtDeviceName.Text = device.DeviceName;
            txtIpAddress.Text = device.IpAddress;
            txtProductName.Text = device.ProductName;
        }

        private void InitDeviceDetail()
        {
            GroupBoxMenu.Header = "机台参数编辑";
            DeviceInfoDetail detail = DeviceDetailManager.GetDeviceDetailById(editId);
            txtPointName.Text = detail.PointName;
            txtPointPos.Text = detail.PointPos;
            txtPointAddress.Text = detail.PointAddress;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Succeed = false;
            Close();
        }

        //拖动窗体
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        //编辑
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            #region 验证

            if (!txtPointName.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            if (!txtPointPos.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            if (!txtPointAddress.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            Regex ipRegex = new Regex(@"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}$");
            if (!ipRegex.IsMatch(txtIpAddress.Text))
            {
                tab.SelectedIndex = 0;
                MessageBoxX.Show("IP地址格式错误", "提示");
                return;
            }

            string name = txtPointName.Text;
            string pos = txtPointPos.Text;
            string address = txtPointAddress.Text;

            #endregion

            var deviceModel = LogManager.QueryBySql<DeviceInfoDetail>($@"select * from DeviceInfoDetail with(nolock) where IsValid=1 and DeviceId={_deviceId} ").ToList();

            if (IsEdit)
            {
                #region 验证

                //验证参数地址唯一
                if (deviceModel.Any(c => c.PointAddress == address && c.Id != editId))
                {
                    //存在
                    MessageBoxX.Show($"存在参数地址为[{address}]的数据，参数地址必须唯一", "数据存在");
                    return;
                }

                //验证点号（编号）、夹序号、参数地址俩俩唯一
                if (deviceModel.Any(c => c.PointName == name && c.PointPos == pos && c.Id != editId))
                {
                    //存在
                    MessageBoxX.Show($"存在点号（编号）[{name}]和夹序号（槽位）为[{pos}]的数据，点号（编号）和夹序号（槽位）必须唯一", "数据存在");
                    return;
                }

                if (deviceModel.Any(c => c.PointName == name && c.PointAddress == address && c.Id != editId))
                {
                    //存在
                    MessageBoxX.Show($"存在点号（编号）[{name}]和参数地址为[{address}]的数据，点号（编号）和参数地址必须唯一", "数据存在");
                    return;
                }

                if (deviceModel.Any(c => c.PointPos == pos && c.PointAddress == address && c.Id != editId))
                {
                    //存在
                    MessageBoxX.Show($"存在夹序号（槽位）[{pos}]和参数地址为[{address}]的数据，夹序号（槽位）和参数地址必须唯一", "数据存在");
                    return;
                }

                //点号（编号）、夹序号、参数地址唯一
                if (deviceModel.Any(c => c.PointName == name && c.PointPos == pos && c.PointAddress == address && c.Id != editId))
                {
                    //存在
                    MessageBoxX.Show($"存在点号（编号）[{name}]、夹序号（槽位）[{pos}]和参数地址为[{address}]的数据，点号（编号）、夹序号（槽位）和参数地址必须唯一", "数据存在");
                    return;
                }

                #endregion

                #region  编辑状态

                DeviceDetailManager.ModifyDeviceDetail(new DeviceInfoDetail
                {
                    Id = editId,
                    PointName = name,
                    PointPos = pos,
                    PointAddress = address,
                    UpdateName = UserGlobal.CurrUser.UserName,
                    UpdateNo = UserGlobal.CurrUser.UserNo
                });

                #endregion

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：点号（编号）[{name}]、夹序号（槽位）[{pos}]和参数地址为[{address}]编辑成功", "编辑机台参数");
                UserGlobal.MainWindow.WriteInfoOnBottom($"点号（编号）[{name}]、夹序号（槽位）[{pos}]和参数地址为[{address}]编辑成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：点号（编号）[{name}]、夹序号（槽位）[{pos}]和参数地址为[{address}]编辑成功！", LogLevel.Operation);
            }
            else
            {
                #region 验证

                //验证参数地址唯一
                if (deviceModel.Any(c => c.PointAddress == address))
                {
                    //存在
                    MessageBoxX.Show($"存在参数地址为[{address}]的数据，参数地址必须唯一", "数据存在");
                    return;
                }

                //验证点号（编号）、夹序号、参数地址俩俩唯一
                if (deviceModel.Any(c => c.PointName == name && c.PointPos == pos))
                {
                    //存在
                    MessageBoxX.Show($"存在点号（编号）[{name}]和夹序号（槽位）为[{pos}]的数据，点号（编号）和夹序号（槽位）必须唯一", "数据存在");
                    return;
                }

                if (deviceModel.Any(c => c.PointName == name && c.PointAddress == address))
                {
                    //存在
                    MessageBoxX.Show($"存在点号（编号）[{name}]和参数地址为[{address}]的数据，点号（编号）和参数地址必须唯一", "数据存在");
                    return;
                }

                if (deviceModel.Any(c => c.PointPos == pos && c.PointAddress == address))
                {
                    //存在
                    MessageBoxX.Show($"存在夹序号（槽位）[{pos}]和参数地址为[{address}]的数据，夹序号（槽位）和参数地址必须唯一", "数据存在");
                    return;
                }

                //点号（编号）、夹序号、参数地址唯一
                if (deviceModel.Any(c => c.PointName == name && c.PointPos == pos && c.PointAddress == address))
                {
                    //存在
                    MessageBoxX.Show($"存在点号（编号）[{name}]、夹序号（槽位）[{pos}]和参数地址为[{address}]的数据，点号（编号）、夹序号（槽位）和参数地址必须唯一", "数据存在");
                    return;
                }

                #endregion

                #region  添加状态

                DeviceDetailManager.AddDeviceDetail(new DeviceInfoDetail
                {
                    PointName = name,
                    PointPos = pos,
                    PointAddress = address,
                    DeviceId = _deviceId,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo
                });

                #endregion

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：点号（编号）[{name}]、夹序号（槽位）[{pos}]和参数地址为[{address}]添加成功", "新增机台产品");
                UserGlobal.MainWindow.WriteInfoOnBottom($"点号（编号）[{name}]、夹序号（槽位）[{pos}]和参数地址为[{address}]添加成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：点号（编号）[{name}]、夹序号（槽位）[{pos}]和参数地址为[{address}]添加成功！", LogLevel.Operation);
            }

            deviceModel = LogManager.QueryBySql<DeviceInfoDetail>($@"select * from DeviceInfoDetail with(nolock) where IsValid=1 and DeviceId={_deviceId} ").ToList();
            Model = deviceModel.First(c => c.PointName == name && c.PointPos == pos && c.PointAddress == address);
            //btnClose_Click(null, null);//模拟关闭
            Succeed = true;
            Close();
        }

        #region 校验ip

        //实时过滤非数字和点号字符输入
        //输入满3位数字自动添加点号分隔符
        //失去焦点时自动修正超范围段值(>255的自动改为255)
        //最终提交时进行完整IP格式验证
        //禁止空格键等特殊操作

        private void IpTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(newText, @"^(\d{1,3}\.){0,3}\d{0,3}$");
        }

        private void IpTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Text.Length > 0 && !textBox.Text.EndsWith("."))
            {
                int lastDot = textBox.Text.LastIndexOf('.');
                string currentSegment = lastDot >= 0 ?
                    textBox.Text.Substring(lastDot + 1) : textBox.Text;

                if (currentSegment.Length == 3 && textBox.Text.Count(c => c == '.') < 3)
                {
                    textBox.Text += ".";
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private void IpTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string[] segments = textBox.Text.Split('.');
            for (int i = 0; i < segments.Length; i++)
            {
                if (int.TryParse(segments[i], out int value) && value > 255)
                    segments[i] = "255";
            }
            textBox.Text = string.Join(".", segments);
        }

        private void IpTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) e.Handled = true; // 禁止空格键
        }

        #endregion
    }
}
