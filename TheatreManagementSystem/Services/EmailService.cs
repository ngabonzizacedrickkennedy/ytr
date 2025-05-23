using Microsoft.Extensions.Caching.Memory;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Cryptography;
using System.Text;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;
using TheatreManagementSystem.Services.Interfaces;
using AutoMapper;
namespace TheatreManagementSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Theatre Management System", _configuration["Email:From"]));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                message.Body = new TextPart("plain")
                {
                    Text = body
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(_configuration["Email:Host"], int.Parse(_configuration["Email:Port"]!), false);
                await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                // Log exception here
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string email, string otp)
        {
            var subject = "Your Theatre Management System Verification Code";
            var body = $"Your verification code is: {otp}\nThis code will expire in 10 minutes.";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string token)
        {
            var subject = "Password Reset Request - Theatre Management System";
            var body = $"To reset your password, click on the link below:\n\n" +
                      $"http://localhost:5173/reset-password?token={token}\n\n" +
                      $"This link will expire in 24 hours.\n\n" +
                      $"If you didn't request a password reset, please ignore this email.";

            return await SendEmailAsync(email, subject, body);
        }
    }
}
