using System;
using System.Windows;
using System.Windows.Controls;

namespace last_project
{
    /// <summary>
    /// WpfLogoutControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WpfLogoutControl : System.Windows.Controls.UserControl
    {
        // 1. setting.cs로 "신호"를 보낼 이벤트를 정의합니다.
        public event EventHandler? LogoutClicked;

        public WpfLogoutControl()
        {
            InitializeComponent();
        }

        // 2. XAML의 버튼 클릭 시, "신호"를 발생시킵니다.
        private void BtnLogout_Click(object? sender, RoutedEventArgs e)
        {
            // "로그아웃 버튼 눌렸다!" 라고 외부에 신호를 보냅니다.
            LogoutClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
