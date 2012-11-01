using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace SendMail
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new SmtpClient();
            client.Host = args[0];

            var message = new MailMessage();
            message.From = new MailAddress("buildserver@lanxum.com");
            message.To.Add(new MailAddress(args[1]));

            message.IsBodyHtml = false;
            message.Subject = args[2];

            message.Body = args[3];
            client.Send(message);
        }
    }
}
