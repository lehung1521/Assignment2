using B2BWholesale.Data.Services;

namespace BackOffice.WinForms
{
    public partial class AgentForm : Form
    {
        private readonly IAgentManagementService _agentService;
        private readonly AgentRecord? _existingAgent;

        public AgentForm(IAgentManagementService agentService, AgentRecord? existingAgent = null)
        {
            InitializeComponent();
            _agentService = agentService;
            _existingAgent = existingAgent;
            this.Text = existingAgent == null ? "Add New Agent" : "Edit Agent";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void AgentForm_Load(object sender, EventArgs e)
        {
            if (_existingAgent != null)
            {
                txtAgentName.Text = _existingAgent.AgentName;
                txtAddress.Text = _existingAgent.Address;
                txtPhone.Text = _existingAgent.Phone;
            }
            txtAgentName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var input = new AgentInput(txtAgentName.Text, txtAddress.Text, txtPhone.Text);
                if (_existingAgent is null)
                    _agentService.CreateAgent(input);
                else
                    _agentService.UpdateAgent(_existingAgent.AgentId, input);

                MessageBox.Show(_existingAgent is null ? "Agent added successfully." : "Agent updated successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving agent: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

    }
}
