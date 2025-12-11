using System;
using System.Collections.Generic; // List 사용을 위해 추가
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; // 필터링(LINQ)을 위해 추가
using System.Runtime.CompilerServices;

namespace last_project
{
    public class PictureLogViewModel : INotifyPropertyChanged
    {
        // 1. (원본 저장소) 모든 로그를 담고 있는 리스트 (화면엔 안 보임)
        private List<PictureLogEntry> _allLogs;

        // 2. (화면 표시용) 실제 UI(ListBox)와 연결될 컬렉션
        public ObservableCollection<PictureLogEntry> FilteredLogs { get; set; }

        public PictureLogViewModel()
        {
            _allLogs = new List<PictureLogEntry>();
            FilteredLogs = new ObservableCollection<PictureLogEntry>();
        }

        // 로그 추가 함수
        public void AddLog(string imagePath, string description, DateTime? customTime = null)
        {
            var newEntry = new PictureLogEntry
            {
                ImagePath = imagePath,
                Description = description,
                // customTime(입력한 시간)이 있으면 그걸 쓰고, 없으면 현재 시간(DateTime.Now)을 씀
                Timestamp = customTime ?? DateTime.Now
            };

            _allLogs.Insert(0, newEntry);
            FilteredLogs.Insert(0, newEntry);
        }
        // ★ [핵심 기능] 날짜와 시간대로 필터링하는 함수
        public void SearchLogs(DateTime selectedDate, int startHour, int endHour)
        {
            // 1. LINQ를 사용하여 조건에 맞는 데이터만 추출
            var result = _allLogs.Where(log =>
                log.Timestamp.Date == selectedDate.Date && // 날짜가 같고
                log.Timestamp.Hour >= startHour &&         // 시작 시간보다 크거나 같고
                log.Timestamp.Hour <= endHour              // 종료 시간보다 작거나 같은 것
            ).OrderByDescending(log => log.Timestamp);     // 최신순 정렬

            // 2. 화면 리스트 초기화 후 결과만 다시 담기
            FilteredLogs.Clear();
            foreach (var item in result)
            {
                FilteredLogs.Add(item);
            }
        }

        // [초기화] 모든 로그 다시 보여주기
        public void ResetFilter()
        {
            FilteredLogs.Clear();
            foreach (var item in _allLogs)
            {
                FilteredLogs.Add(item);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
