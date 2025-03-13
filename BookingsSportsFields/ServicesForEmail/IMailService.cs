using BookingsSportsFields.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingsSportsFields.Application.ServicesForEmail
{
    public interface IMailService
    {
        bool SendMail(MailData Mail_Data);
    }
}
