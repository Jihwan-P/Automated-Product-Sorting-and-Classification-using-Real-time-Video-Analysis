using System;
using System.Windows;
using System.Windows.Controls;

namespace last_project
{
    /// <summary>
    /// WpfProductAdmin.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WpfProductAdmin : System.Windows.Controls.UserControl
    {
        // --- 1. main.cs로 "신호"를 보낼 4개의 이벤트를 정의 ---
        public event EventHandler? RegisterClicked;
        public event EventHandler? UpdateClicked; // "수정"
        public event EventHandler? DeleteClicked;
        public event EventHandler? RefreshClicked;

        // --- 2. main.cs가 텍스트박스 값을 '읽고/쓸' 수 있도록 '속성' 정의 ---
        // (이것들이 '정의'되면서 setting.cs의 오류가 사라집니다)

        public string ItemCode
        {
            get { return TxtItemCode.Text; }
            set { TxtItemCode.Text = value; }
        }
        public string Brand
        {
            get { return TxtBrand.Text; }
            set { TxtBrand.Text = value; }
        }
        public string Color
        {
            get { return TxtColor.Text; }
            set { TxtColor.Text = value; }
        }

        // --- ▼▼▼ [핵심 수정!] 'Size' -> 'ProductSize'로 이름 변경 (충돌 방지) ▼▼▼ ---
        public string ProductSize
        {
            get { return TxtSize.Text; }
            set { TxtSize.Text = value; }
        }

        [Obsolete("Use ProductSize instead of Size to avoid conflicts.")]
        public string Size
        {
            get => ProductSize;
            set => ProductSize = value;
        }
        // --- ▲▲▲ [핵심 수정!] ---

        public string Category
        {
            get { return TxtCategory.Text; }
            set { TxtCategory.Text = value; }
        }
        public string Stock
        {
            get { return TxtStock.Text; }
            set { TxtStock.Text = value; }
        }

        public WpfProductAdmin()
        {
            InitializeComponent();
        }

        // --- 3. 텍스트박스를 비우는 '공용' 함수 ---
        public void ClearTextBoxes()
        {
            TxtItemCode.Text = "";
            TxtBrand.Text = "";
            TxtColor.Text = "";
            TxtSize.Text = "";
            TxtCategory.Text = "";
            TxtStock.Text = "";
        }

        // --- 4. XAML의 버튼 클릭 시, "신호" 발생 ---
        private void BtnRegister_Click(object? sender, RoutedEventArgs e)
        {
            RegisterClicked?.Invoke(this, EventArgs.Empty);
        }
        private void BtnUpdate_Click(object? sender, RoutedEventArgs e)
        {
            UpdateClicked?.Invoke(this, EventArgs.Empty);
        }
        private void BtnDelete_Click(object? sender, RoutedEventArgs e)
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }
        private void BtnRefresh_Click(object? sender, RoutedEventArgs e)
        {
            RefreshClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
