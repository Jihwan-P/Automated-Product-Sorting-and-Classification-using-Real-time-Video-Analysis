using System;
using System.Windows;
using System.Windows.Controls; // <-- (WPF의 UserControl, Button 등을 위해 필수!)

namespace last_project
{
    /// <summary>
    /// WpfButtonMenu.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WpfButtonMenu : System.Windows.Controls.UserControl // <-- 명시적으로 WPF UserControl 사용
    {
        // --- 1. WinForms(main.cs)에 "신호"를 보낼 4개의 이벤트를 정의합니다 ---
        public event EventHandler? BaljuButtonClicked;
        public event EventHandler? TonggyeButtonClicked;
        public event EventHandler? LogButtonClicked;
        public event EventHandler? SettingButtonClicked;
        public event EventHandler? AppLogButtonClicked;
        public WpfButtonMenu()
        {
            InitializeComponent();
        }

        // --- 2. XAML에서 연결한 'Click' 이벤트가 이 함수들을 호출합니다 ---

        private void BtnBalju_Click(object? sender, RoutedEventArgs e)
        {
            BaljuButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnTonggye_Click(object? sender, RoutedEventArgs e)
        {
            TonggyeButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnLog_Click(object? sender, RoutedEventArgs e)
        {
            LogButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnSetting_Click(object? sender, RoutedEventArgs e)
        {
            SettingButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        // ▼▼▼ [추가!] XAML에서 연결한 "BtnAppLog_Click" 함수입니다 ▼▼▼
        private void BtnAppLog_Click(object? sender, RoutedEventArgs e)
        {
            // "AppLog 버튼 눌렸다!" 라고 main.cs에 신호를 보냅니다.
            AppLogButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
