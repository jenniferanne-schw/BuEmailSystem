using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

public class ViewEmailModel : PageModel
{
    private readonly ILogger<ViewEmailModel> _logger;
    private readonly IConfiguration _configuration;

    public ViewEmailModel(ILogger<ViewEmailModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string EmailSubject { get; set; }
    public string EmailSender { get; set; }
    public string EmailDate { get; set; }
    public string EmailMessage { get; set; }

    public async Task OnGet(int emailid)
    {
        try
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // SQL to mark the email as read
                string sqlUpdate = "UPDATE Emails SET ReadStatus = 1 WHERE Id = @EmailID";

                // SQL to get the email details
                string sql = "SELECT * FROM Emails WHERE Id = @EmailID";

                // First, update the read status
                using (SqlCommand updateCommand = new SqlCommand(sqlUpdate, connection))
                {
                    updateCommand.Parameters.AddWithValue("@EmailID", emailid);
                    await updateCommand.ExecuteNonQueryAsync(); // Execute the update query
                }

                // Then, retrieve the email content
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EmailID", emailid);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Map the email data to the properties
                            EmailSubject = reader.GetString(2);  // Assuming EmailSubject is at index 2
                            EmailSender = reader.GetString(5);   // Assuming EmailSender is at index 5
                            EmailDate = reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss"); // EmailDate at index 4
                            EmailMessage = reader.GetString(3);  // EmailMessage at index 3
                        }
                        else
                        {
                            // Handle case where the email wasn't found
                            EmailSubject = "Email not found";
                            EmailMessage = "The email you're looking for could not be found.";
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception
            _logger.LogError($"Error retrieving email: {ex.Message}");
            EmailSubject = "Error";
            EmailMessage = "There was an error retrieving the email.";
        }
    }
}
