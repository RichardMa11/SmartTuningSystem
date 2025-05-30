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
    /// AddDevice.xaml 的交互逻辑
    /// </summary>
    public partial class AddDevice : Window
    {
        public bool Succeed = false;
        public readonly DeviceInfoManager DeviceInfoManager = new DeviceInfoManager();
        public DeviceInfo Model = new DeviceInfo();

        int editId = 0;
        bool IsEdit
        {
            get { return editId > 0; }
        }

        public AddDevice(int id = 0)
        {
            InitializeComponent();
            this.UseCloseAnimation();

            editId = id;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsEdit)
                InitDeviceInfo();
        }

        /// <summary>
        /// 编辑时初始化信息
        /// </summary>
        private void InitDeviceInfo()
        {
            GroupBoxMenu.Header = "机台产品编辑";
            DeviceInfo device = DeviceInfoManager.GetDeviceById(editId);
            txtDeviceName.Text = device.DeviceName;
            txtIpAddress.Text = device.IpAddress;
            txtProductName.Text = device.ProductName;
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

            if (!txtDeviceName.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            if (!txtIpAddress.NotEmpty())
            {
                tab.SelectedIndex = 0;
                return;
            }

            if (!txtProductName.NotEmpty())
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

            string name = txtDeviceName.Text;
            string ip = txtIpAddress.Text;
            string product = txtProductName.Text;

            var deviceModel = LogManager
                .QueryBySql<DeviceModel>(
                    @"select distinct DeviceName,IpAddress from DeviceInfo with(nolock) where IsValid=1 ").ToList();

            //验证机台、IP唯一
            if (deviceModel.Any(c => c.DeviceName == name && c.IpAddress != ip))
            {
                //存在
                MessageBoxX.Show($"存在机台编号[{name}]并且IP不为[{ip}]的数据，机台和ip必须唯一", "数据存在");
                return;
            }

            if (deviceModel.Any(c => c.DeviceName != name && c.IpAddress == ip))
            {
                //存在
                MessageBoxX.Show($"存在IP为[{ip}]并且机台编号不为[{name}]的数据，机台和ip必须唯一", "数据存在");
                return;
            }

            #endregion

            if (IsEdit)
            {
                #region 验证

                //机台、IP、产品品名唯一
                var deviceInfos = DeviceInfoManager.GetDeviceByParam(device: name, ip: ip, product: product);
                if (deviceInfos.Any(c => c.Id != editId))
                {
                    //存在
                    MessageBoxX.Show($"存在相同机台编号[{name}]、ip[{ip}]以及产品品名[{product}]", "数据存在");
                    return;
                }

                #endregion

                #region  编辑状态

                DeviceInfoManager.ModifyDevice(new DeviceInfo
                {
                    Id = editId,
                    DeviceName = name,
                    IpAddress = ip,
                    ProductName = product,
                    UpdateName = UserGlobal.CurrUser.UserName,
                    UpdateNo = UserGlobal.CurrUser.UserNo
                });

                #endregion

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：机台编号[{name}]、ip[{ip}]以及产品品名[{product}]编辑成功", "编辑机台产品");
                UserGlobal.MainWindow.WriteInfoOnBottom($"机台编号[{name}]、ip[{ip}]以及产品品名[{product}]编辑成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：机台编号[{name}]、ip[{ip}]以及产品品名[{product}]编辑成功！", LogLevel.Operation);
            }
            else
            {
                #region 验证

                //机台、IP、产品品名唯一
                if (DeviceInfoManager.GetDeviceByParam(device: name, ip: ip, product: product).Count != 0)
                {
                    //存在
                    MessageBoxX.Show($"存在相同机台编号[{name}]、ip[{ip}]以及产品品名[{product}]", "数据存在");
                    return;
                }

                #endregion

                #region  添加状态

                DeviceInfoManager.AddDevice(new DeviceInfo
                {
                    DeviceName = name,
                    IpAddress = ip,
                    ProductName = product,
                    CreateName = UserGlobal.CurrUser.UserName,
                    CreateNo = UserGlobal.CurrUser.UserNo
                });

                #endregion

                MessageBoxX.Show($"{UserGlobal.CurrUser.UserName} 操作：机台编号[{name}]、ip[{ip}]以及产品品名[{product}]添加成功", "新增机台产品");
                UserGlobal.MainWindow.WriteInfoOnBottom($"机台编号[{name}]、ip[{ip}]以及产品品名[{product}]添加成功。");
                LogHelps.WriteLogToDb($"{UserGlobal.CurrUser.UserName}操作：机台编号[{name}]、ip[{ip}]以及产品品名[{product}]添加成功！", LogLevel.Operation);
            }

            Model = DeviceInfoManager.GetDeviceByParam(device: name, ip: ip, product: product).First();
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
