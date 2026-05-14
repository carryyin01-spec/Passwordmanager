using System;
using System.Drawing;
using System.Windows.Forms;
using PasswordManagerApp.Data;

namespace PasswordManagerApp.Forms
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private CheckBox chkAutoLogin = null!;
        private Button btnLogin = null!;
        private Label lblUsername = null!;
        private Label lblPassword = null!;
        private Label lblAuthor = null!;

        public LoginForm()
        {
            InitializeComponent();
            this.Text = "密码管理系统登录";
            this.BackColor = Color.FromArgb(173, 216, 230); // Light blue background
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(675, 500);
        }

        private void InitializeComponent()
        {
            this.lblUsername = new Label();
            this.lblPassword = new Label();
            this.lblAuthor = new Label();
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.chkAutoLogin = new CheckBox();
            this.btnLogin = new Button();

            // lblUsername
            this.lblUsername.Text = "用户名";
            this.lblUsername.Location = new Point(50, 100);
            this.lblUsername.Size = new Size(120, 40);
            this.lblUsername.Font = new Font("Microsoft YaHei", 18F);
            this.lblUsername.ForeColor = Color.FromArgb(0, 51, 102);
            this.lblUsername.TextAlign = ContentAlignment.MiddleRight;

            // txtUsername
            this.txtUsername.Location = new Point(180, 100);
            this.txtUsername.Size = new Size(350, 40);
            this.txtUsername.Font = new Font("Microsoft YaHei", 18F);
            this.txtUsername.Text = "admin";

            // lblPassword
            this.lblPassword.Text = "密  码";
            this.lblPassword.Location = new Point(50, 180);
            this.lblPassword.Size = new Size(120, 40);
            this.lblPassword.Font = new Font("Microsoft YaHei", 18F);
            this.lblPassword.ForeColor = Color.FromArgb(0, 51, 102);
            this.lblPassword.TextAlign = ContentAlignment.MiddleRight;

            // txtPassword
            this.txtPassword.Location = new Point(180, 180);
            this.txtPassword.Size = new Size(350, 40);
            this.txtPassword.Font = new Font("Microsoft YaHei", 18F);
            this.txtPassword.PasswordChar = '*';

            // chkAutoLogin
            this.chkAutoLogin.Text = "自动登录";
            this.chkAutoLogin.Location = new Point(250, 250);
            this.chkAutoLogin.Font = new Font("Microsoft YaHei", 11F);
            this.chkAutoLogin.AutoSize = true;

            // btnLogin
            this.btnLogin.Text = "登录";
            this.btnLogin.Location = new Point(220, 300);
            this.btnLogin.Size = new Size(200, 60);
            this.btnLogin.Font = new Font("Microsoft YaHei", 16F);
            this.btnLogin.BackColor = Color.White;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.Click += BtnLogin_Click;

            // lblAuthor
            this.lblAuthor.Text = "著作: carryYIN";
            this.lblAuthor.Location = new Point(450, 420);
            this.lblAuthor.Size = new Size(200, 30);
            this.lblAuthor.Font = new Font("Microsoft YaHei", 10F, FontStyle.Italic);
            this.lblAuthor.ForeColor = Color.Gray;
            this.lblAuthor.TextAlign = ContentAlignment.MiddleRight;

            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.chkAutoLogin);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.lblAuthor);
            this.AcceptButton = this.btnLogin;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string savedPw = DatabaseHelper.GetLoginPassword();

            // First run setup
            if (string.IsNullOrEmpty(savedPw))
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("首次运行，请设置您的登录密码！", "设置密码", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                DatabaseHelper.SetLoginPassword(txtPassword.Text);
                MessageBox.Show("密码设置成功，请牢记您的密码！", "设置成功");
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }

            if (txtUsername.Text == "admin" && txtPassword.Text == savedPw)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("用户名或密码错误！", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
