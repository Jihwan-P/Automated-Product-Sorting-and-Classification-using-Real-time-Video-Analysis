using System;
using System.IO.Ports;     // (필수) 아두이노 통신(SerialPort)을 위해
using System.Windows;      // (필수) Window, RoutedEventArgs 등을 사용하기 위해
using System.Windows.Controls; // (필수) Slider, TextBlock 등을 사용하기 위해
using System.Windows.Input;  // (필수) MouseButtonEventArgs 등을 사용하기 위해
// (중요) setting.xaml의 x:Class="last_project.SettingWindow"와 일치해야 합니다.
namespace last_project
{
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>

    // (중요) XAML의 <Window ...> 태그 및 x:Class와 일치해야 합니다.
    public partial class SettingWindow : Window
    {
        // 아두이노와 통신할 시리얼 포트 객체를 선언합니다.
        SerialPort arduinoPort = new SerialPort();

        public SettingWindow()
        {
            // (필수!) XAML 디자인과 C# 코드를 연결하는 함수
            InitializeComponent();
        }

        // ========== XAML에서 연결한 이벤트 핸들러들 ==========

        // --- 차량 이동 ---
        private void ForwardButton_MouseDown(object? sender, MouseButtonEventArgs e)
        {
            SendSerialCommand("FORWARD");
            AddLog("전진 명령 전송");
        }

        private void ForwardButton_MouseUp(object? sender, MouseButtonEventArgs e)
        {
            // 마우스 버튼을 떼면 정지
            SendSerialCommand("STOP");
            AddLog("정지 (전진 버튼 뗌)");
        }

        private void BackwardButton_MouseDown(object? sender, MouseButtonEventArgs e)
        {
            SendSerialCommand("BACKWARD");
            AddLog("후진 명령 전송");
        }

        private void BackwardButton_MouseUp(object? sender, MouseButtonEventArgs e)
        {
            // 마우스 버튼을 떼면 정지
            SendSerialCommand("STOP");
            AddLog("정지 (후진 버튼 뗌)");
        }

        private void StopButton_Click(object? sender, RoutedEventArgs e)
        {
            SendSerialCommand("STOP");
            AddLog("정지 명령 전송 (수동)");
        }

        // --- 속도 조절 ---
        private void SpeedSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // (중요) SpeedValueText가 null이 아닐 때만 실행 (프로그램 시작 시 오류 방지)
            if (SpeedValueText != null)
            {
                // 슬라이더의 현재 값을 정수(int)로 변환
                int newSpeed = (int)e.NewValue;

                // XAML의 TextBlock(x:Name="SpeedValueText") 값을 변경
                SpeedValueText.Text = newSpeed.ToString();

                // (옵션) 슬라이더를 '조작 중일 때'는 명령을 보내지 않고,
                // '마우스를 뗐을 때'만 보내고 싶다면 이 코드를 다른 이벤트(예: Thumb.DragCompleted)로 옮겨야 합니다.
                // 지금은 값이 바뀔 때마다 즉시 전송됩니다.
                SendSerialCommand($"SPEED,{newSpeed}");
                AddLog($"속도 {newSpeed}로 변경");
            }
        }

        // --- 보조 기능 ---
        private void HonkButton_Click(object? sender, RoutedEventArgs e)
        {
            SendSerialCommand("HONK");
            AddLog("경적 울리기 명령 전송");
        }


        // ========== 로그 및 통신을 위한 헬퍼(Helper) 함수 ==========

        /**
         * @name AddLog
         * @brief LogTextBox(x:Name="LogTextBox")에 시간과 함께 로그를 추가합니다.
         */
        private void AddLog(string message)
        {
            // [14:05:01] 메시지
            string logEntry = $"[{DateTime.Now.ToString("HH:mm:ss")}] {message}\n";

            // (중요) UI 스레드에서 UI 컨트롤(LogTextBox)을 안전하게 업데이트합니다.
            // (지금은 같은 스레드에서 실행되지만, 나중에 통신 스레드가 분리되면 Dispatcher가 필요할 수 있습니다.)
            LogTextBox.AppendText(logEntry);
            LogTextBox.ScrollToEnd(); // 스크롤을 항상 맨 아래로
        }

        /**
         * @name SendSerialCommand
         * @brief 아두이노로 시리얼 명령을 전송하는 함수
         */
        private void SendSerialCommand(string command)
        {
            // (나중에 이 부분에 실제 SerialPort 전송 코드를 넣습니다)
            // 1. 포트가 실제로 열려있는지 확인
            if (arduinoPort != null && arduinoPort.IsOpen)
            {
                try
                {
                    // 2. 아두이노로 명령어를 문자열(라인)으로 전송
                    arduinoPort.WriteLine(command);
                }
                catch (Exception ex)
                {
                    // 3. 전송 중 오류가 나면 로그 남기기
                    AddLog($"전송 오류: {ex.Message}");
                }
            }
            else
            {
                // 4. 연결이 안 되어 있으면 로그 남기기
                // (이 로그가 너무 자주 찍히면 AddLog($"오류: 아두이노 연결 안됨") 대신
                //  상태표시줄 같은 곳에 한 번만 표시하는 것이 좋습니다.)
                AddLog("오류: 아두이노가 연결되지 않았습니다.");
            }

            // (디버깅용) Visual Studio의 '출력' 창에도 로그를 찍습니다.
            System.Diagnostics.Debug.WriteLine($"[Serial 전송 시도] {command}");
        }
    }
}
