using BookingsSportsFields.Application.ServicesForEmail;
using BookingsSportsFields.Core.Model;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookingsSportsFields.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly Application.ServicesForEmail.IMailService _mailService;

        public MailController(Application.ServicesForEmail.IMailService mailService)
        {
            _mailService = mailService;
        }

        [HttpPost]
        [Route("SendMail")]
        public bool SendMail(MailData mailData) 
        {
            return _mailService.SendMail(mailData);
        }
    }
}
