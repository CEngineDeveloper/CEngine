//------------------------------------------------------------------------------
// Mail.cs
// Copyright 2020 2020/8/10 
// Created by CYM on 2020/8/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Net.Mail;
using System.Text;
using System.Net;
using System.IO;
using System.ComponentModel;
namespace CYM
{
    public class CMail 
    {
		static MailMessage message=null;
		static SmtpClient smtpClient = null;
		static Attachment attachment = null;
		static bool IsSending = false;
		static string AddressName = "1678184904@qq.com";
		static string DisplayName = "CYM";
		public static void Init(string displayName)
		{
			DisplayName = displayName;
		}
		public static void Send(string title,string desc,string attachFile=null)
		{
			if (IsSending)
				return;
			IsSending = true;
			//Mail 內容設定
			message = new MailMessage(new MailAddress(AddressName, DisplayName), new MailAddress(AddressName, DisplayName));//MailMessage(寄信者, 收信者)

			message.SubjectEncoding = Encoding.UTF8;    //標題編碼
			message.BodyEncoding = Encoding.UTF8;       //內容編碼

			message.Subject = DisplayName + "：" + title;           //E-mail主旨
			message.Body = desc;                  //E-mail內容

			if (attachFile!=null && File.Exists(attachFile))
			{
			    attachment = new Attachment(attachFile);//<-這是附件部分~先用附件的物件把路徑指定進去~
				message.Attachments.Add(attachment);//<-郵件訊息中加入附件
		
			}

            //mail server 內容設定
            
			smtpClient = new SmtpClient("smtp.qq.com", 25); //gmail smtp設定 port:587  SMTP: smtp.gmail.com
			smtpClient.Credentials = new NetworkCredential(AddressName, "jlkgpjgdvitobecf");//gmail 帳密    "hahamiror@gmail.com", "hahamiror123"																									
			smtpClient.EnableSsl = true;
			//完成寄信後的callback function
			smtpClient.SendCompleted += smtp_SendCompleted;

			//寄送mail
			smtpClient.SendAsync(message, null);//寄送

		}

		//完成寄信後的callback function
		static void smtp_SendCompleted(object sender, AsyncCompletedEventArgs e)
		{
			IsSending = false;
			attachment?.Dispose();
			smtpClient?.Dispose();
			message?.Attachments?.Dispose();
			message?.Dispose();
		}
	}
}