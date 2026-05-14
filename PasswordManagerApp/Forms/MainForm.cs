using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using ClosedXML.Excel;
using PasswordManagerApp.Models;
using PasswordManagerApp.Data;

namespace PasswordManagerApp.Forms
{
    public partial class MainForm : Form
    {
        private Panel sidebar = null!;
        private TabControl tabControl = null!;
        private DataGridView dataGridView = null!;
        private TextBox txtProject = null!, txtAddress = null!, txtAccount = null!, txtPassword = null!, txtNotes = null!, txtSearch = null!;
        private TextBox txtExtra1 = null!, txtExtra2 = null!, txtExtra3 = null!;
        private Label lblExtra1 = null!, lblExtra2 = null!, lblExtra3 = null!;
        private Button btnCopyExtra1 = null!, btnCopyExtra2 = null!, btnCopyExtra3 = null!;
        private Label lblAvatar = null!, lblUsername = null!, lblStats = null!;
        private Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnSearch = null!, btnLogout = null!, btnChangeLoginPw = null!, btnExport = null!, btnImport = null!, btnSettings = null!;
        private Label lblAuthor = null!;
        private Panel mainContentPanel = null!, settingsPanel = null!;
        private ListBox lstLibraries = null!;
        private TextBox txtOldPw = null!, txtNewPw = null!;
        private TextBox txtLibName = null!, txtLibCol1 = null!, txtLibCol2 = null!, txtLibCol3 = null!, txtLibCol4 = null!, txtLibCol5 = null!, txtLibCol6 = null!, txtLibCol7 = null!, txtLibCol8 = null!;
        private Label lblCurrentHome = null!;
        private List<PasswordEntry> currentEntries = new List<PasswordEntry>();
        private List<LibraryConfig> allLibraries = new List<LibraryConfig>();

        public MainForm()
        {
            InitializeComponent();
            LoadData();
            tabControl.SelectedIndexChanged += (object? s, EventArgs e) => FilterData();
        }

        private void InitializeComponent()
        {
            this.Text = "密码管理系统";
            this.Size = new Size(1600, 1000);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Sidebar
            sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BackColor = Color.FromArgb(63, 81, 181) // Indigo blue
            };

            lblAvatar = new Label
            {
                Text = "😊",
                Font = new Font("Segoe UI Emoji", 80F),
                Location = new Point(50, 50),
                Size = new Size(200, 200),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White
            };

            lblUsername = new Label
            {
                Text = "admin",
                Font = new Font("Microsoft YaHei", 18F, FontStyle.Bold),
                Location = new Point(0, 260),
                Size = new Size(300, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White
            };

            btnLogout = new Button
            {
                Text = "注销",
                Location = new Point(75, 330),
                Size = new Size(150, 50),
                Font = new Font("Microsoft YaHei", 12F),
                BackColor = Color.FromArgb(48, 63, 159),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogout.Click += (object? s, EventArgs e) => { this.Close(); };

            btnChangeLoginPw = new Button
            {
                Text = "修改登录密码",
                Location = new Point(75, 400),
                Size = new Size(150, 50),
                Font = new Font("Microsoft YaHei", 12F),
                BackColor = Color.FromArgb(48, 63, 159),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnChangeLoginPw.Click += BtnChangeLoginPw_Click;

            btnSettings = new Button
            {
                Text = "⚙️",
                Font = new Font("Segoe UI Emoji", 20F),
                Location = new Point(110, 480),
                Size = new Size(80, 80),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnSettings.Click += (s, e) => ToggleSettings();

            lblStats = new Label
            {
                Text = "密码库已存 0 条记录\n自定义库已存 0 条记录",
                Font = new Font("Microsoft YaHei", 12F),
                Location = new Point(20, 800),
                Size = new Size(260, 100),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblAuthor = new Label
            {
                Text = "著作: carryYIN",
                Font = new Font("Microsoft YaHei", 10F, FontStyle.Italic),
                Location = new Point(20, 910),
                Size = new Size(260, 30),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            sidebar.Controls.Add(lblAvatar);
            sidebar.Controls.Add(lblUsername);
            sidebar.Controls.Add(btnLogout);
            sidebar.Controls.Add(btnChangeLoginPw);
            sidebar.Controls.Add(btnSettings);
            sidebar.Controls.Add(lblStats);
            sidebar.Controls.Add(lblAuthor);

            // Main Area
            mainContentPanel = new Panel { Dock = DockStyle.Fill };

            // TabControl
            tabControl = new TabControl { Dock = DockStyle.Top, Height = 60, Font = new Font("Microsoft YaHei", 12F) };
            UpdateTabNames();

            // Input Section
            Panel inputPanel = new Panel { Dock = DockStyle.Top, Height = 320, Padding = new Padding(20) };
            
            AddLabelAndTextBox(inputPanel, "项目名", out txtProject, 40, 30);
            AddLabelAndTextBox(inputPanel, "账户", out txtAccount, 40, 90);
            AddLabelAndTextBox(inputPanel, "备注", out txtNotes, 40, 150);

            AddLabelAndTextBox(inputPanel, "地址", out txtAddress, 650, 30);
            AddLabelAndTextBox(inputPanel, "密码", out txtPassword, 650, 90);

            // Extra columns (now rearranged to fit the layout)
            AddLabelAndTextBox(inputPanel, "", out txtExtra1, 650, 150, out lblExtra1, out btnCopyExtra1);
            AddLabelAndTextBox(inputPanel, "", out txtExtra2, 40, 210, out lblExtra2, out btnCopyExtra2);
            AddLabelAndTextBox(inputPanel, "", out txtExtra3, 650, 210, out lblExtra3, out btnCopyExtra3);
            
            lblExtra1.Visible = txtExtra1.Visible = btnCopyExtra1.Visible = false;
            lblExtra2.Visible = txtExtra2.Visible = btnCopyExtra2.Visible = false;
            lblExtra3.Visible = txtExtra3.Visible = btnCopyExtra3.Visible = false;

            btnAdd = new Button { Text = "新增项", Location = new Point(40, 270), Size = new Size(130, 45), Font = new Font("Microsoft YaHei", 11F), BackColor = Color.FromArgb(0, 150, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += BtnAdd_Click;

            btnUpdate = new Button { Text = "修改项", Location = new Point(180, 270), Size = new Size(130, 45), Font = new Font("Microsoft YaHei", 11F), BackColor = Color.FromArgb(255, 152, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button { Text = "删除选中项", Location = new Point(320, 270), Size = new Size(130, 45), Font = new Font("Microsoft YaHei", 11F), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += BtnDelete_Click;

            btnExport = new Button { Text = "导出", Location = new Point(460, 270), Size = new Size(80, 45), Font = new Font("Microsoft YaHei", 11F), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExport.Click += BtnExport_Click;

            btnImport = new Button { Text = "导入", Location = new Point(550, 270), Size = new Size(80, 45), Font = new Font("Microsoft YaHei", 11F), BackColor = Color.FromArgb(156, 39, 176), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnImport.Click += BtnImport_Click;

            txtSearch = new TextBox { Location = new Point(700, 275), Size = new Size(250, 40), Font = new Font("Microsoft YaHei", 12F) };
            btnSearch = new Button { Text = "🔍 搜索", Location = new Point(960, 270), Size = new Size(100, 45), Font = new Font("Microsoft YaHei", 11F), FlatStyle = FlatStyle.Flat };
            btnSearch.Click += (object? s, EventArgs e) => LoadData(txtSearch.Text);

            inputPanel.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnExport, btnImport, txtSearch, btnSearch });

            // Grid Section
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Font = new Font("Microsoft YaHei", 11F),
                RowTemplate = { Height = 40 }
            };
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei", 12F, FontStyle.Bold);
            dataGridView.SelectionChanged += DataGridView_SelectionChanged;

            mainContentPanel.Controls.Add(dataGridView);
            mainContentPanel.Controls.Add(inputPanel);
            mainContentPanel.Controls.Add(tabControl);

            InitializeSettingsPanel();

            this.Controls.Add(mainContentPanel);
            this.Controls.Add(settingsPanel);
            this.Controls.Add(sidebar);

            // Set Default Tab
            string defaultTab = DatabaseHelper.GetConfig("DefaultTab", "密码库");
            foreach (TabPage tab in tabControl.TabPages)
            {
                if (tab.Text == defaultTab)
                {
                    tabControl.SelectedTab = tab;
                    break;
                }
            }
        }

        private void UpdateTabNames()
        {
            tabControl.TabPages.Clear();
            allLibraries = DatabaseHelper.GetAllLibraries();
            foreach (var lib in allLibraries)
            {
                TabPage tp = new TabPage(lib.Name);
                tp.Tag = lib;
                tabControl.TabPages.Add(tp);
            }
        }

        private void InitializeSettingsPanel()
        {
            settingsPanel = new Panel { Dock = DockStyle.Fill, Visible = false, BackColor = Color.White, Padding = new Padding(20) };
            
            // 1. Change Password Section
            GroupBox gpPw = new GroupBox { Text = "修改登录密码", Location = new Point(20, 20), Size = new Size(1000, 160), Font = new Font("Microsoft YaHei", 12F) };
            gpPw.Controls.Add(new Label { Text = "原密码:", Location = new Point(40, 40), Size = new Size(100, 30) });
            txtOldPw = new TextBox { Location = new Point(150, 40), Size = new Size(300, 30), PasswordChar = '*' };
            gpPw.Controls.Add(txtOldPw);
            gpPw.Controls.Add(new Label { Text = "新密码:", Location = new Point(40, 90), Size = new Size(100, 30) });
            txtNewPw = new TextBox { Location = new Point(150, 90), Size = new Size(300, 30), PasswordChar = '*' };
            gpPw.Controls.Add(txtNewPw);
            Button btnSavePw = new Button { Text = "修改", Location = new Point(480, 60), Size = new Size(120, 45), BackColor = Color.FromArgb(0, 150, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSavePw.Click += (s, e) => SaveNewPassword();
            gpPw.Controls.Add(btnSavePw);

            // 2. Library Management Settings
            GroupBox gpLibs = new GroupBox { Text = "库管理设置", Location = new Point(20, 200), Size = new Size(1000, 420), Font = new Font("Microsoft YaHei", 12F) };
            
            lstLibraries = new ListBox { Location = new Point(20, 40), Size = new Size(200, 350) };
            lstLibraries.SelectedIndexChanged += LstLibraries_SelectedIndexChanged;
            gpLibs.Controls.Add(lstLibraries);

            Label lblName = new Label { Text = "库名称:", Location = new Point(240, 40), Size = new Size(80, 30) };
            txtLibName = new TextBox { Location = new Point(330, 40), Size = new Size(250, 30) };
            gpLibs.Controls.Add(lblName);
            gpLibs.Controls.Add(txtLibName);

            Label lblCols = new Label { Text = "表列名:", Location = new Point(240, 100), Size = new Size(80, 30) };
            txtLibCol1 = new TextBox { Location = new Point(330, 100), Size = new Size(110, 30) };
            txtLibCol2 = new TextBox { Location = new Point(450, 100), Size = new Size(110, 30) };
            txtLibCol3 = new TextBox { Location = new Point(570, 100), Size = new Size(110, 30) };
            txtLibCol4 = new TextBox { Location = new Point(690, 100), Size = new Size(110, 30) };
            txtLibCol5 = new TextBox { Location = new Point(810, 100), Size = new Size(110, 30) };
            txtLibCol6 = new TextBox { Location = new Point(330, 150), Size = new Size(110, 30) };
            txtLibCol7 = new TextBox { Location = new Point(450, 150), Size = new Size(110, 30) };
            txtLibCol8 = new TextBox { Location = new Point(570, 150), Size = new Size(110, 30) };
            gpLibs.Controls.AddRange(new Control[] { lblCols, txtLibCol1, txtLibCol2, txtLibCol3, txtLibCol4, txtLibCol5, txtLibCol6, txtLibCol7, txtLibCol8 });

            Button btnAddLib = new Button { Text = "新增库", Location = new Point(330, 220), Size = new Size(100, 40), BackColor = Color.FromArgb(0, 150, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAddLib.Click += (s, e) => AddNewLibrary();
            
            Button btnSaveLib = new Button { Text = "保存设置", Location = new Point(440, 220), Size = new Size(100, 40), BackColor = Color.FromArgb(255, 152, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSaveLib.Click += (s, e) => SaveLibrarySettings();

            Button btnDelLib = new Button { Text = "删除该库", Location = new Point(550, 220), Size = new Size(100, 40), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelLib.Click += (s, e) => DeleteSelectedLibrary();

            gpLibs.Controls.AddRange(new Control[] { btnAddLib, btnSaveLib, btnDelLib });

            // 3. Homepage Settings
            GroupBox gpHome = new GroupBox { Text = "首页设置", Location = new Point(20, 640), Size = new Size(1000, 140), Font = new Font("Microsoft YaHei", 12F) };
            lblCurrentHome = new Label { Text = $"当前首页为: {DatabaseHelper.GetConfig("DefaultTab")}", Location = new Point(40, 50), Size = new Size(400, 30) };
            gpHome.Controls.Add(lblCurrentHome);
            Button btnSetHome = new Button { Text = "修改为当前选定库", Location = new Point(450, 45), Size = new Size(200, 45), BackColor = Color.FromArgb(0, 150, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSetHome.Click += (s, e) => SetDefaultTab();
            gpHome.Controls.Add(btnSetHome);

            settingsPanel.Controls.AddRange(new Control[] { gpPw, gpLibs, gpHome });
        }

        private void ToggleSettings()
        {
            settingsPanel.Visible = !settingsPanel.Visible;
            mainContentPanel.Visible = !settingsPanel.Visible;
            if (settingsPanel.Visible)
            {
                RefreshLibraryList();
                lblCurrentHome.Text = $"当前首页为: {DatabaseHelper.GetConfig("DefaultTab")}";
            }
        }

        private void SaveNewPassword()
        {
            string currentPw = DatabaseHelper.GetLoginPassword();
            if (txtOldPw.Text != currentPw)
            {
                MessageBox.Show("原密码错误！", "错误");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNewPw.Text))
            {
                MessageBox.Show("新密码不能为空！", "错误");
                return;
            }
            DatabaseHelper.SetLoginPassword(txtNewPw.Text);
            MessageBox.Show("密码修改成功！", "提示");
            txtOldPw.Clear();
            txtNewPw.Clear();
        }

        private void RefreshLibraryList()
        {
            lstLibraries.Items.Clear();
            allLibraries = DatabaseHelper.GetAllLibraries();
            foreach (var lib in allLibraries)
            {
                lstLibraries.Items.Add(lib.Name);
            }
            if (lstLibraries.Items.Count > 0) lstLibraries.SelectedIndex = 0;
        }

        private void LstLibraries_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstLibraries.SelectedIndex >= 0)
            {
                var lib = allLibraries[lstLibraries.SelectedIndex];
                txtLibName.Text = lib.Name;
                txtLibCol1.Text = lib.Col1;
                txtLibCol2.Text = lib.Col2;
                txtLibCol3.Text = lib.Col3;
                txtLibCol4.Text = lib.Col4;
                txtLibCol5.Text = lib.Col5;
                txtLibCol6.Text = lib.Col6;
                txtLibCol7.Text = lib.Col7;
                txtLibCol8.Text = lib.Col8;
            }
        }

        private void AddNewLibrary()
        {
            var newLib = new LibraryConfig { Name = "新库" + (allLibraries.Count + 1) };
            DatabaseHelper.SaveLibrary(newLib);
            RefreshLibraryList();
            UpdateTabNames();
        }

        private void SaveLibrarySettings()
        {
            if (lstLibraries.SelectedIndex < 0) return;
            var lib = allLibraries[lstLibraries.SelectedIndex];
            lib.Name = txtLibName.Text;
            lib.Col1 = txtLibCol1.Text;
            lib.Col2 = txtLibCol2.Text;
            lib.Col3 = txtLibCol3.Text;
            lib.Col4 = txtLibCol4.Text;
            lib.Col5 = txtLibCol5.Text;
            lib.Col6 = txtLibCol6.Text;
            lib.Col7 = txtLibCol7.Text;
            lib.Col8 = txtLibCol8.Text;
            
            DatabaseHelper.SaveLibrary(lib);
            RefreshLibraryList();
            UpdateTabNames();
            MessageBox.Show("库配置已保存！", "提示");
        }

        private void DeleteSelectedLibrary()
        {
            if (lstLibraries.SelectedIndex < 0) return;
            var lib = allLibraries[lstLibraries.SelectedIndex];
            if (lib.Name == "密码库")
            {
                MessageBox.Show("不能删除基础密码库！", "警告");
                return;
            }
            if (MessageBox.Show($"确定要删除库「{lib.Name}」及其所有密码记录吗？", "确认删除", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DatabaseHelper.DeleteLibrary(lib.Id);
                RefreshLibraryList();
                UpdateTabNames();
            }
        }

        private void SetDefaultTab()
        {
            string current = tabControl.SelectedTab?.Text ?? "密码库";
            DatabaseHelper.SetConfig("DefaultTab", current);
            lblCurrentHome.Text = $"当前首页为: {current}";
            MessageBox.Show($"已将「{current}」设置为默认首页！\n下次启动软件将直接进入此页面。", "设置成功");
        }

        private void AddLabelAndTextBox(Panel panel, string labelText, out TextBox textBox, int x, int y)
        {
            AddLabelAndTextBox(panel, labelText, out textBox, x, y, out _, out _);
        }

        private void AddLabelAndTextBox(Panel panel, string labelText, out TextBox textBox, int x, int y, out Label lbl, out Button btnCopy)
        {
            lbl = new Label { Text = labelText, Location = new Point(x, y), Size = new Size(100, 40), Font = new Font("Microsoft YaHei", 12F), TextAlign = ContentAlignment.MiddleLeft };
            textBox = new TextBox { Location = new Point(x + 110, y), Size = new Size(350, 40), Font = new Font("Microsoft YaHei", 12F) };
            panel.Controls.Add(lbl);
            panel.Controls.Add(textBox);

            btnCopy = new Button { Text = "复制", Location = new Point(x + 470, y), Size = new Size(80, 40), Font = new Font("Microsoft YaHei", 11F), FlatStyle = FlatStyle.Flat };
            TextBox localTxt = textBox;
            btnCopy.Click += (object? s, EventArgs e) => { if (!string.IsNullOrEmpty(localTxt.Text)) Clipboard.SetText(localTxt.Text); };
            panel.Controls.Add(btnCopy);
        }

        private void LoadData(string search = "")
        {
            if (string.IsNullOrEmpty(search))
                currentEntries = DatabaseHelper.GetAllEntries();
            else
                currentEntries = DatabaseHelper.SearchEntries(search);

            FilterData();
            UpdateStats();
        }

        private void FilterData()
        {
            string? category = tabControl.SelectedTab?.Text;
            if (category == null) return;
            var filtered = currentEntries.FindAll(e => e.Category == category);
            
            // Use a wrapper or anonymous type to add a sequence number
            var dataWithIndex = filtered.Select((e, index) => new {
                Index = index + 1,
                e.ProjectName,
                e.Address,
                e.Account,
                e.Password,
                e.Notes,
                e.ExtraCol1,
                e.ExtraCol2,
                e.ExtraCol3,
                e.Id,
                e.Category
            }).ToList();

            dataGridView.DataSource = null;
            dataGridView.DataSource = dataWithIndex;
            
            if (dataGridView.Columns["Id"] != null) dataGridView.Columns["Id"]!.Visible = false;
            if (dataGridView.Columns["Category"] != null) dataGridView.Columns["Category"]!.Visible = false;

            // Set Chinese Headers
            if (dataGridView.Columns["Index"] != null) dataGridView.Columns["Index"]!.HeaderText = "序号";

            var currentLib = allLibraries.FirstOrDefault(l => l.Name == category);
            if (currentLib != null)
            {
                SetColumnHeader("ProjectName", currentLib.Col1);
                SetColumnHeader("Address", currentLib.Col2);
                SetColumnHeader("Account", currentLib.Col3);
                SetColumnHeader("Password", currentLib.Col4);
                SetColumnHeader("Notes", currentLib.Col5);
                SetColumnHeader("ExtraCol1", currentLib.Col6, true);
                SetColumnHeader("ExtraCol2", currentLib.Col7, true);
                SetColumnHeader("ExtraCol3", currentLib.Col8, true);

                UpdateInputVisibility(currentLib.Col6, currentLib.Col7, currentLib.Col8);
            }
        }

        private void SetColumnHeader(string colName, string headerText, bool isExtra = false)
        {
            if (dataGridView.Columns[colName] != null)
            {
                if (string.IsNullOrWhiteSpace(headerText))
                {
                    dataGridView.Columns[colName]!.Visible = false;
                }
                else
                {
                    dataGridView.Columns[colName]!.Visible = true;
                    dataGridView.Columns[colName]!.HeaderText = headerText;
                }
            }
        }

        private void UpdateInputVisibility(string col6, string col7, string col8)
        {
            lblExtra1.Visible = txtExtra1.Visible = btnCopyExtra1.Visible = !string.IsNullOrWhiteSpace(col6);
            if (lblExtra1.Visible) lblExtra1.Text = col6;

            lblExtra2.Visible = txtExtra2.Visible = btnCopyExtra2.Visible = !string.IsNullOrWhiteSpace(col7);
            if (lblExtra2.Visible) lblExtra2.Text = col7;

            lblExtra3.Visible = txtExtra3.Visible = btnCopyExtra3.Visible = !string.IsNullOrWhiteSpace(col8);
            if (lblExtra3.Visible) lblExtra3.Text = col8;
        }

        private void UpdateStats()
        {
            int pwCount = currentEntries.FindAll(e => e.Category == "密码库").Count;
            int customCount = currentEntries.FindAll(e => e.Category == "自定义库").Count;
            lblStats.Text = $"密码库已存 {pwCount} 条记录\n自定义库已存 {customCount} 条记录";
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProject.Text))
            {
                MessageBox.Show("项目名不能为空！");
                return;
            }

            var entry = new PasswordEntry
            {
                ProjectName = txtProject.Text,
                Address = txtAddress.Text,
                Account = txtAccount.Text,
                Password = txtPassword.Text,
                Notes = txtNotes.Text,
                ExtraCol1 = txtExtra1.Text,
                ExtraCol2 = txtExtra2.Text,
                ExtraCol3 = txtExtra3.Text,
                Category = tabControl.SelectedTab?.Text ?? "密码库"
            };

            DatabaseHelper.AddEntry(entry);
            LoadData();
            ClearInputs();
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要修改的项！");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtProject.Text))
            {
                MessageBox.Show("项目名不能为空！");
                return;
            }

            var entry = new PasswordEntry
            {
                Id = (int)(dataGridView.SelectedRows[0].Cells["Id"].Value ?? 0),
                ProjectName = txtProject.Text,
                Address = txtAddress.Text,
                Account = txtAccount.Text,
                Password = txtPassword.Text,
                Notes = txtNotes.Text,
                ExtraCol1 = txtExtra1.Text,
                ExtraCol2 = txtExtra2.Text,
                ExtraCol3 = txtExtra3.Text,
                Category = tabControl.SelectedTab?.Text ?? "密码库"
            };

            DatabaseHelper.UpdateEntry(entry);
            LoadData();
            ClearInputs();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                int id = (int)(dataGridView.SelectedRows[0].Cells["Id"].Value ?? 0);
                DatabaseHelper.DeleteEntry(id);
                LoadData();
                ClearInputs();
            }
        }

        private void DataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var row = dataGridView.SelectedRows[0];
                txtProject.Text = row.Cells["ProjectName"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";
                txtAccount.Text = row.Cells["Account"].Value?.ToString() ?? "";
                txtPassword.Text = row.Cells["Password"].Value?.ToString() ?? "";
                txtNotes.Text = row.Cells["Notes"].Value?.ToString() ?? "";
                txtExtra1.Text = row.Cells["ExtraCol1"].Value?.ToString() ?? "";
                txtExtra2.Text = row.Cells["ExtraCol2"].Value?.ToString() ?? "";
                txtExtra3.Text = row.Cells["ExtraCol3"].Value?.ToString() ?? "";
            }
        }

        private void ClearInputs()
        {
            txtProject.Clear();
            txtAddress.Clear();
            txtAccount.Clear();
            txtPassword.Clear();
            txtNotes.Clear();
            txtExtra1.Clear();
            txtExtra2.Clear();
            txtExtra3.Clear();
            dataGridView.ClearSelection();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = "Passwords_Export.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("密码库");
                            worksheet.Cell(1, 1).Value = "项目名";
                            worksheet.Cell(1, 2).Value = "地址";
                            worksheet.Cell(1, 3).Value = "账户";
                            worksheet.Cell(1, 4).Value = "密码";
                            worksheet.Cell(1, 5).Value = "备注";
                            worksheet.Cell(1, 6).Value = "Extra1";
                            worksheet.Cell(1, 7).Value = "Extra2";
                            worksheet.Cell(1, 8).Value = "Extra3";
                            worksheet.Cell(1, 9).Value = "分类";

                            for (int i = 0; i < currentEntries.Count; i++)
                            {
                                worksheet.Cell(i + 2, 1).Value = currentEntries[i].ProjectName;
                                worksheet.Cell(i + 2, 2).Value = currentEntries[i].Address;
                                worksheet.Cell(i + 2, 3).Value = currentEntries[i].Account;
                                worksheet.Cell(i + 2, 4).Value = currentEntries[i].Password;
                                worksheet.Cell(i + 2, 5).Value = currentEntries[i].Notes;
                                worksheet.Cell(i + 2, 6).Value = currentEntries[i].ExtraCol1;
                                worksheet.Cell(i + 2, 7).Value = currentEntries[i].ExtraCol2;
                                worksheet.Cell(i + 2, 8).Value = currentEntries[i].ExtraCol3;
                                worksheet.Cell(i + 2, 9).Value = currentEntries[i].Category;
                            }

                            workbook.SaveAs(sfd.FileName);
                        }
                        MessageBox.Show("导出成功！", "提示");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败: {ex.Message}", "错误");
                    }
                }
            }
        }

        private void BtnImport_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Excel Workbook|*.xlsx" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook(ofd.FileName))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var range = worksheet.RangeUsed();
                            if (range == null)
                            {
                                MessageBox.Show("Excel 文件为空！", "提示");
                                return;
                            }
                            var rows = range.RowsUsed().Skip(1); // Skip header row

                            int importedCount = 0;
                            foreach (var row in rows)
                            {
                                var entry = new PasswordEntry
                                {
                                    ProjectName = row.Cell(1).GetValue<string>(),
                                    Address = row.Cell(2).GetValue<string>(),
                                    Account = row.Cell(3).GetValue<string>(),
                                    Password = row.Cell(4).GetValue<string>(),
                                    Notes = row.Cell(5).GetValue<string>(),
                                    ExtraCol1 = row.Cell(6).GetValue<string>(),
                                    ExtraCol2 = row.Cell(7).GetValue<string>(),
                                    ExtraCol3 = row.Cell(8).GetValue<string>(),
                                    Category = row.Cell(9).GetValue<string>()
                                };

                                if (string.IsNullOrWhiteSpace(entry.Category)) entry.Category = "密码库";
                                
                                DatabaseHelper.AddEntry(entry);
                                importedCount++;
                            }

                            MessageBox.Show($"成功导入 {importedCount} 条记录！", "提示");
                            LoadData();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导入失败: {ex.Message}", "错误");
                    }
                }
            }
        }

        private void BtnChangeLoginPw_Click(object? sender, EventArgs e)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 400;
                prompt.Height = 250;
                prompt.Text = "修改登录密码";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.MaximizeBox = false;

                Label textLabel = new Label() { Left = 20, Top = 20, Text = "请输入新密码:", Width = 350 };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, PasswordChar = '*' };
                Button confirmation = new Button() { Text = "确定", Left = 260, Width = 100, Top = 120, DialogResult = DialogResult.OK };
                confirmation.Click += (s, ev) => { prompt.Close(); };

                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    string newPw = textBox.Text;
                    if (!string.IsNullOrWhiteSpace(newPw))
                    {
                        DatabaseHelper.SetLoginPassword(newPw);
                        MessageBox.Show("登录密码已修改成功！");
                    }
                }
            }
        }
    }
}
