﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Panuon.UI.Silver;
using SmartTuningSystem.Utils;

namespace SmartTuningSystem.View.Windows
{
    /// <summary>
    /// IconSelector.xaml 的交互逻辑
    /// </summary>
    public partial class IconSelectorDialog : WindowX
    {
        public class IconSelectorModel
        {
            public string SelectedIcon { get; set; }
            public string SelectedText { get; set; }
        }

        public IconSelectorModel SelectorModel;

        public IconSelectorDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SearchICON();
        }

        private void SearchICON()
        {
            uicons.Children.Clear();
            List<string> keys = FontAwesomeCommon.TypeDict.Keys.Where(c => c.Contains(txtKey.Text)).ToList();
            foreach (var key in keys)
            {
                var _val = FontAwesomeCommon.TypeDict[key];
                string iconStr = FontAwesomeCommon.GetUnicode(_val);

                Border border = new Border();
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = new SolidColorBrush(Colors.LightGray);
                border.Margin = new Thickness(5);
                border.MouseDown += Border_MouseDown;

                StackPanel stackPanel = new StackPanel();
                stackPanel.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Center;

                Label iconLabel = new Label();
                iconLabel.Content = iconStr;
                iconLabel.FontFamily = (FontFamily)FindResource("FontAwesome");
                iconLabel.FontSize = 25;
                iconLabel.HorizontalAlignment = HorizontalAlignment.Center;

                Label textLabel = new Label();
                textLabel.Content = key;

                stackPanel.Children.Add(iconLabel);
                stackPanel.Children.Add(textLabel);

                border.Tag = new IconSelectorModel()
                {
                    SelectedIcon = iconStr,
                    SelectedText = key
                };
                border.Child = stackPanel;
                uicons.Children.Add(border);
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectorModel = (sender as Border).Tag as IconSelectorModel;
            DialogResult = true;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchICON();
            }
        }
    }
}
