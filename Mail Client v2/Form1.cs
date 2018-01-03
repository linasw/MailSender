using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mail_Client_v2
{
    public partial class Form1 : Form
    {
        SMTPMailSender sm = new SMTPMailSender();
        int ffsport;

        public Form1()
        {
            InitializeComponent();
            cbPort.SelectedIndex = 1;
        }

        private void bSend_Click(object sender, EventArgs e)
        {
            sm.SenderEmail = lFromEmail.Text;
            sm.RecipientEmail = "<" + lToEmail.Text + ">";
            //sm.SMTPServerAdress = "aspmx.l.google.com";
            sm.SenderName = lFromName.Text;
            sm.RecipientName = lToName.Text;
            sm.EmailSubject = lSubject.Text;
            sm.EmailBody = lMessage.Text;
            sm.RecipientPassword = tbPassword.Text.ToString();
            int.TryParse(cbPort.SelectedItem.ToString(),out ffsport);
            Console.WriteLine(ffsport);
            sm.SMTPRemotePort = ffsport;
            
            if (ffsport == 25)
            {
                sm.SMTPServerAdress = "aspmx.l.google.com";
                if (sm.SendEmail())
                    MessageBox.Show("Email has been sent successfully");
                else
                    MessageBox.Show("Error occured");
            }

            if (ffsport == 465)
            {
                sm.SMTPServerAdress = "smtp.gmail.com";
                if (sm.SendEmailSSL())
                    MessageBox.Show("Email has been sent successfully");
                else
                    MessageBox.Show("Error occured");
            }
        }

        private void cbPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbPort.SelectedItem.ToString().Equals("25"))
            {
                tbPassword.Enabled = false;
            }
            else
                tbPassword.Enabled = true;
        }
    }
}
