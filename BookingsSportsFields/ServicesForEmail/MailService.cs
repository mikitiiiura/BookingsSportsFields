using BookingsSportsFields.Core.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
//using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BookingsSportsFields.Application.ServicesForEmail
{

    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public bool SendMail(MailData mailData)
        {
            try
            {
                using (var client = new System.Net.Mail.SmtpClient(_mailSettings.Host, _mailSettings.Port))
                {
                    client.Credentials = new NetworkCredential(_mailSettings.UserName, _mailSettings.Password);
                    client.EnableSsl = _mailSettings.UseSSL;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_mailSettings.EmailId, _mailSettings.Name),
                        Subject = mailData.EmailSubject,
                        Body = mailData.EmailBody,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(new MailAddress(mailData.EmailToId, mailData.EmailToName));

                    client.Send(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Логуйте помилку (наприклад, через ILogger)
                Console.WriteLine($"Помилка при відправці листа: {ex.Message}");
                Console.WriteLine("LOX");
                return false;
            }
        }
    }


    //public class MailService : IMailService
    //{
    //    private readonly MailSettings _mailSettings;

    //    public MailService(IOptions<MailSettings> mailSettings)
    //    {
    //        _mailSettings = mailSettings.Value;
    //    }

    //    public bool SendMail(MailData mailData)
    //    {
    //        try
    //        {
    //            var email = new MimeMessage();
    //            email.From.Add(new MailboxAddress(_mailSettings.Name, _mailSettings.EmailId));
    //            email.To.Add(new MailboxAddress(mailData.EmailToName, mailData.EmailToId));
    //            email.Subject = mailData.EmailSubject;

    //            var bodyBuilder = new BodyBuilder
    //            {
    //                HtmlBody = mailData.EmailBody
    //            };

    //            email.Body = bodyBuilder.ToMessageBody();

    //            using var smtp = new SmtpClient();
    //            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
    //            smtp.Authenticate(_mailSettings.UserName, _mailSettings.Password);
    //            smtp.Send(email);
    //            smtp.Disconnect(true);

    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            // Логуйте помилку (наприклад, через ILogger)
    //            Console.WriteLine($"Помилка при відправці листа: {ex.Message}");
    //            return false;
    //        }
    //    }
    //}




















    //cтаре
    //public class MailService : IMailService
    //{
    //    MailSettings Mail_Settings = null;
    //    public MailService(IOptions<MailSettings> options)
    //    {
    //        Mail_Settings = options.Value;
    //    }
    //    public bool SendMail(MailData Mail_Data)
    //    {
    //        try
    //        {
    //            //MimeMessage - a class from Mimekit
    //            MimeMessage email_Message = new MimeMessage();
    //            MailboxAddress email_From = new MailboxAddress(Mail_Settings.Name, Mail_Settings.EmailId);
    //            email_Message.From.Add(email_From);
    //            MailboxAddress email_To = new MailboxAddress(Mail_Data.EmailToName, Mail_Data.EmailToId);
    //            email_Message.To.Add(email_To);
    //            email_Message.Subject = Mail_Data.EmailSubject;
    //            BodyBuilder emailBodyBuilder = new BodyBuilder();
    //            emailBodyBuilder.TextBody = Mail_Data.EmailBody;
    //            email_Message.Body = emailBodyBuilder.ToMessageBody();
    //            //this is the SmtpClient class from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
    //            SmtpClient MailClient = new SmtpClient();
    //            MailClient.Connect(Mail_Settings.Host, Mail_Settings.Port, Mail_Settings.UseSSL);
    //            MailClient.Authenticate(Mail_Settings.EmailId, Mail_Settings.Password);
    //            MailClient.Send(email_Message);
    //            MailClient.Disconnect(true);
    //            MailClient.Dispose();
    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            // Exception Details
    //            return false;
    //        }
    //    }
    //}
}
