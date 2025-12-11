using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace last_project // (네임스페이스는 실제 프로젝트와 동일해야 합니다)
{
    public partial class LogForm : Form
    {
        public LogForm()
        {
            InitializeComponent();

            // ▼▼▼ [추가!] 폼 Load 이벤트를 C# 코드로 직접 연결합니다 ▼▼▼
            this.Load += LogForm_Load;
        }

        /// <summary>
        /// 폼이 "처음 켜질 때" 딱 한 번 실행되는 함수입니다.
        /// </summary>
        private void LogForm_Load(object? sender, EventArgs e)
        {
            try
            {
                // 1. LogManager(로그 수집기)에서 지금까지 쌓인 모든 로그를 가져옵니다.
                List<string> allLogs = LogManager.GetLogs();

                // 2. 텍스트박스(txtLogs)에 모든 로그를 한 번에 넣습니다.
                //    (Environment.NewLine = 윈도우의 표준 줄바꿈 문자 "\r\n")
                //    (만약 txtLogs 컨트롤이 null이면 디자이너에서 Name 속성을 확인하세요)
                if (txtLogs != null)
                {
                    txtLogs.Text = string.Join(Environment.NewLine, allLogs);

                    // 3. (중요!) 텍스트박스의 스크롤을 맨 아래 (가장 최신 로그)로 이동시킵니다.
                    txtLogs.SelectionStart = txtLogs.Text.Length;
                    txtLogs.ScrollToCaret();
                }
                else
                {
                    MessageBox.Show("디자인 오류: txtLogs 컨트롤을 찾을 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"로그 폼 로드 중 오류 발생: {ex.Message}");
            }
        }
    }
}
