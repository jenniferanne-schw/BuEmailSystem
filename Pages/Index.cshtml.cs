using FinalProject.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Pages
{
    public class IndexModel : PageModel
    {
        // Changed from listEmails to ListEmails for consistency with the property
        public List<EmailInfo> ListEmails { get; set; } = new List<EmailInfo>();

        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // Changed the return type from void to Task to properly handle async operations
        public async Task OnGet()
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(); // Ensured the connection opens asynchronously

                    string username = User.Identity?.Name ?? ""; // Simplified username assignment

                    // SQL query using parameterized query to prevent SQL injection
                    string sql = "SELECT * FROM Emails WHERE emailreceiver = @EmailReceiver";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EmailReceiver", username); // Safe parameter binding

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            // Clear the list before adding items
                            ListEmails.Clear();

                            while (await reader.ReadAsync())
                            {
                                // Read each row and map it to the EmailInfo model
                                EmailInfo emailInfo = new EmailInfo
                                {
                                    EmailID = reader.GetInt32(0).ToString(),
                                    EmailSubject = reader.GetString(2),
                                    EmailMessage = reader.GetString(3),
                                    EmailDate = reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss"),
                                    EmailIsRead = reader.GetInt32(6).ToString(),
                                    EmailSender = reader.GetString(5),
                                    EmailReceiver = reader.GetString(1)
                                };

                                ListEmails.Add(emailInfo); // Add to the ListEmails
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error to the logger, so you can see it in the console or logs
                _logger.LogError($"Error loading emails: {ex.Message}");
            }
        }
        public async Task<IActionResult> OnPostDeleteEmail(int emailid)
        {
            try
            {
                String connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // SQL to delete the email by ID
                    string sql = "DELETE FROM Emails WHERE Id = @EmailID";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EmailID", emailid);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return RedirectToPage();  // Redirect to the inbox after deletion
            }
            catch (Exception ex)
            {
                // Handle any errors
                Console.WriteLine(ex.ToString());
                return RedirectToPage();  // Optionally, you can show an error message here
            }
        }
        public async Task<IActionResult> OnPostMarkAsRead(int emailid)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // SQL to update the email's read status to '1'
                    string sql = "UPDATE Emails SET ReadStatus = 1 WHERE Id = @EmailID";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EmailID", emailid);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return RedirectToPage();  // Refresh the page after marking as read
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking email as read: {ex.Message}");
                return RedirectToPage();
            }
        }

    }

    // EmailInfo class with properties (encapsulation)
    public class EmailInfo
    {
        public string EmailID { get; set; }
        public string EmailSubject { get; set; }
        public string EmailMessage { get; set; }
        public string EmailDate { get; set; }
        public string EmailIsRead { get; set; }
        public string EmailSender { get; set; }
        public string EmailReceiver { get; set; }
    }
}