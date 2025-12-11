namespace last_project
{
    partial class setting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            splitContainer1 = new SplitContainer();
            dataGridView1 = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            tabPage2 = new TabPage();
            dataGridView2 = new DataGridView();
            Column13 = new DataGridViewTextBoxColumn();
            Column8 = new DataGridViewTextBoxColumn();
            Column9 = new DataGridViewTextBoxColumn();
            Column10 = new DataGridViewTextBoxColumn();
            Column11 = new DataGridViewTextBoxColumn();
            Column12 = new DataGridViewTextBoxColumn();
            tabPage3 = new TabPage();
            tabPage4 = new TabPage();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 3);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1043, 658);
            tabControl1.TabIndex = 0;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(splitContainer1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1035, 630);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "슬롯 상세 설정";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(dataGridView1);
            splitContainer1.Size = new Size(1029, 624);
            splitContainer1.SplitterDistance = 627;
            splitContainer1.TabIndex = 0;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, Column6 });
            dataGridView1.Location = new Point(3, 216);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowTemplate.Height = 25;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(394, 406);
            dataGridView1.TabIndex = 1;
            // 
            // Column1
            // 
            Column1.HeaderText = "ID";
            Column1.Name = "Column1";
            Column1.Width = 66;
            // 
            // Column2
            // 
            Column2.HeaderText = "X";
            Column2.Name = "Column2";
            Column2.Width = 66;
            // 
            // Column3
            // 
            Column3.HeaderText = "Y";
            Column3.Name = "Column3";
            Column3.Width = 66;
            // 
            // Column4
            // 
            Column4.HeaderText = "W";
            Column4.Name = "Column4";
            Column4.Width = 66;
            // 
            // Column5
            // 
            Column5.HeaderText = "H";
            Column5.Name = "Column5";
            Column5.Width = 66;
            // 
            // Column6
            // 
            Column6.HeaderText = "슬롯 활성화";
            Column6.Name = "Column6";
            Column6.Width = 66;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(dataGridView2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1035, 630);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "제품 품목 설정";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { Column13, Column8, Column9, Column10, Column11, Column12 });
            dataGridView2.Location = new Point(2, 277);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowTemplate.Height = 25;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.Size = new Size(431, 238);
            dataGridView2.TabIndex = 3;
            dataGridView2.CellClick += dataGridView2_CellClick_1;
            dataGridView2.CellContentClick += dataGridView2_CellContentClick;
            // 
            // Column13
            // 
            Column13.DataPropertyName = "item_code";
            Column13.HeaderText = "품목번호";
            Column13.Name = "Column13";
            Column13.Width = 75;
            // 
            // Column8
            // 
            Column8.DataPropertyName = "brand";
            Column8.HeaderText = "브랜드";
            Column8.Name = "Column8";
            Column8.Width = 75;
            // 
            // Column9
            // 
            Column9.DataPropertyName = "color";
            Column9.HeaderText = "색상";
            Column9.Name = "Column9";
            Column9.Width = 75;
            // 
            // Column10
            // 
            Column10.DataPropertyName = "size";
            Column10.HeaderText = "사이즈";
            Column10.Name = "Column10";
            Column10.Width = 75;
            // 
            // Column11
            // 
            Column11.DataPropertyName = "category";
            Column11.HeaderText = "카테고리";
            Column11.Name = "Column11";
            Column11.Width = 75;
            // 
            // Column12
            // 
            Column12.DataPropertyName = "stock";
            Column12.HeaderText = "재고";
            Column12.Name = "Column12";
            Column12.Width = 75;
            // 
            // tabPage3
            // 
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(1035, 630);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "수동 제어";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(1035, 630);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "로그아웃";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // setting
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1034, 610);
            Controls.Add(tabControl1);
            Name = "setting";
            Text = "setting";
            Load += setting_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        // --- ▼▼▼ [수정!] 맨 아래 변수 선언 코드 ▼▼▼ ---
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;

        // --- (tabPage2 "제품 품목 설정" 관련 변수들 - 'dataGridView2'만 남김) ---
        private DataGridView dataGridView2;

        // --- ▼▼▼ [삭제!] groupBox2와 그 안의 컨트롤, 버튼 4개 변수 "모두 삭제" ▼▼▼ ---
        // private GroupBox groupBox2;
        // private TextBox txtCategory;
        // ( ... 이하 txtSize, txtColor, txtBrand, txtItemCode, txtStock ... )
        // ( ... 이하 label11, label10, label9, label8, label6, label7 ... )
        // private Button btnRefresh;
        // private Button btnDelete;
        // private Button button3;
        // private Button btnRegister;
        // --- ▲▲▲ [삭제!] 여기까지 ▲▲▲ ---

        // --- (tabPage1 "슬롯 상세 설정" 관련 변수들 - 그대로 둡니다) ---
        private SplitContainer splitContainer1;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column6;
        private DataGridViewTextBoxColumn Column13;
        private DataGridViewTextBoxColumn Column8;
        private DataGridViewTextBoxColumn Column9;
        private DataGridViewTextBoxColumn Column10;
        private DataGridViewTextBoxColumn Column11;
        private DataGridViewTextBoxColumn Column12;
        private TabPage tabPage4;
    }
}

