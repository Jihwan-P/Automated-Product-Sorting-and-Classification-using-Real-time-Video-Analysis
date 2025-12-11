using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json; // [필수] 이 부분이 없으면 추가하세요!

namespace last_project
{
    // 데이터 변경 시 UI에 즉시 반영하기 위해 INotifyPropertyChanged 구현
    public class OrderModel : INotifyPropertyChanged
    {
        private string _status = string.Empty;

        // ▼▼▼ [핵심 수정] 서버(DB)의 이름표와 짝을 맞춰줍니다 ▼▼▼

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; } = string.Empty;     // 발주처

        [JsonProperty("item_name")]             // ★ 서버는 item_name으로 보냄 -> C#은 ItemName으로 받음
        public string ItemName { get; set; } = string.Empty;    // 품목명

        [JsonProperty("quantity")]
        public int Quantity { get; set; }       // 수량

        [JsonProperty("order_date")]
        public string OrderDate { get; set; } = string.Empty;   // 발주 일자

        [JsonProperty("due_date")]
        public string DueDate { get; set; } = string.Empty;     // 납기일

        [JsonProperty("contact")]
        public string Contact { get; set; } = string.Empty;     // 연락처

        [JsonProperty("price")]
        public int Price { get; set; }          // 단가

        [JsonProperty("note")]
        public string Note { get; set; } = string.Empty;        // 비고dgh

        [JsonProperty("status")]
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // 총액 (단가 * 수량) - DB에 없어도 C#에서 계산해서 보여줌
        public int TotalPrice => Price * Quantity;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
