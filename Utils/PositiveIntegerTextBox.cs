using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace SmartTuningSystem.Utils
{
    public class PositiveIntegerTextBox : TextBox
    {
        public PositiveIntegerTextBox()
        {
            // 禁用输入法
            InputMethod.SetIsInputMethodEnabled(this, false);

            // 绑定事件
            PreviewTextInput += OnPreviewTextInput;
            PreviewKeyDown += OnPreviewKeyDown;
            DataObject.AddPastingHandler(this, OnPaste);
            TextChanged += OnTextChanged;
        }

        // 输入字符验证
        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        // 拦截空格键
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        // 粘贴内容验证
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                if (!Regex.IsMatch(text, @"^\d+$"))
                    e.CancelCommand(); // 阻止非数字粘贴
            }
            else
            {
                e.CancelCommand();
            }
        }

        // 实时验证并提示错误
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (string.IsNullOrEmpty(textBox.Text)) return;

            if (!int.TryParse(textBox.Text, out int value) || value <= 0)
            {
                // 输入错误时显示红色边框
                textBox.BorderBrush = Brushes.Red;
                ToolTipService.SetToolTip(textBox, "请输入正整数！");
            }
            else
            {
                textBox.ClearValue(BorderBrushProperty);
                ToolTipService.SetToolTip(textBox, null);
            }
        }
    }
}
