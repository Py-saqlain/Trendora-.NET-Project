using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Trendora.Models.ViewModels;
using Trendora.Services;

namespace Trendora.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailSender _emailSender;

        public ContactController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Prepare email content
                var subject = $"[Trendora Contact] {GetSubjectText(model.Subject)} - {model.FirstName} {model.LastName}";

                var message = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                    <h2 style='color: #333;'>New Contact Form Submission</h2>
                    
                    <table style='border-collapse: collapse; width: 100%;'>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; width: 150px;'><strong>Name:</strong></td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{model.FirstName} {model.LastName}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9;'><strong>Email:</strong></td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{model.Email}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9;'><strong>Phone:</strong></td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{model.Phone ?? "Not provided"}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9;'><strong>Subject:</strong></td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{GetSubjectText(model.Subject)}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9;'><strong>Newsletter:</strong></td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{(model.SubscribeToNewsletter ? "Subscribed" : "Not subscribed")}</td>
                        </tr>
                    </table>


                    <h3 style='color: #333; margin-top: 20px;'>Message:</h3>
                    <div style='background-color: #f5f5f5; padding: 15px; border-left: 4px solid #4361ee; margin: 10px 0;'>
                        {model.Message.Replace("\n", "<br>")}
                    </div>


                    <hr style='margin: 20px 0;'>
                    <p style='color: #666; font-size: 12px;'>
                        This message was sent from the Trendora contact form on {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}
                    </p>
                </body>
                </html>";

                // Send email to your address
                await _emailSender.SendEmailAsync(
                    "talhaleet@gmail.com",  // Changed parameter order to match IEmailSender interface
                    subject,
                    message
                );

                // Send auto-reply to the user
                var autoReplySubject = "Thank You for Contacting Trendora";
                var autoReplyMessage = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                    <h2 style='color: #333;'>Thank You for Contacting Trendora!</h2>
                    
                    <p>Dear {model.FirstName},</p>
                    
                    <p>Thank you for reaching out to us. We have received your message and will get back to you within 24-48 hours.</p>
                    
                    <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                        <strong>Reference Information:</strong><br>
                        • Subject: {GetSubjectText(model.Subject)}<br>
                        • Submitted on: {DateTime.Now.ToString("MMMM dd, yyyy HH:mm")}
                    </div>
                    
                    <p>In the meantime, you can:</p>
                    <ul>
                        <li>Visit our <a href='https://yourwebsite.com/faq'>FAQ page</a> for common questions</li>
                        <li>Check our <a href='https://yourwebsite.com/blog'>blog</a> for latest updates</li>
                        <li>Follow us on social media for news and promotions</li>
                    </ul>
                    
                    <p>Best regards,<br>
                    <strong>Trendora Customer Support Team</strong></p>
                    
                    <hr style='margin: 20px 0;'>
                    <p style='color: #666; font-size: 12px;'>
                        This is an automated response. Please do not reply to this email.
                    </p>
                </body>
                </html>";

                await _emailSender.SendEmailAsync(
                    model.Email,
                    autoReplySubject,
                    autoReplyMessage
                );

                TempData["SuccessMessage"] = "Thank you for your message! We've sent a confirmation to your email and will get back to you soon.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while sending your message. Please try again later. Error: {ex.Message}");
                return View(model);
            }
        }

        private string GetSubjectText(string subjectCode)
        {
            return subjectCode switch
            {
                "general" => "General Inquiry",
                "product" => "Product Question",
                "order" => "Order Issue",
                "return" => "Return & Exchange",
                "shipping" => "Shipping Information",
                "feedback" => "Feedback",
                "other" => "Other",
                _ => subjectCode
            };
        }
    }
}