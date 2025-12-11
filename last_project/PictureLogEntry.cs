using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace last_project
{
    /// <summary>
    /// 사진 로그 1개 항목을 나타내는 데이터 모델 클래스입니다.
    /// INotifyPropertyChanged는 데이터 변경 시 UI를 자동으로 업데이트하기 위해 필요합니다.
    /// </summary>
    public class PictureLogEntry : INotifyPropertyChanged
    {
        private string _imagePath = string.Empty;
        private DateTime _timestamp;
        private string _description = string.Empty;

        /// <summary>
        /// (필수) 캡처된 이미지의 파일 경로 (예: "C:\\Logs\\capture_001.jpg")
        /// </summary>
        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// (필수) 캡처된 시간
        /// </summary>
        public DateTime Timestamp
        {
            get => _timestamp;
            set { _timestamp = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// (선택) 로그 설명 (예: "A-3 슬롯 수납 완료")
        /// </summary>
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        // --- INotifyPropertyChanged 구현 ---
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
