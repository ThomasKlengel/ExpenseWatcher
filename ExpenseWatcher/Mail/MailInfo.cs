using System;
using System.Text.RegularExpressions;
using MimeKit;
using MailKit;

namespace ExpenseWatcher.Mail
{
    public class MailInfo
    {
        public MailInfo(MimeMessage mail, UniqueId uid, IMailFolder containingFolder)
        {
            this.Mail = mail;
            this.UID = uid;
            this.ContainingFolder = containingFolder;
            this.PayPalPlainText = GetPayPalPlainText();
        }

        public MimeMessage Mail { get; private set; }

        public IMailFolder ContainingFolder { get; private set; }
        public DateTime Time
        {
            get
            {
                return Mail.Date != null ? Mail.Date.DateTime : new DateTime();
            }
        }

        public string PayPalPlainText { get; private set; }

        public UniqueId UID { get; private set; }

        public override bool Equals(object? obj)
        {
            if (obj is MailInfo info)
            {
                return Time.Equals(info.Time);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Time.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.UID} = {this.Time}: {this.PayPalPlainText}";
        }

        private string GetPayPalPlainText()
        {
            Regex untilBody = new Regex(@"[\S\s]*<body>");
            Regex html = new Regex(@"<[^>]*>");
            Regex whitespace = new Regex(@" {2,}");
            Regex nextline = new Regex(@" \n");
            Regex doubleline = new Regex(@"\n\n");
            //Regex fonts = new Regex(@"PayPal\sFonts[\s\S]*Styles\sfrom\sNeptune");
            //Regex secondFont = new Regex(@"PP\sSans\sFont\sImport[\s\S]*Outlook\sfrom\sdefaulting\sto\sTimes\sNew\sRoman");
            Regex multipleEmptyLines = new Regex(@"(\r\n\s?){2,}");

            var found = html.Match(this.Mail.HtmlBody).Value;
            var txt = untilBody.Replace(this.Mail.HtmlBody, string.Empty);
            txt = html.Replace(txt, " ");
            //if (fonts.IsMatch(txt))
            //{
            //    txt = fonts.Replace(txt, "");
            //}
            //if (secondFont.IsMatch(txt))
            //{
            //    txt = secondFont.Replace(txt, "");
            //}
            txt = txt.Replace("\t", string.Empty);
            while (whitespace.IsMatch(txt))
            {
                txt = whitespace.Replace(txt, " ");
            }
            while (nextline.IsMatch(txt))
            {
                txt = nextline.Replace(txt, "\n");
            }
            while (doubleline.IsMatch(txt))
            {
                txt = doubleline.Replace(txt, "\n");
            }
            txt = multipleEmptyLines.Replace(txt, "\r\n");
            return txt;
        }
    }
}
