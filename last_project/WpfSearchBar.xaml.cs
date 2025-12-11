using System;
using System.Windows;
using System.Windows.Controls; // (UserControl을 위해 필수)

namespace last_project
{
    /// <summary>
    /// WpfSearchBar.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WpfSearchBar : System.Windows.Controls.UserControl
    {
        // --- 1. main.cs로 "신호"를 보낼 이벤트 정의 ---
        public event EventHandler? SearchButtonClicked;
        public event EventHandler? RefreshButtonClicked;

        // --- 2. main.cs가 검색어를 읽어갈 수 있도록 '속성' 정의 ---
        public string SearchTerm
        {
            get { return TxtSearch.Text; }
            set { TxtSearch.Text = value; }
        }

        public WpfSearchBar()
        {
            InitializeComponent();
        }

        // --- 3. XAML의 버튼 클릭 시, "신호" 발생 ---
        private void BtnSearch_Click(object? sender, RoutedEventArgs e)
        {
            SearchButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void BtnRefresh_Click(object? sender, RoutedEventArgs e)
        {
            RefreshButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
