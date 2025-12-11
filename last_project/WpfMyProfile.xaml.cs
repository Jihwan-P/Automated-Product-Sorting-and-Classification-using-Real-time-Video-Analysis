using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace last_project
{
    public partial class WpfMyProfile : System.Windows.Controls.UserControl
    {
        private DispatcherTimer timer = null!;
        private static readonly HttpClient client = new HttpClient();
        private const string API_BASE = "http://127.0.0.1:5000/api";
        public WpfMyProfile()
        {
            InitializeComponent();
            LoadSessionData();
            StartTimer();   
        }


        // WpfMyProfile.xaml.cs 파일 안에 추가하세요
        private void ProfileImage_Click(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 세션에서 이미지 경로 가져오기
            string imagePath = Session.ProfileImagePath;

            if (string.IsNullOrEmpty(imagePath)) return;

            try
            {
                // 큰 화면 뷰어 띄우기
                PictureViewerWindow viewer = new PictureViewerWindow(imagePath);
                viewer.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("이미지 로드 오류: " + ex.Message);
            }
        }
        private void LoadSessionData()
        {
            if (TxtBigName != null) TxtBigName.Text = Session.UserName;
            if (TxtBigRole != null) TxtBigRole.Text = $"ROLE: {Session.Role}";
            if (TxtLastLogin != null) TxtLastLogin.Text = Session.LoginTime.ToString("yyyy-MM-dd HH:mm");

            if (InId != null) InId.Text = Session.UserId;
            if (InName != null) InName.Text = Session.UserName;
            if (InNickname != null) InNickname.Text = Session.Nickname;
            if (InBirth != null) InBirth.Text = Session.Birthdate;
            if (InEmail != null) InEmail.Text = Session.Email;
            if (InPhone != null) InPhone.Text = Session.Phone;

            // [수정됨] 로그인 시 받아온 프로필 사진 URL이 있으면 로드
            if (!string.IsNullOrEmpty(Session.ProfileImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(Session.ProfileImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ProfileImgBrush.ImageSource = bitmap;
                    TxtNoImg.Visibility = Visibility.Collapsed;
                }
                catch { }
            }
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                if (TxtNowTime != null) TxtNowTime.Text = DateTime.Now.ToString("HH:mm:ss");
                TimeSpan elapsed = DateTime.Now - Session.LoginTime;
                if (TxtElapsedTime != null)
                    TxtElapsedTime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)elapsed.TotalHours, elapsed.Minutes, elapsed.Seconds);
            };
            timer.Start();
        }

        // [기능 1] 프로필 사진 업로드 및 저장
        private async void BtnUploadPhoto_Click(object? sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.png;*.jpeg";

            if (dlg.ShowDialog() == true)
            {
                string filePath = dlg.FileName;
                try
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        var fileBytes = File.ReadAllBytes(filePath);
                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                        content.Add(fileContent, "file", Path.GetFileName(filePath));
                        content.Add(new StringContent(Session.UserId), "user_id");

                        var response = await client.PostAsync($"{API_BASE}/user/upload_image", content);

                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            JObject result = JObject.Parse(json);

                            if (result["success"]?.Value<bool>() == true)
                            {
                                string? newUrl = result["url"]?.ToString();
                                if (string.IsNullOrWhiteSpace(newUrl))
                                {
                                    System.Windows.MessageBox.Show("이미지 URL을 가져오지 못했습니다.");
                                    return;
                                }

                                Session.ProfileImagePath = newUrl; // 세션 갱신

                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(newUrl, UriKind.Absolute);
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();

                                ProfileImgBrush.ImageSource = bitmap;
                                TxtNoImg.Visibility = Visibility.Collapsed;
                                System.Windows.MessageBox.Show("프로필 사진이 변경되었습니다.");
                            }
                        }
                        else System.Windows.MessageBox.Show("업로드 실패");
                    }
                }
                catch (Exception ex) { System.Windows.MessageBox.Show("오류: " + ex.Message); }
            }
        }

        // [기능 2] 정보 수정 (비밀번호 포함)
        private async void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            string? newPassword = null;

            // 비밀번호 변경 시도 시
            if (!string.IsNullOrEmpty(PwNew.Password))
            {
                // 현재 비밀번호 입력 확인
                if (string.IsNullOrEmpty(PwCurrent.Password))
                {
                    System.Windows.MessageBox.Show("비밀번호를 변경하려면 '현재 비밀번호'를 입력해야 합니다.");
                    return;
                }
                if (PwNew.Password != PwNewConfirm.Password)
                {
                    System.Windows.MessageBox.Show("새 비밀번호가 일치하지 않습니다.");
                    return;
                }
                newPassword = PwNew.Password;
            }

            var updateData = new
            {
                id = Session.UserId,
                name = InName.Text,
                nickname = InNickname.Text,
                email = InEmail.Text,
                phone = InPhone.Text,
                birthdate = InBirth.Text,
                new_password = newPassword
            };

            try
            {
                string json = JsonConvert.SerializeObject(updateData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{API_BASE}/user/update", content);

                if (response.IsSuccessStatusCode)
                {
                    Session.UserName = InName.Text;
                    Session.Nickname = InNickname.Text;
                    Session.Email = InEmail.Text;
                    Session.Phone = InPhone.Text;
                    Session.Birthdate = InBirth.Text;
                    TxtBigName.Text = Session.UserName;

                    System.Windows.MessageBox.Show("정보가 수정되었습니다.");

                    PwCurrent.Password = "";
                    PwNew.Password = "";
                    PwNewConfirm.Password = "";
                }
                else System.Windows.MessageBox.Show("수정 실패");
            }
            catch (Exception ex) { System.Windows.MessageBox.Show("오류: " + ex.Message); }
        }
    }
}
