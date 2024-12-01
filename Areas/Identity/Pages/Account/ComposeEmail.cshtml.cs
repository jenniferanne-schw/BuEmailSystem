using FinalProject.Data;  // Your DbContext
using FinalProject.Models;  // The Email model
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;  // Correct namespace for SqlParameter
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Areas.Identity.Pages.Account
{
    public class ComposeEmailModel : PageModel
    {
        private readonly FinalProjectContext _context;
        private readonly ILogger<ComposeEmailModel> _logger;

        [BindProperty]
        public string To { get; set; }

        [BindProperty]
        public string Subject { get; set; }

        [BindProperty]
        public string Body { get; set; }

        public ComposeEmailModel(FinalProjectContext context, ILogger<ComposeEmailModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // OnGet: You can use this to set up anything on page load
        public void OnGet()
        {
            // Any setup code can go here, if needed
        }

        // OnPostAsync: This method handles form submission
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // If validation fails, return the page with errors
            }

            try
            {
                // SQL query to insert email
                string insertEmailQuery = @"
                    INSERT INTO Emails (emailreceiver, Subject, Body, DateSent, EmailSender, ReadStatus) 
                    VALUES (@To, @Subject, @Body, @DateSent, @EmailSender, @ReadStatus)";  // Fixed column list

                // Define parameters to prevent SQL injection
                var parameters = new[]
                {
                    new SqlParameter("@To", To),
                    new SqlParameter("@Subject", Subject),
                    new SqlParameter("@Body", Body),
                    new SqlParameter("@DateSent", DateTime.UtcNow.AddHours(7)),
                    new SqlParameter("@EmailSender", User.Identity.Name),
                    new SqlParameter("@ReadStatus", "0")  // Mark as unread
                };

                // Create SQL connection
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(insertEmailQuery, connection))
                    {
                        command.Parameters.AddRange(parameters);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Step 3: Set TempData message and redirect
                TempData["Message"] = "Email sent successfully!";
                return RedirectToPage("/Index"); // Redirect to the email list page
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the process
                _logger.LogError(ex, "An error occurred while sending the email.");
                TempData["ErrorMessage"] = "An error occurred while sending the email.";
                return RedirectToPage("/Error");
            }
        }
    }
}