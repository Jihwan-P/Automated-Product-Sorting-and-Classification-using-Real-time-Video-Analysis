using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace last_project
{
    public partial class WpfPictureLog : System.Windows.Controls.UserControl
    {
        public WpfPictureLog()
        {
            InitializeComponent();
            InitializeSearchOptions();
        }

        // 초기 설정: 콤보박스 채우기 및 오늘 날짜 설정
        private void InitializeSearchOptions()
        {
            // 1. 날짜는 오늘로 기본 설정
            DtPicker.SelectedDate = DateTime.Today;

            // 2. 시간 콤보박스 (0시 ~ 23시)
            for (int i = 0; i <= 23; i++)
            {
                string hour = i.ToString("D2"); // "00", "01" ...
                CmbStartHour.Items.Add(hour);
                CmbEndHour.Items.Add(hour);
            }

            // 기본값: 00시 ~ 23시 (하루 전체)
            CmbStartHour.SelectedIndex = 0;
            CmbEndHour.SelectedIndex = 23;
        }

        // [검색] 버튼 클릭
        private void BtnSearch_Click(object? sender, RoutedEventArgs e)
        {
            // 1. ViewModel 가져오기
            var vm = this.DataContext as PictureLogViewModel;
            if (vm == null) return;

            // 2. 날짜 확인
            if (DtPicker.SelectedDate == null)
            {
                System.Windows.MessageBox.Show("날짜를 선택해주세요.");
                return;
            }
            DateTime selectedDate = DtPicker.SelectedDate.Value;

            // 3. 시간 확인
            if (!int.TryParse(CmbStartHour.SelectedItem?.ToString(), out int start) ||
                !int.TryParse(CmbEndHour.SelectedItem?.ToString(), out int end))
            {
                System.Windows.MessageBox.Show("시간 범위를 다시 선택해주세요.");
                return;
            }

            if (start > end)
            {
                System.Windows.MessageBox.Show("시작 시간이 종료 시간보다 늦을 수 없습니다.");
                return;
            }

            // 4. ViewModel에 필터링 요청
            vm.SearchLogs(selectedDate, start, end);
        }

        // [전체] 버튼 클릭 (초기화)
        private void BtnReset_Click(object? sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as PictureLogViewModel;
            if (vm == null) return;

            vm.ResetFilter();

            // UI도 초기화
            DtPicker.SelectedDate = DateTime.Today;
            CmbStartHour.SelectedIndex = 0;
            CmbEndHour.SelectedIndex = 23;
        }

        // 이미지 클릭 (확대 보기) - 기존 코드 유지
        private void SmallImage_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            var imageControl = sender as FrameworkElement;
            if (imageControl == null) return;

            var logEntry = imageControl.DataContext as PictureLogEntry;
            if (logEntry == null) return;

            if (string.IsNullOrEmpty(logEntry.ImagePath)) return;

            PictureViewerWindow viewer = new PictureViewerWindow(logEntry.ImagePath);
            viewer.ShowDialog();
        }
    }
}
