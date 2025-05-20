using DocumentsProject.Core.Domain.Models;
using DocumentsProject.Core.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Services
{
    public class MailService: IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly DocumentdbContext _dbContext;

        public MailService(IConfiguration configuration, DocumentdbContext DocumentdbContext)
        {
            _configuration = configuration;
            _dbContext = DocumentdbContext;
        }

        public async Task<bool> SendMail(string toEmail, string forWhat)
        {
            try
            {
                string otp = GenerateRandomOTP();
                var setOtp = setOTPinDB(toEmail, otp);

                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.EnableSsl = true;
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("tsrc060@gmail.com", "suvkqzfuqjnxuhmq");
                
                if(forWhat == "ForgetPassword")
                {
                    smtp.Send("tsrc060@gmail.com", toEmail, "Documents Project - Recover your password", $"To recover your account use this OTP : {otp}");
                }
                else
                {
                    smtp.Send("tsrc060@gmail.com", toEmail, "Documents Project - Sign Up", $"To create your account use this OTP : {otp}");
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool setOTPinDB(string userEmail, string otp)
        {
            try
            {
                var userdata = _dbContext.Otptables.FirstOrDefault(u => u.UserEmail == userEmail);
                if (userdata == null)
                {
                    Otptable data = new Otptable()
                    {
                        UserEmail = userEmail,
                        Otp = otp,
                        OtpCreatedTime = DateTime.Now,
                    };
                    _dbContext.Otptables.Add(data);
                    _dbContext.SaveChanges();
                    return true;
                }
                userdata.Otp = otp;
                userdata.OtpCreatedTime = DateTime.Now;
                _dbContext.Update(userdata);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string GenerateRandomOTP()
        {
            Random random = new Random();

            string randomDigits = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                randomDigits += random.Next(0, 10).ToString();
            }

            return randomDigits;

        }
    }
}
