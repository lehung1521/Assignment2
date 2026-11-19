using B2BWholesale.Data.Services;

namespace BackOffice.WinForms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            using (var dbContext = AppConfig.CreateDbContext())
            {
                try
                {
                    if (!dbContext.Database.CanConnect())
                    {
                        MessageBox.Show("Cannot connect to B2BWholesaleDB. Run Database/B2BWholesaleDB.sql and verify App.config.",
                            "Database Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database initialization failed: {ex.Message}", "Database Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var authenticationService = new AdminAuthenticationService(dbContext);
                var inventoryService = new InventoryService(dbContext);
                var agentService = new AgentManagementService(dbContext);
                var orderService = new OrderManagementService(dbContext);
                var manifestService = new ShippingManifestService(dbContext);
                var manifestPdfGenerator = new ShippingManifestPdfGenerator();

                Application.Run(new LoginForm(
                    authenticationService,
                    user => new MainForm(
                        inventoryService,
                        agentService,
                        orderService,
                        manifestService,
                        manifestPdfGenerator,
                        user)));
            }
        }
    }
}