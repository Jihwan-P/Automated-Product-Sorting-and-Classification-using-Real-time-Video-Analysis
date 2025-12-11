using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;

namespace last_project
{
    public partial class second : Form
    {
        private WpfLogin wpfLoginControl = null!;

        public second()
        {
            InitializeComponent();
            InitializeWpfLogin();
        }

        private void InitializeWpfLogin()
        {
            this.Text = "Login System";
            this.Size = new Size(460, 640);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Controls.Clear();

            ElementHost host = new ElementHost();
            host.Dock = DockStyle.Fill;
            wpfLoginControl = new WpfLogin();
            wpfLoginControl.LoginClicked += WpfLoginControl_LoginClicked;
            wpfLoginControl.RegisterClicked += WpfLoginControl_RegisterClicked;
            host.Child = wpfLoginControl;
            this.Controls.Add(host);
        }

        // [수정됨] 로그인 처리 함수 (프로필 사진 정보 받기 추가)
        private async void WpfLoginControl_LoginClicked(object? sender, EventArgs e)
        {
            string inputId = wpfLoginControl.UserId;
            string inputPw = wpfLoginControl.UserPw;

            var loginData = new { id = inputId, pw = inputPw };
            string jsonString = JsonConvert.SerializeObject(loginData);
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            string url = "http://127.0.0.1:5000/api/login";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        try
                        {
                            JObject result = JObject.Parse(responseJson);
                            JObject? userInfo = result["userInfo"] as JObject;

                            // 서버에서 받은 사용자 정보 파싱
                            string name = userInfo?["name"]?.ToString() ?? "관리자";
                            string nickname = userInfo?["nickname"]?.ToString() ?? "Admin";
                            string role = userInfo?["role"]?.ToString() ?? "ADMIN";
                            string email = userInfo?["email"]?.ToString() ?? string.Empty;
                            string phone = userInfo?["phone"]?.ToString() ?? string.Empty;
                            string birthdate = userInfo?["birthdate"]?.ToString() ?? string.Empty;

                            // ★ [핵심] 프로필 이미지 경로 받기
                            string profileImg = userInfo?["profile_image"]?.ToString() ?? string.Empty;

                            // Session에 저장 (이미지 경로 포함)
                            Session.SetUser(inputId, name, nickname, role, email, phone, birthdate, profileImg);
                        }
                        catch
                        {
                            // 예외 시 기본값
                            Session.SetUser(inputId, "관리자(오류)", "Admin", "ADMIN", "", "", "", "");
                        }

                        MessageBox.Show($"환영합니다, {Session.UserName}님!", "로그인 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Hide();

                        var f = new main();
                        f.StartPosition = FormStartPosition.CenterScreen;
                        f.FormClosed += (s, args) => this.Close();
                        f.Show();
                    }
                    else
                    {
                        MessageBox.Show("아이디 또는 비밀번호를 확인해주세요.", "로그인 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"서버 연결 오류: {ex.Message}", "오류");
            }
        }

        private void WpfLoginControl_RegisterClicked(object? sender, EventArgs e)
        {
            OpenRegisterForm();
        }

        private void OpenRegisterForm()
        {
            Form registerForm = new Form();
            registerForm.Text = "회원가입";
            registerForm.Size = new Size(465, 690);
            registerForm.StartPosition = FormStartPosition.CenterParent;
            registerForm.BackColor = Color.FromArgb(30, 30, 30);
            registerForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            registerForm.MaximizeBox = false;

            ElementHost host = new ElementHost();
            host.Dock = DockStyle.Fill;
            WpfRegistration wpfReg = new WpfRegistration();

            wpfReg.CancelClicked += (s, args) => registerForm.Close();

            wpfReg.CheckDuplicateClicked += async (s, args) =>
            {
                string userId = wpfReg.UserId;
                string url = $"http://127.0.0.1:5000/api/check_id?id={userId}";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode) MessageBox.Show("사용 가능한 아이디입니다.");
                        else MessageBox.Show("이미 사용 중인 아이디입니다.");
                    }
                }
                catch (Exception ex) { MessageBox.Show($"오류: {ex.Message}"); }
            };

            wpfReg.RegisterClicked += async (s, args) =>
            {
                var joinData = new
                {
                    id = wpfReg.UserId,
                    pw = wpfReg.UserPw,
                    name = wpfReg.UserName,
                    nickname = wpfReg.UserNickname,
                    role = wpfReg.UserRole,
                    phone = wpfReg.UserPhone,
                    email = wpfReg.UserEmail,
                    birthdate = wpfReg.UserBirthdate
                };

                string json = JsonConvert.SerializeObject(joinData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                string url = "http://127.0.0.1:5000/api/register";

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.PostAsync(url, content);
                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("회원가입 완료!");
                            registerForm.Close();
                        }
                        else MessageBox.Show("회원가입 실패");
                    }
                }
                catch (Exception ex) { MessageBox.Show($"오류: {ex.Message}"); }
            };

            host.Child = wpfReg;
            registerForm.Controls.Add(host);
            registerForm.ShowDialog(this);
        }

        // 레거시 코드 (삭제하지 않음)
        private void btnLogin_Click(object? sender, EventArgs e) { }
        private void btnRegister_Click(object? sender, EventArgs e) { }
        private void txtId_TextChanged(object? sender, EventArgs e) { }
        private void pictureBox1_Click(object? sender, EventArgs e) { }
    }
}
