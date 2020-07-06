using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class MailHelper : IMailHelper
    {
        IConfiguration configuration;
        public MailHelper(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void SendMail(string to, string subject, string content)
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Employee Management", "skbk1996@gmail.com"));
            //message.To.Add(new MailboxAddress("Bittu", "skbk404@gmail.com"));
            message.To.Add(new MailboxAddress("Saurav Kumar Shah",to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();

            var htmlContent1 = "<p>Welcome to <b>Employee Management Service</b> click the button below to verify your Email address.</p>";
            var htmlContent2 = "<a href = '" + content + "' class='btn btn-primary'" +
                " style='text-decoration:none;background-color: blue;color:white;padding:14px 25px;text-align: center;text-decoration: none;display: inline-block;'>" +
                "Click Me</a>";



            bodyBuilder.HtmlBody = htmlContent1 + htmlContent2;

            //bodyBuilder.Attachments.Add("C:/Users/SAURAV KUMAR/Downloads/pie.jpg");


            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("skbk1996@gmail.com", "9031141170s");
                client.Send(message);
                client.Disconnect(true);
            }



            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        }
    }
}
