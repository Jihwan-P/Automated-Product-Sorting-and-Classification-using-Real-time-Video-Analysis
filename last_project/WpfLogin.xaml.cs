using System;
using System.Windows;
using System.Windows.Controls;

namespace last_project
{
    // [수정] 'UserControl' 앞에 'System.Windows.Controls.'를 붙여서 모호함 해결
    public partial class WpfLogin : System.Windows.Controls.UserControl
    {
        // 부모(second.cs)에게 보낼 신호
        public event EventHandler? LoginClicked;
        public event EventHandler? RegisterClicked;

        // 입력값 속성
        public string UserId => TxtUserId.Text;
        public string UserPw => TxtUserPw.Password;

        public WpfLogin()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(UserPw))
            {
                // [수정] 메시지박스도 명확하게 지정
                System.Windows.MessageBox.Show("아이디와 비밀번호를 입력해주세요.");
                return;
            }
            // 로그인 신호 보내기
            LoginClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnGoRegister_Click(object? sender, RoutedEventArgs e)
        {
            // 회원가입 신호 보내기
            RegisterClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
