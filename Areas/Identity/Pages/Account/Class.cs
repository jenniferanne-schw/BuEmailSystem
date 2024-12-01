using System;

namespace FinalProject.Models
{
    public class Email
    {
        public int Id { get; set; }  // Primary key
        public string To { get; set; }  // Recipient email
        public string Subject { get; set; }  // Subject of the email
        public string Body { get; set; }  // Body content of the email
        public DateTime DateSent { get; set; }  // Date when the email was "sent"
    }
}
