using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http; // 서버 통신용
using Newtonsoft.Json; // JSON 변환용
using Microsoft.VisualBasic; // InputBox 용
using System.Windows.Forms.Integration; // <-- 1. ElementHost를 위해 추가!

namespace last_project
{
    public partial class setting : Form
    {
        // (★★★★★) 서버와 통신하기 위한 필수 객체입니다.
        private static readonly HttpClient client = new HttpClient();

        // --- ▼▼▼ [수정] 클래스 변수 5줄 (오류 방지) ▼▼▼ ---
        private Size largeFormSize = new Size(1043, 658);
        private Size smallFormSize = new Size(520, 584);
        private Size productTabSize = new Size(456, 581);

        // (WPF 컨트롤 3개를 클래스 변수로 선언)
        private WpfSlotEditor wpfEditor = null!;
        private WpfSlotInfo wpfSlotInfo = null!;
        private WpfProductAdmin wpfProductAdmin = null!;
        private bool isManualControlLoaded = false; // (tabPage3용 '깃발')
        // --- ▲▲▲ 'isSlotEditorLoaded' 깃발은 이제 필요 없음 ▲▲▲ ---
        private WpfLogoutControl wpfLogoutControl = null!;
        private bool isLogoutLoaded = false;
        private TabPage tabPageProfile = null!; // 코드로 추가할 탭 페이지 객체
        private bool isMyInfoLoaded = false; // 내 정보 탭이 로드되었는지 확인하는 플래그
        public setting()
        {
            InitializeComponent();
        }

        private void WpfLogoutControl_LogoutClicked(object? sender, EventArgs e)
        {
            LogManager.Add("로그아웃 버튼 클릭됨. 확인창 표시.");

            DialogResult result = MessageBox.Show(
                "정말 로그아웃하시겠습니까?\n프로그램이 재시작되어 로그인 화면으로 돌아갑니다.",
                "로그아웃 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                LogManager.Add("사용자가 '예'를 선택. 애플리케이션을 다시 시작합니다.");
                Application.Restart(); // 로그인 폼(second.cs)으로 돌아갑니다.
            }
            else
            {
                LogManager.Add("사용자가 '아니요'를 선택. 로그아웃 취소.");
            }
        }

        // --- ▼▼▼ [추가!] 폼이 "처음 켜질 때" 실행되는 Load 이벤트 ▼▼▼ ---
        private async void setting_Load(object? sender, EventArgs e)
        {
            // --- 1. 왼쪽 패널 (WPF 카메라/그리기) 설정 ---
            if (tabPageProfile == null)
            {
                tabPageProfile = new TabPage("내 정보"); // 탭 이름
                tabPageProfile.BackColor = Color.FromArgb(30, 30, 30); // 배경색 (다크 모드)

                // 기존 탭(0, 1, 2) 뒤인 3번 인덱스에 삽입 (로그아웃 탭은 4번으로 밀림)
                if (tabControl1.TabCount >= 3)
                {
                    tabControl1.TabPages.Insert(3, tabPageProfile);
                }
                else
                {
                    tabControl1.TabPages.Add(tabPageProfile);
                }
            }
            ElementHost wpfHostLeft = new ElementHost();
            wpfHostLeft.Dock = DockStyle.Fill;
            wpfEditor = new WpfSlotEditor(); // (클래스 변수에 할당)
            wpfEditor.SlotDrawn += WpfEditor_SlotDrawn; // (신호 연결)
            wpfHostLeft.Child = wpfEditor;
            splitContainer1.Panel1.Controls.Add(wpfHostLeft);

            // --- 2. 오른쪽 패널 (WPF 슬롯 정보) 설정 ---
            ElementHost wpfHostRight = new ElementHost();
            wpfHostRight.Dock = DockStyle.Fill;
            wpfSlotInfo = new WpfSlotInfo(); // (클래스 변수에 할당)

            // --- ▼▼▼ [추가!] "저장" 버튼 신호 연결! ▼▼▼ ---
            wpfSlotInfo.SaveButtonClicked += async (s, ev) => await SaveSlotDataAsync();

            wpfSlotInfo.DeleteButtonClicked += async (s, ev) => await DeleteSlotAsync();

            wpfHostRight.Child = wpfSlotInfo;
            splitContainer1.Panel2.Controls.Add(wpfHostRight);

            wpfHostRight.Dock = DockStyle.Top;
            wpfHostRight.Height = 220;
            dataGridView1.Dock = DockStyle.Fill;

            // --- (dataGridView1 스타일링 코드) ---
            var grid = dataGridView1;
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = System.Drawing.Color.FromArgb(45, 45, 48);
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            grid.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = System.Drawing.Color.Gray;
            grid.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(63, 63, 70); // (파란색 -> 회색)
            grid.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            grid.RowTemplate.Height = 30;
            grid.ColumnHeadersHeight = 35;
            splitContainer1.BackColor = System.Drawing.Color.Black;



            dataGridView1.CellClick += DataGridView1_CellClick;

            // --- ▼▼▼ [추가!] 폼이 켜질 때 "슬롯 목록" 로드! ▼▼▼ ---
            await LoadSlotDataAsync();
            // --- ▼▼▼ [추가!] 폼이 켜질 때 "슬롯 목록" 로드! ▼▼▼ ---
            await LoadSlotDataAsync();
        }


        // (기존 이벤트 핸들러 - 내용은 비어있음)
        private void label4_Click(object? sender, EventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object? sender, EventArgs e) { }

        // --- ▼▼▼ [수정] 탭 변경 이벤트 (WPF 로드 기능 추가) ▼▼▼ ---
        private async void tabControl1_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // 0번 인덱스 ("슬롯 상세 설정")
            if (tabControl1.SelectedIndex == 0)
            {
                this.Size = largeFormSize;
            }
            // 1번 인덱스 ("제품 품목 설정")
            else if (tabControl1.SelectedIndex == 1)
            {
                this.Size = productTabSize;

                // --- ▼▼▼ [핵심] tabPage2에 WPF 컨트롤 심기 ▼▼▼ ---
                if (wpfProductAdmin == null)
                {
                    ElementHost wpfHostProducts = new ElementHost();
                    wpfHostProducts.Dock = DockStyle.Top;
                    wpfHostProducts.Height = 300;

                    wpfProductAdmin = new WpfProductAdmin();

                    // (신호 연결) WPF 버튼의 '신호'를 C# '함수'와 연결
                    wpfProductAdmin.RegisterClicked += btnRegister_Click; // "신규등록"
                    wpfProductAdmin.UpdateClicked += button3_Click; // "수정"
                    wpfProductAdmin.DeleteClicked += btnDelete_Click; // "삭제"
                    wpfProductAdmin.RefreshClicked += btnRefresh_Click; // "새로고침"

                    wpfHostProducts.Child = wpfProductAdmin;
                    tabPage2.Controls.Add(wpfHostProducts);

                    // --- ▼▼▼ [추가!] dataGridView2 스타일링 코드 ▼▼▼ ---
                    var grid = dataGridView2;
                    grid.BorderStyle = BorderStyle.None;
                    grid.BackgroundColor = System.Drawing.Color.FromArgb(45, 45, 48);
                    grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
                    grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                    grid.EnableHeadersVisualStyles = false;
                    grid.RowHeadersVisible = false;
                    grid.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
                    grid.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                    grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                    grid.GridColor = System.Drawing.Color.Gray;
                    grid.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(63, 63, 70); // (파란색 -> 회색)
                    grid.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
                    grid.RowTemplate.Height = 30;
                    grid.ColumnHeadersHeight = 35;
                    // --- ▲▲▲ [추가!] 여기까지 ▲▲▲ ---

                    dataGridView2.Dock = DockStyle.Fill;
                }

                await LoadProductDataAsync();
            }
            // 2번 인덱스 ("수동 제어")
            else if (tabControl1.SelectedIndex == 2)
            {
                this.Size = largeFormSize;

                // (WPF 컨트롤이 '처음' 로드되는지 확인)
                if (isManualControlLoaded == false)
                {
                    // 1. (그릇) ElementHost 생성
                    ElementHost wpfHostManual = new ElementHost();
                    wpfHostManual.Dock = DockStyle.Fill; // tabPage3 꽉 채우기

                    // 2. (내용물) "WPF 수동 제어판" 생성
                    WpfManualControl wpfControl = new WpfManualControl();

                    // 3. (조립) 그릇에 내용물을 담습니다.
                    wpfHostManual.Child = wpfControl;

                    // 4. (★★★★★) tabPage3 (세 번째 탭)에 '그릇'을 추가!
                    tabPage3.Controls.Add(wpfHostManual);

                    isManualControlLoaded = true; // "띄우기 완료" 깃발
                }
            }
            // --- ▼▼▼ [3. 추가!] 3번 인덱스 ("로그아웃") 처리 ▼▼▼ ---
            else if (tabControl1.SelectedIndex == 3)
            {
                this.Size = largeFormSize; // 큰 화면 사용

                // 내 정보 컨트롤이 아직 로드되지 않았으면 로드
                if (!isMyInfoLoaded && tabPageProfile != null)
                {
                    ElementHost host = new ElementHost();
                    host.Dock = DockStyle.Fill;

                    // 방금 만든 WPF 내 정보 컨트롤 생성
                    WpfMyProfile myProfile = new WpfMyProfile();
                    host.Child = myProfile;

                    // 코드로 만든 탭 페이지에 추가
                    tabPageProfile.Controls.Add(host);
                    isMyInfoLoaded = true;
                }
            }

            // 4번: "로그아웃" 탭 (원래 3번이었으나 뒤로 밀림)
            else if (tabControl1.SelectedIndex == 4)
            {
                this.Size = smallFormSize; // 작은 화면 사용

                if (isLogoutLoaded == false)
                {
                    // ... (기존 로그아웃 로직 그대로 사용) ...
                    ElementHost wpfHostLogout = new ElementHost();
                    wpfHostLogout.Dock = DockStyle.Fill;
                    wpfLogoutControl = new WpfLogoutControl();
                    wpfLogoutControl.LogoutClicked += WpfLogoutControl_LogoutClicked;
                    wpfHostLogout.Child = wpfLogoutControl;

                    // 주의: 디자이너에 있는 기존 탭(tabPage4)을 사용
                    tabPage4.Controls.Add(wpfHostLogout);
                    isLogoutLoaded = true;
                }
            }


        }

        // --- "좌표 받기" 함수 (WpfEditor_SlotDrawn) ---
        private void WpfEditor_SlotDrawn(object? sender, SlotDrawnEventArgs e)
        {
            if (wpfSlotInfo == null) return;

            int x = (int)Math.Round(e.X);
            int y = (int)Math.Round(e.Y);
            int w = (int)Math.Round(e.W);
            int h = (int)Math.Round(e.H);

            wpfSlotInfo.SlotX = x.ToString();
            wpfSlotInfo.SlotY = y.ToString();
            wpfSlotInfo.SlotW = w.ToString();
            wpfSlotInfo.SlotH = h.ToString();
            wpfSlotInfo.SlotId = "(신규 슬롯)";
            wpfSlotInfo.IsSlotActive = true;
        }


        // --- ▼▼▼ [추가!] "슬롯 목록 읽기" C# 함수 (dataGridView1용) ▼▼▼ ---
        private async Task LoadSlotDataAsync()
        {
            var targetGrid = dataGridView1; // (★★★★★) 0번 탭의 슬롯 그리드
            targetGrid.AutoGenerateColumns = false;

            targetGrid.Columns[0].DataPropertyName = "slot_id";   // 1번째 칸: ID
            targetGrid.Columns[1].DataPropertyName = "x";         // 2번째 칸: X
            targetGrid.Columns[2].DataPropertyName = "y";         // 3번째 칸: Y
            targetGrid.Columns[3].DataPropertyName = "w";         // 4번째 칸: W
            targetGrid.Columns[4].DataPropertyName = "h";         // 5번째 칸: H
            targetGrid.Columns[5].DataPropertyName = "is_active"; // 6번째 칸: 활성화

            string apiUrl = $"http://127.0.0.1:5000/api/slots?_t={DateTime.Now.Ticks}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    // (정렬을 위해 'DataTable'로 변환)
                    DataTable? slotTable = JsonConvert.DeserializeObject<DataTable>(jsonResponse);
                    if (slotTable == null)
                    {
                        MessageBox.Show("슬롯 데이터를 불러오지 못했습니다.");
                        return;
                    }

                    targetGrid.DataSource = slotTable;
                }
                else
                {
                    MessageBox.Show($"슬롯 목록 로드 오류: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"슬롯 목록 로드 중 예외 발생: {ex.Message}");
            }
        }

        // --- ▼▼▼ [추가!] "슬롯 저장" C# 함수 (WPF '저장' 버튼용) ▼▼▼ ---
        private async Task SaveSlotDataAsync()
        {
            if (wpfSlotInfo == null) return;

            string apiUrl = "http://127.0.0.1:5000/api/slots/save";
            try
            {
                // 1. (WPF 텍스트박스에서 값 읽기)
                var slotData = new
                {
                    slot_id = wpfSlotInfo.SlotId,
                    x = Convert.ToInt32(wpfSlotInfo.SlotX),
                    y = Convert.ToInt32(wpfSlotInfo.SlotY),
                    w = Convert.ToInt32(wpfSlotInfo.SlotW),
                    h = Convert.ToInt32(wpfSlotInfo.SlotH),
                    is_active = wpfSlotInfo.IsSlotActive
                };

                if (string.IsNullOrWhiteSpace(slotData.slot_id) || slotData.slot_id == "(신규 슬롯)")
                {
                    MessageBox.Show("슬롯 아이디를 입력해주세요.\n(예: 'A-3')", "저장 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. (JSON 변환 및 전송)
                string jsonData = JsonConvert.SerializeObject(slotData);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("슬롯이 성공적으로 저장되었습니다.");
                    // 3. (마무리) 저장이 끝나면 그리드 "새로고침"
                    await LoadSlotDataAsync();
                }
                else
                {
                    MessageBox.Show($"슬롯 저장 오류: {responseString}");
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("좌표(X,Y,W,H)는 숫자만 입력해야 합니다.", "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"슬롯 저장 중 예외 발생: {ex.Message}");
            }
        }


        // --- (이하 "제품 품목 설정" 탭(tabPage2)의 모든 함수들) ---

        // --- 1. 데이터 "읽기" 함수 (DataTable 버전) ---
        private async Task LoadProductDataAsync()
        {
            var targetGrid = dataGridView2;
            targetGrid.AutoGenerateColumns = false;
            string apiUrl = $"http://127.0.0.1:5000/api/products?_t={DateTime.Now.Ticks}";
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    DataTable? productTable = JsonConvert.DeserializeObject<DataTable>(jsonResponse);
                    if (productTable == null)
                    {
                        MessageBox.Show("제품 데이터를 불러오지 못했습니다.");
                        return;
                    }

                    targetGrid.DataSource = productTable;
                }
                else
                {
                    MessageBox.Show($"서버 응답 오류: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 로드 중 오류 발생: {ex.Message}");
            }
        }

        // --- 2. 데이터 "수정" 함수 ---
        private async Task UpdateStockInDatabaseAsync(string itemCode, int newStock)
        {
            string apiUrl = "http://127.0.0.1:5000/api/product/update_stock";
            try
            {
                var updateData = new { item_code = itemCode, new_stock = newStock };
                string jsonData = JsonConvert.SerializeObject(updateData);
                StringContent content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"'{itemCode}'의 재고가 {newStock}개로 수정되었습니다.");
                }
                else
                {
                    string errorJson = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"서버 수정 오류: {errorJson}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"재고 수정 중 예외 발생: {ex.Message}");
            }
        }

        // --- 3. "재고상태" 계산 이벤트 핸들러 (삭제됨) ---
        // (private void dataGridView2_DataBindingComplete(...) 함수는 삭제했습니다)

        // (이벤트 핸들러 - 비어있음)
        private void dataGridView2_CellClick(object? sender, DataGridViewCellEventArgs e) { }
        private void dataGridView2_CellClick_1(object? sender, DataGridViewCellEventArgs e) { }
        private void dataGridView2_DataBindingComplete_1(object? sender, DataGridViewBindingCompleteEventArgs e) { }
        private void dataGridView2_CellContentClick(object? sender, DataGridViewCellEventArgs e) { }

        // --- "수정" 버튼 클릭 이벤트 (button3) ---
        private async void button3_Click(object? sender, EventArgs e)
        {
            var targetGrid = dataGridView2;
            string itemCodeColumnName = "Column13";

            if (targetGrid.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = targetGrid.SelectedRows[0];
                string? itemCode = selectedRow.Cells[itemCodeColumnName].Value?.ToString();
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    MessageBox.Show("삭제할 제품의 코드가 비어 있습니다.");
                    return;
                }

                try
                {
                    int newStock = Convert.ToInt32(wpfProductAdmin.Stock);
                    await UpdateStockInDatabaseAsync(itemCode, newStock);
                    await LoadProductDataAsync();
                }
                catch (FormatException) { MessageBox.Show("재고는 숫자만 입력해주세요."); }
                catch (Exception ex) { MessageBox.Show($"수정 중 오류: {ex.Message}"); }
            }
            else
            {
                MessageBox.Show("먼저 그리드에서 수정할 행을 '선택'해주세요.");
            }
        }

        // --- "새로고침" 버튼 클릭 이벤트 (btnRefresh) ---
        private async void btnRefresh_Click(object? sender, EventArgs e)
        {
            if (wpfProductAdmin != null) wpfProductAdmin.ClearTextBoxes();
            await LoadProductDataAsync();
        }

        // --- "신규 등록" 전송 함수 ---
        private async Task AddNewProductAsync(string itemCode, string brand, string color, string size, string category, int stock)
        {
            string apiUrl = "http://127.0.0.1:5000/api/product/add";
            try
            {
                var newProductData = new { item_code = itemCode, brand = brand, color = color, size = size, category = category, stock = stock };
                string jsonData = JsonConvert.SerializeObject(newProductData);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("신규 제품이 성공적으로 등록되었습니다.");
                }
                else
                {
                    MessageBox.Show($"등록 오류: {responseString}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"신규 등록 중 예외 발생: {ex.Message}");
            }
        }

        // --- "신규등록" 버튼 클릭 이벤트 (btnRegister) ---
        private async void btnRegister_Click(object? sender, EventArgs e)
        {
            if (wpfProductAdmin == null) return;

            string itemCode = wpfProductAdmin.ItemCode;
            string brand = wpfProductAdmin.Brand;
            string color = wpfProductAdmin.Color;
            string size = wpfProductAdmin.ProductSize; // ('Size'가 아닌 'ProductSize')
            string category = wpfProductAdmin.Category;
            string stockText = wpfProductAdmin.Stock;

            if (string.IsNullOrWhiteSpace(itemCode))
            {
                MessageBox.Show("품목번호는 필수 입력 항목입니다.");
                return;
            }

            int stock = 0;
            if (!string.IsNullOrWhiteSpace(stockText) && !int.TryParse(stockText, out stock))
            {
                MessageBox.Show("재고는 숫자만 입력해주세요.");
                return;
            }

            try
            {
                await AddNewProductAsync(itemCode, brand, color, size, category, stock);
                await LoadProductDataAsync();
                wpfProductAdmin.ClearTextBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"신규 등록 중 오류 발생: {ex.Message}");
            }
        }

        // --- "삭제" 전송 함수 ---
        private async Task DeleteProductAsync(string itemCode)
        {
            string apiUrl = "http://127.0.0.1:5000/api/product/delete";
            try
            {
                var deleteData = new { item_code = itemCode };
                string jsonData = JsonConvert.SerializeObject(deleteData);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseString = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("제품이 성공적으로 삭제되었습니다.");
                }
                else
                {
                    MessageBox.Show($"삭제 오류: {responseString}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"삭제 중 예외 발생: {ex.Message}");
            }
        }

        // --- "삭제" 버튼 클릭 이벤트 (btnDelete) ---
        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            var targetGrid = dataGridView2;
            string itemCodeColumnName = "Column13";

            if (targetGrid.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = targetGrid.SelectedRows[0];
                string? itemCode = selectedRow.Cells[itemCodeColumnName].Value?.ToString();
                if (string.IsNullOrWhiteSpace(itemCode))
                {
                    MessageBox.Show("삭제할 제품의 코드가 비어 있습니다.");
                    return;
                }

                string confirmedItemCode = itemCode;

                DialogResult result = MessageBox.Show(
                    $"정말 '{confirmedItemCode}' 제품을 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.",
                    "삭제 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await DeleteProductAsync(confirmedItemCode);
                        await LoadProductDataAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"삭제 중 오류 발생: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("먼저 그리드에서 삭제할 행을 '선택'해주세요.");
            }
        }

        // ▼▼▼ [신규 추가] 그리드 행 클릭 시 상세 정보창에 데이터 채우기 ▼▼▼
        private void DataGridView1_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            // 헤더 클릭이나 빈 공간 클릭 방지
            if (e.RowIndex < 0 || wpfSlotInfo == null) return;

            try
            {
                // 1. 선택된 행 가져오기
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // 2. 각 셀의 데이터를 가져와서 WpfSlotInfo에 전달
                // (DB 컬럼명이나 인덱스가 맞는지 확인 필요, 여기서는 순서대로 매핑한다고 가정)
                // 만약 DataTable로 바인딩했다면 컬럼 이름("slot_id" 등)으로 접근하는 것이 안전합니다.

                // 예시: DataTable의 컬럼명을 정확히 안다면: row.Cells["slot_id"].Value.ToString();
                // 현재 코드를 보면 Column1 ~ Column6으로 정의되어 있으므로 인덱스나 이름 확인이 필요합니다.
                // 보통 DataTable 바인딩 시 DataPropertyName을 따릅니다.

                // 안전하게 셀 값들을 문자열로 변환하여 넣습니다.
                // (아래 컬럼 인덱스 0~5는 dataGridView1의 컬럼 순서에 따라 조정하세요)
                wpfSlotInfo.SlotId = row.Cells[0].Value?.ToString() ?? "";       // ID
                wpfSlotInfo.SlotX = row.Cells[1].Value?.ToString() ?? "0";      // X
                wpfSlotInfo.SlotY = row.Cells[2].Value?.ToString() ?? "0";      // Y
                wpfSlotInfo.SlotW = row.Cells[3].Value?.ToString() ?? "0";      // W
                wpfSlotInfo.SlotH = row.Cells[4].Value?.ToString() ?? "0";      // H

                // "슬롯 활성화" 체크박스 (Boolean 변환)
                var activeVal = row.Cells[5].Value;
                if (activeVal is bool bVal)
                {
                    wpfSlotInfo.IsSlotActive = bVal;
                }
                else if (activeVal != null)
                {
                    // 1, "True", "true" 문자열 처리
                    string sVal = (activeVal.ToString() ?? string.Empty).ToLower();
                    wpfSlotInfo.IsSlotActive = (sVal == "1" || sVal == "true");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 선택 중 오류: {ex.Message}");
            }
        }

        // [setting.cs] 맨 아래쪽 (다른 함수들 근처)에 추가하세요.

        private async Task DeleteSlotAsync()
        {
            // 1. 선택된 ID가 있는지 확인
            string targetId = wpfSlotInfo.SlotId;
            if (string.IsNullOrWhiteSpace(targetId) || targetId == "(신규 슬롯)")
            {
                MessageBox.Show("삭제할 슬롯을 선택해주세요.");
                return;
            }

            // 2. 진짜 지울 건지 물어보기 (안전 장치)
            if (MessageBox.Show($"정말 '{targetId}' 슬롯을 삭제하시겠습니까?", "삭제 확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            // 3. 서버에 삭제 요청 보내기
            string apiUrl = "http://127.0.0.1:5000/api/slots/delete";
            try
            {
                var data = new { slot_id = targetId };
                string json = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("삭제되었습니다.");

                    // 4. 입력창 비우기
                    wpfSlotInfo.SlotId = "";
                    wpfSlotInfo.SlotX = "0";
                    wpfSlotInfo.SlotY = "0";
                    wpfSlotInfo.SlotW = "0";
                    wpfSlotInfo.SlotH = "0";
                    wpfSlotInfo.IsSlotActive = false;

                    // 5. 목록 새로고침
                    await LoadSlotDataAsync();
                }
                else
                {
                    MessageBox.Show("삭제 실패 (서버 오류)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"삭제 중 오류 발생: {ex.Message}");
            }
        }
    }
}
