using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;

namespace SinExWebApp20256461
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //Instantiate a new MailMessage instance 
                MailMessage mailMessage = new MailMessage();
                //Add recipients 
                mailMessage.To.Add("gqi@ust.hk");
                mailMessage.To.Add("xduac@ust.hk");
                mailMessage.To.Add("zyuaf@ust.hk");
                mailMessage.To.Add("swuai@ust.hk");

                //Setting the displayed email address and display name
                mailMessage.From = new MailAddress("comp3111_team108@cse.ust.hk","Team 108 Xinnan");

                //Subject and content of the email
                mailMessage.Subject = "We love Xinnan";
                mailMessage.Body = "We have fixed this freaking email server!!! yay!!!";
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.High;

                //Instantiate a new SmtpClient instance
                SmtpClient smtpClient = new SmtpClient("smtp.cse.ust.hk");
                
              //  smtpClient.Credentials = new System.Net.NetworkCredential("comp3111_team108@cse.ust.hk", "team108#");
        
                //Send
                smtpClient.Send(mailMessage);
                Response.Write("Email Sent!!! Yay!");
            }

            catch (Exception ex)
            {
                Response.Write("gg, Could not send the email -error" + ex.Message);
            }

        }
    }
}