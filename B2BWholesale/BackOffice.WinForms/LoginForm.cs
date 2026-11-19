using B2BWholesale.Data.Models;
using B2BWholesale.Data.Services;

namespace BackOffice.WinForms
{
    public partial class LoginForm : Form
    {
        private readonly IAdminAuthenticationService _authenticationService;
        private readonly Func<User, Form> _adminFormFactory;

        public LoginForm(IAdminAuthenticationService authenticationService, Func<User, Form> adminFormFactory)
        {
            InitializeComponent();
            _authenticationService = authenticationService;
            _adminFormFactory = adminFormFactory;
            this.Text = "B2B Wholesale - Admin Login";
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            txtUsername.Focus();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var result = _authenticationService.AuthenticateAdmin(txtUsername.Text, txtPassword.Text);
                switch (result.Status)
                {
                    case AdminLoginStatus.MissingCredentials:
                        MessageBox.Show("Please enter both username/email and password.", "Input Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    case AdminLoginStatus.InvalidCredentials:
                        var message = result.AttemptsRemaining > 0
                            ? $"Incorrect password. {result.AttemptsRemaining} attempt(s) remaining."
                            : "Username/email or password is incorrect.";
                        MessageBox.Show(message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    case AdminLoginStatus.AccountLocked:
                        MessageBox.Show("This account is locked. Please contact the administrator.",
                            "Account Locked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    case AdminLoginStatus.AgentAccessDenied:
                        MessageBox.Show("Access denied. Only Admin users can access the back-office.",
                            "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    case AdminLoginStatus.Success when result.User is not null:
                        _adminFormFactory(result.User).Show();
                        this.Hide();
                        return;
                    default:
                        MessageBox.Show("Unable to sign in.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                btnLogin_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
        }
    }
}
