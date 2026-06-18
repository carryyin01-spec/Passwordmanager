using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace PasswordManagerApp.Forms
{
    public class RandomPasswordForm : Form
    {
        private TextBox txtLength = null!;
        private CheckBox chkLower = null!;
        private CheckBox chkUpper = null!;
        private CheckBox chkNumbers = null!;
        private CheckBox chkSpecial = null!;
        private TextBox txtPrefix = null!;
        private TextBox txtSuffix = null!;
        private TextBox txtResult = null!;
        private TextBox txtMd5 = null!;
        private Button btnGenerate = null!;
        private Button btnCopy = null!;

        public RandomPasswordForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "随机密码生成工具";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Font mainFont = new Font("Microsoft YaHei", 12F);

            // Left Panel elements
            Label lblLength = new Label { Text = "生成长度:", Location = new Point(40, 50), Size = new Size(100, 30), Font = mainFont };
            txtLength = new TextBox { Text = "8", Location = new Point(140, 45), Size = new Size(200, 30), Font = mainFont };
            
            chkLower = new CheckBox { Text = "含小写字母(abc...z)", Location = new Point(40, 110), Size = new Size(300, 30), Font = mainFont, Checked = true };
            chkUpper = new CheckBox { Text = "含大写字母(ABC...Z)", Location = new Point(40, 150), Size = new Size(300, 30), Font = mainFont, Checked = true };
            chkNumbers = new CheckBox { Text = "含数字(0123...9)", Location = new Point(40, 190), Size = new Size(300, 30), Font = mainFont, Checked = true };
            chkSpecial = new CheckBox { Text = "含特殊符号(~!@#$..)", Location = new Point(40, 230), Size = new Size(300, 30), Font = mainFont, Checked = false };

            Label lblPrefix = new Label { Text = "增加前缀:", Location = new Point(40, 290), Size = new Size(100, 30), Font = mainFont };
            txtPrefix = new TextBox { PlaceholderText = "选填", Location = new Point(140, 285), Size = new Size(200, 30), Font = mainFont };

            Label lblSuffix = new Label { Text = "增加后缀:", Location = new Point(40, 340), Size = new Size(100, 30), Font = mainFont };
            txtSuffix = new TextBox { PlaceholderText = "选填", Location = new Point(140, 335), Size = new Size(200, 30), Font = mainFont };

            btnGenerate = new Button { Text = "生成", Location = new Point(40, 400), Size = new Size(100, 40), Font = mainFont, BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGenerate.Click += BtnGenerate_Click;

            btnCopy = new Button { Text = "复制密码", Location = new Point(160, 400), Size = new Size(120, 40), Font = mainFont, BackColor = Color.FromArgb(253, 126, 20), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCopy.Click += BtnCopy_Click;

            // Right Panel elements
            Label lblResult = new Label { Text = "密码:", Location = new Point(400, 50), Size = new Size(80, 30), Font = mainFont };
            txtResult = new TextBox { Location = new Point(480, 45), Size = new Size(260, 30), Font = mainFont, ReadOnly = true };

            Label lblMd5 = new Label { Text = "MD5:", Location = new Point(400, 110), Size = new Size(80, 30), Font = mainFont };
            txtMd5 = new TextBox { Location = new Point(480, 105), Size = new Size(260, 30), Font = mainFont, ReadOnly = true };

            this.Controls.AddRange(new Control[] {
                lblLength, txtLength,
                chkLower, chkUpper, chkNumbers, chkSpecial,
                lblPrefix, txtPrefix,
                lblSuffix, txtSuffix,
                btnGenerate, btnCopy,
                lblResult, txtResult,
                lblMd5, txtMd5
            });
        }

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtLength.Text, out int length) || length <= 0)
            {
                MessageBox.Show("请输入有效的生成长度！", "提示");
                return;
            }

            string lower = "abcdefghijklmnopqrstuvwxyz";
            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string numbers = "0123456789";
            string special = "~!@#$%^&*()_+`-=[]{}|;':\",./<>?";

            string charSet = "";
            if (chkLower.Checked) charSet += lower;
            if (chkUpper.Checked) charSet += upper;
            if (chkNumbers.Checked) charSet += numbers;
            if (chkSpecial.Checked) charSet += special;

            if (string.IsNullOrEmpty(charSet))
            {
                MessageBox.Show("请至少选择一种字符类型！", "提示");
                return;
            }

            Random rand = new Random();
            StringBuilder sb = new StringBuilder();
            
            // Optional: guarantee at least one char of each selected type
            int remainingLength = length;
            if (chkLower.Checked && remainingLength > 0) { sb.Append(lower[rand.Next(lower.Length)]); remainingLength--; }
            if (chkUpper.Checked && remainingLength > 0) { sb.Append(upper[rand.Next(upper.Length)]); remainingLength--; }
            if (chkNumbers.Checked && remainingLength > 0) { sb.Append(numbers[rand.Next(numbers.Length)]); remainingLength--; }
            if (chkSpecial.Checked && remainingLength > 0) { sb.Append(special[rand.Next(special.Length)]); remainingLength--; }

            for (int i = 0; i < remainingLength; i++)
            {
                sb.Append(charSet[rand.Next(charSet.Length)]);
            }

            // Shuffle the generated characters
            char[] passwordArray = sb.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                char temp = passwordArray[i];
                passwordArray[i] = passwordArray[j];
                passwordArray[j] = temp;
            }

            string generatedPassword = new string(passwordArray);
            
            // Add prefix and suffix
            string finalPassword = $"{txtPrefix.Text}{generatedPassword}{txtSuffix.Text}";
            
            txtResult.Text = finalPassword;
            txtMd5.Text = GetMd5Hash(finalPassword);
        }

        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtResult.Text))
            {
                Clipboard.SetText(txtResult.Text);
                MessageBox.Show("密码已复制到剪贴板！", "提示");
            }
        }

        private string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }
    }
}
