using System;
using System.Windows;
using System.Windows.Controls;

namespace last_project
{
    /// <summary>
    /// WpfSlotInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WpfSlotInfo : System.Windows.Controls.UserControl
    {
        // --- 1. main.cs로 "신호"를 보낼 이벤트 정의 ---
        public event EventHandler? SaveButtonClicked;
        public event EventHandler? DeleteButtonClicked;

        // --- 2. main.cs가 좌표를 '읽고/쓸' 수 있도록 '속성' 정의 ---
        // (XAML의 텍스트박스(예: TxtSlotX)와 C# 속성(예: SlotX)을 연결합니다)

        public string SlotId
        {
            get { return TxtSlotId.Text; }
            set { TxtSlotId.Text = value; }
        }
        public string SlotX
        {
            get { return TxtSlotX.Text; }
            set { TxtSlotX.Text = value; }
        }
        public string SlotY
        {
            get { return TxtSlotY.Text; }
            set { TxtSlotY.Text = value; }
        }
        public string SlotW
        {
            get { return TxtSlotW.Text; }
            set { TxtSlotW.Text = value; }
        }
        public string SlotH
        {
            get { return TxtSlotH.Text; }
            set { TxtSlotH.Text = value; }
        }
        public bool IsSlotActive
        {
            get { return ChkIsActive.IsChecked ?? false; }
            set { ChkIsActive.IsChecked = value; }
        }

        public WpfSlotInfo()
        {
            InitializeComponent();
        }

        // --- 3. XAML의 "저장" 버튼 클릭 시, "신호" 발생 ---
        private void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            SaveButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        private void BtnDelete_Click(object? sender, RoutedEventArgs e)
        {
            // "삭제 버튼 눌렸음!" 하고 외부(setting.cs)로 신호를 보냅니다.
            DeleteButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
