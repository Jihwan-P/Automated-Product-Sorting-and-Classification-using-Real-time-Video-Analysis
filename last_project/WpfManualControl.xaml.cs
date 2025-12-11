using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace last_project
{
    public partial class WpfManualControl : System.Windows.Controls.UserControl
    {
        // =========================================================
        // [설정 1] 차량 제어용 ESP-01 (기존 유지)
        // =========================================================
        private const string ESP01_IP = "192.168.0.7";
        private const string ESP01_PORT = "80";

        // =========================================================
        // [설정 2] 카메라 주소 변경 (WebRTC -> CAM3 MJPEG)
        // =========================================================
        private const string CAM4_URL = "http://192.168.0.97:8000/stream.mjpg";

        private readonly HttpClient client = new HttpClient();

        public WpfManualControl()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                client.Timeout = TimeSpan.FromSeconds(3);
                InitializeCameraAsync();
            }
        }

        // =========================================================
        // [설정 3] 카메라 초기화 로직 변경 (Navigate -> NavigateToString)
        // =========================================================
        private async void InitializeCameraAsync()
        {
            try
            {
                // 1. WebView2 컨트롤 초기화 대기
                await CameraWebView.EnsureCoreWebView2Async(null);

                // 2. 화면에 꽉 차게 보여주는 HTML 생성 (main.cs의 방식 응용)
                // 검은 배경에 이미지를 꽉 채우도록(object-fit: fill) 설정
                string htmlContent = $@"
                    <html>
                    <head>
                        <style>
                            body {{ 
                                margin: 0; 
                                padding: 0; 
                                background-color: black; 
                                overflow: hidden; 
                                display: flex;
                                justify-content: center;
                                align-items: center;
                                height: 100vh;
                            }}
                            img {{ 
                                width: 100%; 
                                height: 100%; 
                                object-fit: fill; 
                            }}
                        </style>
                    </head>
                    <body>
                        <img src='{CAM4_URL}' onerror=""this.style.display='none'; document.body.innerHTML='<h2 style=\'color:white\'>CAM3 연결 실패</h2>'"">
                    </body>
                    </html>";

                // 3. 생성한 HTML을 로드하여 스트리밍 시작
                CameraWebView.CoreWebView2.NavigateToString(htmlContent);

                AddLog($"[Camera] CAM3 연결 시도: {CAM4_URL}");
            }
            catch (Exception ex)
            {
                AddLog($"[Error] 카메라 초기화 실패: {ex.Message}");
            }
        }

        private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // NavigateToString 방식을 사용하므로 별도의 스크립트 주입은 필요 없으나, 
            // 이벤트 핸들러 구조 유지를 위해 남겨둡니다.
        }

        // =========================================================
        //  [2] 차량 제어 버튼 이벤트 (기존 코드 유지)
        // =========================================================

        // ▲ 전진 (PreviewMouseDown 사용)
        private async void ForwardButton_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
        {
            AddLog("▲ 전진 버튼 눌림");
            await SendCommandToCar("F");
        }

        // ▲ 전진 뗌 (PreviewMouseUp 사용)
        private async void ForwardButton_PreviewMouseUp(object? sender, MouseButtonEventArgs e)
        {
            await SendCommandToCar("S");
            AddLog("■ 정지 (버튼 뗌)");
        }

        // ▼ 후진 (PreviewMouseDown 사용)
        private async void BackwardButton_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
        {
            AddLog("▼ 후진 버튼 눌림");
            await SendCommandToCar("B");
        }

        // ▼ 후진 뗌 (PreviewMouseUp 사용)
        private async void BackwardButton_PreviewMouseUp(object? sender, MouseButtonEventArgs e)
        {
            await SendCommandToCar("S");
            AddLog("■ 정지 (버튼 뗌)");
        }

        // ■ 정지 (클릭)
        private async void StopButton_Click(object? sender, RoutedEventArgs e)
        {
            await SendCommandToCar("S");
            AddLog("■ 강제 정지 명령 전송");
        }

        // 📢 경적 (클릭)
        private async void HonkButton_Click(object? sender, RoutedEventArgs e)
        {
            await SendCommandToCar("H");
            AddLog("📢 빵빵!");
        }

        // =========================================================
        //  [3] 로그 및 기타 기능 (기존 코드 유지)
        // =========================================================

        private void BtnExpandLog_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                LogForm logForm = new LogForm();
                logForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                logForm.Show();
                AddLog("[System] 로그 상세 창 열기");
            }
            catch (Exception ex) { AddLog($"[Error] 로그 창 열기 실패: {ex.Message}"); }
        }

        // 로그 출력 헬퍼 함수
        private void AddLog(string message)
        {
            // 1. 전역 로그 매니저에 저장
            last_project.LogManager.Add(message);

            // 2. 현재 화면의 로그창에 표시
            if (LogTextBox == null) return;

            Dispatcher.Invoke(() =>
            {
                string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}\r\n";
                LogTextBox.AppendText(logEntry);
                LogTextBox.ScrollToEnd();
            });
        }

        private async Task SendCommandToCar(string command)
        {
            if (client == null) return;

            string url = $"http://{ESP01_IP}:{ESP01_PORT}/?cmd={command}&speed=200";
            AddLog($"[통신] '{command}' 명령 전송 중...");

            try
            {
                using (var cts = new System.Threading.CancellationTokenSource(500))
                {
                    await client.GetAsync(url, cts.Token);
                }
            }
            catch (Exception ex)
            {
                AddLog($"[오류] 통신 실패: {ex.Message}");
            }
        }
    }
}
