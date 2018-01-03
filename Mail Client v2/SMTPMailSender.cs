using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net.Security;


namespace Mail_Client_v2
{
    class SMTPMailSender
    {
        string serverAdress;
        string senderName;
        string senderEmail;
        string recipientName;
        string recipientEmail;
        string subject;
        string body;
        string password;
        int timeOut;
        int remotePort;
        TcpClient tcpClient;
        NetworkStream netStream;
        StreamReader streamReader;
        StreamWriter streamWriter;
        DateTime timeOutCheck;
        SslStream ssl;
        
        public SMTPMailSender()
        {
            timeOut = 60;
            remotePort = 0;
        }

        public bool SendEmailSSL()
        {
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(serverAdress, remotePort);
            }
            catch
            {
                return false;
            }


            netStream = tcpClient.GetStream();
            streamWriter = new StreamWriter(netStream);
            streamReader = new StreamReader(netStream);


            if (remotePort == 465)
            {
                ssl = new SslStream(tcpClient.GetStream());
                ssl.AuthenticateAsClient(serverAdress);
                streamWriter = new StreamWriter(ssl);
                streamReader = new StreamReader(ssl);
            }


            if (WaitForResponse("220"))
            {
                //ssl.Write(Encoding.ASCII.GetBytes("HELO " + serverAdress));
                streamWriter.WriteLine("HELO " + serverAdress);
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("250"))
            {
                streamWriter.WriteLine("AUTH LOGIN");
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("334"))
            {
                streamWriter.WriteLine(Base64Encode(senderEmail));
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("334"))
            {
                streamWriter.WriteLine(Base64Encode(password));
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("235"))
            {
                streamWriter.WriteLine("mail from: " + "<" + senderEmail + ">");
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("250"))
            {
                streamWriter.WriteLine("rcpt to: " + recipientEmail);
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("250"))
            {
                streamWriter.WriteLine("data");
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("354"))
            {
                streamWriter.WriteLine("From: " + senderName + "<" + senderEmail + ">");
                streamWriter.WriteLine("To: " + recipientName + recipientEmail);
                streamWriter.WriteLine("Subject: " + subject);
                streamWriter.WriteLine();
                streamWriter.WriteLine(body);
                streamWriter.WriteLine(".");

                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("250"))
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("221"))
            {
                tcpClient.Close();
                return true;
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }
        }

        public bool SendEmail()
        {
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(serverAdress, remotePort);
            }
            catch
            {
                return false;
            }


            netStream = tcpClient.GetStream();
            streamWriter = new StreamWriter(netStream);
            streamReader = new StreamReader(netStream);

            if (WaitForResponse("220"))
            {
                //ssl.Write(Encoding.ASCII.GetBytes("HELO " + serverAdress));
                streamWriter.WriteLine("HELO " + serverAdress);
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("250"))
            {
                streamWriter.WriteLine("mail from: " + "<" + senderEmail + ">");
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("250"))
            {
                streamWriter.WriteLine("rcpt to: " + recipientEmail);
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("250"))
            {
                streamWriter.WriteLine("data");
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("354"))
            {
                streamWriter.WriteLine("From: " + senderName + "<" + senderEmail + ">");
                streamWriter.WriteLine("To: " + recipientName + recipientEmail);
                streamWriter.WriteLine("Subject: " + subject);
                streamWriter.WriteLine();
                streamWriter.WriteLine(body);
                streamWriter.WriteLine(".");

                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("250"))
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }

            if (WaitForResponse("221"))
            {
                tcpClient.Close();
                return true;
            }
            else
            {
                streamWriter.WriteLine("quit");
                streamWriter.Flush();
                tcpClient.Close();
                return false;
            }
        }

        bool WaitForResponse(string responseCode)
        {
            timeOutCheck = DateTime.Now;
            TimeSpan tsp = DateTime.Now - timeOutCheck;
            string incomingData = "";

            while (tsp.TotalSeconds < timeOut)
            {
                if (netStream.DataAvailable)
                {
                    incomingData = streamReader.ReadLine();
                    if (incomingData.Substring(0, responseCode.Length) == responseCode)
                    {
                        Console.WriteLine(incomingData);
                        while (netStream.DataAvailable)
                        {
                            incomingData = streamReader.ReadLine();
                            Console.WriteLine(incomingData);
                        }
                        return true;
                    }
                    else
                    {
                        Console.WriteLine(incomingData);
                        while (netStream.DataAvailable)
                        {
                            incomingData = streamReader.ReadLine();
                            Console.WriteLine(incomingData);
                        }
                        return false;
                    }
                }
                tsp = DateTime.Now - timeOutCheck;
            }
            return false;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string SMTPServerAdress
        {
            get { return serverAdress; }
            set { serverAdress = value; }
        }

        public string SenderName
        {
            get { return SenderName; }
            set { senderName = value; }
        }

        public string SenderEmail
        {
            get { return SenderEmail; }
            set { senderEmail = value; }
        }

        public string RecipientName
        {
            get { return RecipientName; }
            set { recipientName = value; }
        }

        public string RecipientPassword
        {
            get { return password; }
            set { password = value; }
        }

        public string RecipientEmail
        {
            get { return RecipientEmail; }
            set { recipientEmail = value; }
        }

        public string EmailSubject
        {
            get { return EmailSubject; }
            set { subject = value; }
        }

        public string EmailBody
        {
            get { return EmailBody; }
            set { body = value; }
        }

        public int SMTPTimeOut
        {
            get { return SMTPTimeOut; }
            set { timeOut = value; }
        }

        public int SMTPRemotePort
        {
            get { return SMTPRemotePort; }
            set { remotePort = value; }
        }
    }
}
