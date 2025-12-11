using System;
using System.Windows;
using System.Windows.Controls;

namespace last_project
{
    public partial class WpfRegistration : System.Windows.Controls.UserControl
    {
        // 부모 폼(WinForms)으로 보낼 신호들
        public event EventHandler? RegisterClicked;
        public event EventHandler? CancelClicked;
        public event EventHandler? CheckDuplicateClicked;

        // --- [기존] 입력값 속성 ---
        public string UserId => TxtId?.Text ?? string.Empty;
        public string UserPw => TxtPw?.Password ?? string.Empty;
        public string UserPwConfirm => TxtPwConfirm?.Password ?? string.Empty;

        // --- [추가] 새로 만든 입력칸들의 값을 가져오는 속성들 ---
        public string UserName => TxtName?.Text ?? string.Empty;          // 이름
        public string UserNickname => TxtNickname?.Text ?? string.Empty;  // 닉네임
        public string UserBirthdate => TxtBirthdate?.Text ?? string.Empty;// 생년월일
        public string UserPhone => TxtPhone?.Text ?? string.Empty;        // 연락처
        public string UserEmail => TxtEmail?.Text ?? string.Empty;        // 이메일

        // 직급 (콤보박스에서 선택된 항목의 텍스트 앞부분만 가져옴, 예: "STAFF")
        public string UserRole
        {
            get
            {
                if (CmbRole.SelectedItem is ComboBoxItem item)
                {
                    // "STAFF (일반 사원)" 같은 문자열에서 공백 앞부분만 잘라서 반환
                    if (item.Content != null)
                    {
                        string content = item.Content.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(content))
                            return content.Split(' ')[0];
                    }
                }
                return "STAFF"; // 기본값
            }
        }

        public WpfRegistration()
        {
            InitializeComponent();
        }

        // 중복 확인 버튼
        private void BtnCheckDup_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                System.Windows.MessageBox.Show("아이디를 입력해주세요.");
                return;
            }
            CheckDuplicateClicked?.Invoke(this, EventArgs.Empty);
        }

        // [수정됨] 회원가입 완료 버튼
        private void BtnRegister_Click(object? sender, RoutedEventArgs e)
        {
            // 1. 필수 입력값 검사
            if (string.IsNullOrWhiteSpace(UserId) ||
                string.IsNullOrWhiteSpace(UserPw) ||
                string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(UserNickname))
            {
                System.Windows.MessageBox.Show("필수 정보(아이디, 비번, 이름, 닉네임)를 모두 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. 비밀번호 일치 여부 확인
            if (UserPw != UserPwConfirm)
            {
                System.Windows.MessageBox.Show("비밀번호가 일치하지 않습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. 부모 폼(second.cs)에 신호 전송
            RegisterClicked?.Invoke(this, EventArgs.Empty);
        }

        // 취소 버튼
        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
