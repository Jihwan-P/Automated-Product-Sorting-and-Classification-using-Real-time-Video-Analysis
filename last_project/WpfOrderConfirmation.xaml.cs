using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace last_project
{
    public partial class WpfOrderConfirmation : System.Windows.Controls.UserControl
    {
        // WinForms로 보낼 이벤트 신호
        public event EventHandler? RefreshClicked;
        public event EventHandler<string>? ApproveClicked; // 선택된 ID 보냄
        public event EventHandler<string>? CancelOrderClicked; // 선택된 ID 보냄

        public event EventHandler<string>? DeleteOrderClicked;

        public WpfOrderConfirmation()
        {
            InitializeComponent();
        }

        // 그리드에 데이터 채우기 (외부에서 호출)
        public void SetOrderData(List<OrderModel> orders)
        {
            OrderGrid.ItemsSource = orders;
        }

        // 새로고침 버튼
        private void BtnRefresh_Click(object? sender, RoutedEventArgs e)
        {
            RefreshClicked?.Invoke(this, EventArgs.Empty);
        }

        // 승인 버튼
        private void BtnApprove_Click(object? sender, RoutedEventArgs e)
        {
            if (OrderGrid.SelectedItem is OrderModel selectedOrder)
            {
                ApproveClicked?.Invoke(this, selectedOrder.Id.ToString());
            }
            else
            {
                 System.Windows.MessageBox.Show("승인할 주문을 선택해주세요.");
            }
        }

        // 취소 버튼
        private void BtnCancelOrder_Click(object? sender, RoutedEventArgs e)
        {
            if (OrderGrid.SelectedItem is OrderModel selectedOrder)
            {
                CancelOrderClicked?.Invoke(this, selectedOrder.Id.ToString());
            }
            else
            {
                System.Windows.MessageBox.Show("취소할 주문을 선택해주세요.");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (OrderGrid.SelectedItem is OrderModel selectedOrder)
            {
                // 정말 삭제할 것인지 물어보는 것은 main.cs에서 처리하거나 여기서 해도 됨.
                // 여기서는 ID만 전달합니다.
                DeleteOrderClicked?.Invoke(this, selectedOrder.Id.ToString());
            }
            else
            {
                System.Windows.MessageBox.Show("삭제할 주문 내역을 선택해주세요.");
            }
        }

    }
}
