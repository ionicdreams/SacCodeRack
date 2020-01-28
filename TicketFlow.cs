//Written by Jason Phillip Zanghi
//IT Service Desk

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;

using System.Web;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

using MySql.Data.Common;
using MySql.Data.MySqlClient;
using Oracle.DataAccess.Client;

namespace TicketFlow
{
    public partial class TicketFlow : ServiceBase
    {
        const string dbConnStringIT = "Data Source=REGSUP; user id=itregionalsupport; password=ItF0RMS9O4;";
        const string dbConnStringKACE = "server=elkaceap01;User Id=R1;Persist Security Info=True;database=ORG1;password=box747";
        
        //These will be used to override the database operations in the event of a problem
        const string dbOverrideLocation = @"\\nsfs1\Software\Web\IT_webapps\tf_flag.txt";
        static int OVERRIDE_TYPE = 0;
        static bool OVERRIDE_ISACTIVE = false;

        MailAddress CodeMaster = new MailAddress("jzanghi@cityofsacramento.org");

        const string email_mnc = "IT-morningnetcheck@cityofsacramento.org";

        Thread oThread;
        long zTicks = 0;
        static int Test_SendState = 0; //for testing
        static int Test_OracleCount = 0;
        string Test_KaceTicketsFound = "";

        int ZeroEntryError = 0;

        public TicketFlow()
        {
            InitializeComponent();
        }

        public void TransInProg()
        {
            bool SentMNC = false;
            int ErrCount = 0;
            ServiceBase myBase = new ServiceBase();
            myBase.CanStop = false;

            FormDBTools R = new FormDBTools(dbConnStringIT, dbConnStringKACE, CodeMaster);

            while (true)
            {
                if (DateTime.Now.TimeOfDay.Hours >= 6 && DateTime.Now.TimeOfDay.Hours < 17 && !SentMNC)
                {
                    //SEND NET CHECK EMAIL
                    for (int X = 0; X < 5; X++)
                    {
                        try
                        {
                            if (TEST_OracleDB() && TEST_KaceDB())
                            {
                                SendEmail(904);
                                SentMNC = true;
                                break;
                            }
                        }
                        catch
                        {
                        }

                        Thread.Sleep(60000);
                    }

                    if (!SentMNC)
                    {
                        SendEmail(905);
                    }
                }
                else if (SentMNC && DateTime.Now.TimeOfDay.Hours >= 20)
                {
                    SentMNC = false;
                }

                if (DateTime.Now.TimeOfDay.Hours >= 6 && DateTime.Now.TimeOfDay.Hours <= 20)
                {
                    try
                    {
                        R.db_action_GetFormTickets(0);
                    }
                    catch (Exception E)
                    {
                        ErrCount++;

                        if (ErrCount > 5)
                        {
                            SendEmail(E.ToString());
                            ErrCount = 0;
                        }
                    }
                }
                else
                {
                    
                    Thread.Sleep(10000);
                    Thread.Sleep(10000);
                    Thread.Sleep(10000);
                    
                }

                //Thread.Sleep(5000);
            }



        }

        protected override void OnStart(string[] args)
        {
            try
            {
                if (TEST_EMAIL() && TEST_OracleDB() && TEST_KaceDB() && TEST_EmailToKace())
                {
                    try
                    {

                    }
                    catch
                    {

                    }
                    SendEmail(23);
                    oThread = new Thread(new ThreadStart(TransInProg));
                    oThread.Start();
                    SendEmail(2);
                }
                else
                {
                    SendEmail(24);
                }
            }
            catch
            {
                SendEmail("Could not open oThread on Line 90");
            }
        }

        protected override void OnStop()
        {
            SendEmail(1);

            try
            {
                string SourceLog = "Application";
                string SourceApp = "TicketFlow";

                EventLog.CreateEventSource(SourceApp, SourceLog);
                EventLog R = new System.Diagnostics.EventLog();
                R.Source = SourceApp;
                R.Log = SourceLog;
                R.WriteEntry("TicketFlow has been stopped by an external process.");
                oThread.Abort();
            }
            catch
            {

            }
        }

        public void SendEmail(int SelectMSG)
        {
            
            MailMessage genForm = new MailMessage();
            SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

            genForm.IsBodyHtml = true;

            genForm.To.Add(CodeMaster);


            switch (SelectMSG)
            {
                case 1:
                    //genForm.To.Add(new MailAddress(email_mnc));
                    genForm.Subject = "TicketFlow (IT Service Desk ticket responder) has stopped. Please restart this service on EWWEBCM01.";
                    genForm.Body = "TicketFlow form submission service has stopped on EWWEBCM01. Please restart this service to continue sending the IT Forms users a courtesy email containing the Kace ticket numbers for the forms they submitted.";

                    break;
                case 2:
                    genForm.Subject = "TicketFlow (IT Service Desk ticket responder) has been started at " + DateTime.Now.ToShortTimeString();
                    genForm.Body = "The process has been started.";
                    break;
                case 3:
                    genForm.Subject = "Oracle DB Test completed with count of " + Test_OracleCount.ToString() + " at " + DateTime.Now.ToShortTimeString();
                    genForm.Body = "Oracle DB Test completed.";
                    break;
                case 4:
                    genForm.Subject = "Email State Test completed with State of " + Test_SendState.ToString() + " at " + DateTime.Now.ToShortTimeString();
                    genForm.Body = "Email State Test completed.";
                    break;
                case 5:
                    genForm.Subject = "Email Kace [TICK:115855] completed at " + DateTime.Now.ToShortTimeString();
                    genForm.Body = "Email To Kace Test completed.";
                    break;
                case 6:
                    genForm.Subject = "Kace DB Test of [TICK:115855] completed at " + DateTime.Now.ToShortTimeString();
                    genForm.Body = "Kace DB Search of 'Permanent Test Ticket To Remain Open For ITFORMS' has Completed with the following tickets: " + Test_KaceTicketsFound;
                    break;
                case 23:
                    genForm.Subject = "All Init Tests Completed Successfully @ " + DateTime.Now.ToShortTimeString();
                    genForm.Body = "All Init Tests completed successfully, now starting initial thread to begin normal operation." + Test_KaceTicketsFound;
                    break;
                case 24:
                    genForm.Subject = "Init Tests Have Failed " + DateTime.Now.ToShortTimeString();
                    genForm.Body = "Not all init tests completed successfully, normal operation will not continue. Application aborting thread.";
                    break;
                case 100:
                    genForm.Subject = "Override Requested (aborted)" + DateTime.Now.ToShortTimeString();
                    genForm.Body = "Override entry detected, however, Authorization missing. Override aborted.";
                    break;
                case 101:
                    genForm.Subject = "Override Requested (Kace/Email)" + DateTime.Now.ToShortTimeString();
                    genForm.Body = "Override entry detected, Kace/Email alerts disabled while all pending database entries are processed.";
                    break;
                case 102:
                    genForm.Subject = "Override Requested (Database)" + DateTime.Now.ToShortTimeString();
                    genForm.Body = "Override entry detected, Database updates disabled while all pending database entries are processed. This override will halt this service, but Windows will restart service in five minutes repeating all alerts.";
                    break;
                case 904:
                    genForm.To.Add(new MailAddress(email_mnc));
                    genForm.Subject = "(GREEN) TicketFlow is Online, Database check complete " + DateTime.Now.ToShortTimeString();
                    genForm.Body = "TicketFlow is Online and the Database check completed successfully. If you are receiving this message after 06:00 hours it is because the service stopped and was restarted.";
                    break;
                case 905:
                    genForm.To.Add(new MailAddress(email_mnc));
                    genForm.Subject = "(RED) TicketFlow is Online, but Database check could not complete " + DateTime.Now.ToShortTimeString();
                    genForm.Body = "TicketFlow is Online, but the Database check did not complete after five minutes of trying. If you are receiving this message after 06:00 hours it is because the service stopped and was restarted.";
                    break;
                default:

                    break;

            }
            genForm.From = new MailAddress("noreply@cityofsacramento.org");
            sendClient.Send(genForm);
        }


        private void SendEmail(string oMessage)
        {
            
            //If it hasnt been at least 5 minutes... dont call me, ill call you
            if (zTicks == 0 || zTicks < (DateTime.Now.Ticks - (TimeSpan.TicksPerMinute * 5)) || zTicks > DateTime.Now.Ticks)
            {
                zTicks = DateTime.Now.Ticks;
                MailMessage genForm = new MailMessage();
                SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

                genForm.IsBodyHtml = true;

                genForm.To.Add(CodeMaster);

                genForm.Subject = "TicketFlow Message";
                genForm.Body = "TicketFlow (IT Service Desk ticket responder) service is having problems. [Returned Message: " + oMessage + "]";
                genForm.From = new MailAddress("noreply@cityofsacramento.org");
                sendClient.Send(genForm);
            }
            else
            {
                Thread.Sleep(60000);
            }
             
        }

        public bool TEST_OracleDB()
        {
            try
            {
                using (OracleConnection dbConnIT = new OracleConnection(dbConnStringIT))
                {
                    dbConnIT.Open();

                    OracleCommand dbComIT = new OracleCommand();
                    dbComIT.Connection = dbConnIT;
                    dbComIT.CommandText = "SELECT SUBDATE,TICKNUM,TICKTEXT,USEREMAIL,TICKSUBJECT,USERNOTIFY,USERTRANSACTION FROM ITINV_TICKETS WHERE (USERNOTIFY = 'PEND' OR USERNOTIFY = 'PROG')";
                    dbComIT.CommandType = CommandType.Text;
                    OracleDataReader dbReadIT = dbComIT.ExecuteReader();

                    Test_OracleCount = 0;
                    while (dbReadIT.Read())
                    {
                        Test_OracleCount++;
                    }

                    dbConnIT.Close();
                    dbComIT.Dispose();
                    dbReadIT.Dispose();
                    //SendEmail(3);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TEST_EmailToKace()
        {
            try
            {
                try
                {
                    MailMessage genForm = new MailMessage();
                    SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

                    genForm.IsBodyHtml = false;

                    genForm.To.Add(new MailAddress("ithelpdesk@cityofsacramento.org"));
                    //genForm.To.Add("jzanghi@cityofsacramento.org");

                    genForm.Subject = "RE: [TICK:115855] - Test Update for Ticket Lists";
                    genForm.Body = "Testing Tickets form transactions: ";

                    genForm.Body += "\r\n 115855 --> https://elkaceap01.cityofsacramento.org/adminui/ticket.php?ID=115855 ";


                    genForm.From = new MailAddress("noreply@cityofsacramento.org");
                    sendClient.Send(genForm);
                }

                catch (Exception E)
                {
                    SendEmail("LINE 350 -> SENDKACEUPDATE(string[] Tickets) -- " + E.ToString());
                }

                //SendEmail(5);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TEST_KaceDB()
        {
            try
            {
                using (MySqlConnection dbConnKace = new MySqlConnection("server=elkaceap01;User Id=R1;Persist Security Info=True;database=ORG1;password=box747"))
                {
                    string[] Test_AllTickets = new string[] { "" };

                    MySqlCommand dbCmdKace = dbConnKace.CreateCommand();
                    dbConnKace.Open();

                    dbCmdKace.CommandText = "SELECT HD_TICKET.ID, HD_TICKET.TITLE FROM HD_TICKET  WHERE (HD_TICKET.TITLE = 'Permanent Test Ticket To Remain Open For ITFORMS')";
                    MySqlDataReader dbReadKace = dbCmdKace.ExecuteReader();

                    while (dbReadKace.Read())
                    {
                        if (dbReadKace.GetValue(0).ToString() != "")
                        {
                            if (Test_AllTickets[0] != "")
                            {
                                string[] Theta = new string[Test_AllTickets.Length + 1];
                                Test_AllTickets.CopyTo(Theta, 0);
                                Theta[Test_AllTickets.Length] = dbReadKace.GetValue(0).ToString();
                                Test_AllTickets = Theta;
                            }
                            else
                            {
                                Test_AllTickets[0] = dbReadKace.GetValue(0).ToString();
                            }
                        }
                        else
                        {
                            //Didnt Find the Ticket
                        }
                    }
                    dbConnKace.Close();
                    dbCmdKace.Dispose();
                    dbReadKace.Dispose();

                    Test_KaceTicketsFound = "";
                    foreach (string X in Test_AllTickets)
                    {
                        Test_KaceTicketsFound += X + " ";
                    }

                    dbConnKace.Close();
                }

                //SendEmail(6);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool funct_Override_PROC()
        {
            int WhatDo = funct_Override_RET();
            switch (WhatDo)
            {
                case 0:
                    SendEmail(100);
                    break;
                case 1: //shutdown kace and email alerts
                    SendEmail(101);
                    break;
                case 2: //shutdown database updates
                    break;

            }
            return true;
        }

        public int funct_Override_RET()
        {
            //0 - decline, override text entered but authorization code is missing
            //1 - normal, no override
            //2 - override, disable kace interactions, disable user emails, and mark all processed
            //3 - override, disable all database interactions and just check kace

            string Proc_Override = File.ReadAllText(dbOverrideLocation);
            if (Proc_Override != "")
            {
                if (Proc_Override.Contains("Auth_Z@NGH!"))
                {
                    if (Proc_Override.Contains("overridekace"))
                    {
                        return 2;
                    }
                    if (Proc_Override.Contains("overridedb"))
                    {
                        return 3;
                    }
                }
                else
                {
                    //not authorized
                    return 0;
                }
            }
            return 1;
        }

        public bool TEST_OVERRIDE()
        {
            try
            {
                funct_Override_PROC();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TEST_EMAIL()
        {
            try
            {
                //SendFormEmail already completed but the mail message failed to send, second attempt
                SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

                sendClient.SendCompleted += new SendCompletedEventHandler(TEST_SendCompletedCallback);

                Test_SendState = 23;
                string userState = "";

                MailMessage genForm = new MailMessage();

                genForm.IsBodyHtml = true;
                genForm.To.Add(CodeMaster);
                genForm.Subject = "Email State Test Started";
                genForm.Body = "Email State Test Started";
                genForm.From = new MailAddress("noreply@cityofsacramento.org");

                sendClient.SendCompleted += new SendCompletedEventHandler(TEST_SendCompletedCallback);

                sendClient.SendAsync(genForm, userState);

                //SendEmail(4);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void TEST_SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                Test_SendState = 2;
            }
            if (e.Error != null)
            {
                Test_SendState = 3;
            }
            else
            {
                Test_SendState = 1;
            }
        }
    }

    public class FormDBTools
    {
        //MySqlConnection dbConnKace = new MySqlConnection("server=elkaceap01;User Id=R1;Persist Security Info=True;database=ORG1;password=box747")

        string dbConnStringIT = "";
        string dbConnStringKACE = "";
        MailAddress CodeMaster;

        static int SendState = 0; //0 = Not Sending, 1 = SENT, 2 = CANCELED
        int ZeroEntryError = 0;

        public FormDBTools(string dbConnStringIt, string dbConnStringKace, MailAddress CM_Pass)
        {
            dbConnStringIT = dbConnStringIt;
            dbConnStringKACE = dbConnStringKace;
            CodeMaster = CM_Pass;
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //CHECK FUNCTIONS TO CHECK DATA
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private bool Check_AllEqual(string[] CheckArray)
        {
            string R = CheckArray[0];

            foreach (string X in CheckArray)
            {
                if (R != X)
                    return false;
            }

            return true;
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                SendState = 2;
            }
            if (e.Error != null)
            {
                SendState = 3;
            }
            else
            {
                SendState = 1;
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // MAIN ACTIONS FOR DB
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~


        public void db_action_GetFormTickets(int ErrCount)
        {
            string[] Transactions = new string[] { "" };


            using (OracleConnection dbConnIT = new OracleConnection(dbConnStringIT))
            {
                dbConnIT.Open();

                OracleCommand dbComIT = new OracleCommand();
                dbComIT.Connection = dbConnIT;
                dbComIT.CommandText = "SELECT SUBDATE,TICKNUM,TICKTEXT,USEREMAIL,TICKSUBJECT,USERNOTIFY,USERTRANSACTION FROM ITINV_TICKETS WHERE (USERNOTIFY = 'PEND' OR USERNOTIFY = 'PROG')";
                dbComIT.CommandType = CommandType.Text;
                OracleDataReader dbReadIT = dbComIT.ExecuteReader();


                try
                {
                    ServiceBase myBase = new ServiceBase();
                    myBase.CanStop = false;

                    for (int X = 0; X < 10; X++)
                    {
                        myBase.RequestAdditionalTime(2000);
                        Thread.Sleep(2000);
                        //Look busy while we wait so windows doesnt think we are slacking
                    }
                }
                catch
                {
                }
                
                while (dbReadIT.Read())
                {
                    try
                    {
                        
                        string GetTrans = dbReadIT.GetValue(6).ToString();

                        if (Transactions[0] == "")
                        {
                            Transactions[0] = GetTrans;
                        }
                        else
                        {
                            bool found = false;
                            foreach (string R in Transactions)
                            {
                                if (R == GetTrans)
                                    found = true;
                            }

                            if (!found)
                            {
                                string[] Temp = new string[Transactions.Length + 1];
                                Transactions.CopyTo(Temp, 0);
                                Temp[Transactions.Length] = GetTrans;
                                Transactions = Temp;
                            }
                        }

                        db_subaction_SearchKace(dbReadIT.GetValue(4).ToString(), 1);
                         
                    }
                    catch (Exception E)
                    {
                        if (ErrCount > 10)
                        {
                            SendEmail(E.ToString());
                            return;
                        }
                        else
                        {
                            Thread.Sleep(5000);
                            db_action_GetFormTickets(ErrCount++);
                        }
                    }
                
                }

                dbConnIT.Close();
            }

            Thread.Sleep(2000);

            string temptemp = "";

            for (int Z = 0; Z < Transactions.Length; Z++)
            {
                if (Transactions[Z].Trim() != "")
                {
                    temptemp += Transactions[Z].Trim() + "\r\n";
                    db_subaction_CheckFormEmail(Transactions[Z].Trim());
                }
            }
        }

        private void SendEmail(string oMessage)
        {
            //If it hasnt been at least 5 minutes... dont call me, ill call you
            //if (zTicks == 0 || zTicks < (DateTime.Now.Ticks - (TimeSpan.TicksPerMinute * 5)) || zTicks > DateTime.Now.Ticks)
            //{
            //zTicks = DateTime.Now.Ticks;
            MailMessage genForm = new MailMessage();
            SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

            genForm.IsBodyHtml = true;

            genForm.To.Add(CodeMaster);

            genForm.Subject = "TicketFlow: -> Message from the code";
            genForm.Body = "TicketFlow (IT Service Desk ticket responder) service has sent a message.<br><br><br> [Returned Message: " + oMessage + "]";
            genForm.From = new MailAddress("noreply@cityofsacramento.org");
            sendClient.Send(genForm);
            //}
            //else
            //{
            //    Thread.Sleep(60000);
            //}
        }

        private void SendKaceUpdate(string[] Tickets, string[] Subjects)
        {
            //If it hasnt been at least 5 minutes... dont call me, ill call you
            //if (zTicks == 0 || zTicks < (DateTime.Now.Ticks - (TimeSpan.TicksPerMinute * 5)) || zTicks > DateTime.Now.Ticks)
            //{
            
            if (Tickets.Length > 1)
            {
                for (int X = 0; X < Tickets.Length; X++)
                {
                    try
                    {
                        if (Tickets[X].Trim() != "" && Tickets[X].Trim().Length >= 5)
                        {
                            MailMessage genForm = new MailMessage();
                            SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

                            genForm.IsBodyHtml = false;

                            genForm.To.Add(new MailAddress("ithelpdesk@cityofsacramento.org"));
                            //genForm.To.Add("jzanghi@cityofsacramento.org");

                            genForm.Subject = "RE: [TICK:" + Tickets[X].ToString() + "] - Update with Ticket List";
                            genForm.Body = "This form transaction consists of multiple tickets: ";

                            for (int R=0; R<Tickets.Length; R++) // each (string R in Tickets)
                            {
                                try
                                {
                                    Subjects[R] = Subjects[R].Substring(0, Subjects[R].IndexOf('['));
                                }
                                catch { }

                                if (Tickets[R].Trim() != "" && Tickets[R].Trim().Length >= 5)
                                {
                                    genForm.Body += "\r\n\r\n Ticket " + Tickets[R].Trim() + " : " + Subjects[R] + "\r\n --> https://elkaceap01.cityofsacramento.org/adminui/ticket.php?ID=" + Tickets[R].Trim() + " ";
                                }
                            } 

                            genForm.From = new MailAddress("noreply@cityofsacramento.org");
                            sendClient.Send(genForm);
                        }
                    }
                    catch (Exception E)
                    {
                        SendEmail("LINE 350 -> SENDKACEUPDATE(string[] Tickets) -- " + E.ToString());
                    }
                }
            }
        }

        private void db_action_SendFormEmail(string email, string[] Subjects, string[] Tickets)
        {
            const string SUBJECT = "Forms Recently Sent to IT for: ";
            const string BODY = "<HTML><BODY style=\"font-size:11pt;font-family:Calibri\">Hello,"
                + "<br /><br />The IT department has recently received a set of forms with this email given as the submitter. If that was you, thank you for using the IT forms."
                + "<br /><br />This email is a confirmation that we received the following forms, along with the ticket number generated for each form:"
                + "<br /><br />";
            const string FOOTER = "</BODY><?HTML>";

            string UserName = "";

            string Output = "<table style=\"font-size:11pt;font-family:Calibri\"><tr>"
                + "<td bgcolor=\"#DDDDDD\" width=\"75\"><u>Ticket</u></td>"
                + "<td bgcolor=\"#DDDDDD\" width=\"250\"><u>Form Submitted</u></td>"
                + "<td bgcolor=\"#DDDDDD\" width=\"100\">&nbsp;&nbsp;&nbsp;<u>Update Ticket Link:</u></td></tr>";

            string UserEmail = email;


            UserName = Subjects[0].Substring(Subjects[0].IndexOf("for: ") + 5);
            UserName = UserName.Substring(0, UserName.IndexOf(" ["));

            //Counter for tickets to see if any were actually added
            int ActualAdds = 0;

            //SendEmail("Form Build Period");
            for (int R = 0; R < Subjects.Length; R++)
            {
                //seems overboard, but this is STILL happening, blank tickets are somehow becoming part of the string.
                if (Tickets[R].Replace("  ", "") != "" || Tickets[R].Replace("  ", "") != " ")
                {
                    if (Tickets[R].Trim() != "")
                    {
                        try
                        {
                            Output += "<tr><td><strong>" + (Tickets[R].Trim() != "" ? Tickets[R] : "N/A") + "</strong></td>"
                                    + "<td>" + Subjects[R].Substring(0, Subjects[R].IndexOf("[")) + "</td>"
                                    + "<td>&nbsp;&nbsp;&nbsp;" + (Tickets[R].Trim() != "" ? "<a href=\"mailto:ithelpdesk@cityofsacramento.org?subject=RE: [TICK:" + Tickets[R] + "] - Update\">Send Update</a>" : "No Ticket Needed") + "</td></tr>";
                        }
                        catch
                        {
                            Output += "<tr><td><strong>" + (Tickets[R].Trim() != "" ? Tickets[R] : "N/A") + "</strong></td>"
                                    + "<td>" + Subjects[R] + "</td>"
                                    + "<td>" + "</td></tr>";
                        }
                        ActualAdds++;
                    }
                }
            }
            
            //SendEmail("Output Period");
            Output += "<tr><td colspan=\"3\"><br />As each ticket is closed, you should receive an automated email from our server, which indicates the work for that particular ticket is complete."
                + "<br />When all tickets are closed, your request is completed."
                + "<br /><br />If you feel that you received this email by mistake, or have any concerns about your tickets, please contact us at x7111."
                + "<br /><br />Thank you,<br />IT Service Desk</td></tr>";

            Output += "</table>";

            //Error detection - detect email floods and leaks
            if (ActualAdds == 0 || db_subaction_ErrorCheck_ByEmail("[" + UserEmail + "][" + UserName + "]" + Output))
            {
                ZeroEntryError++;

                if (ZeroEntryError > 2)
                {
                    //Error Correction Protocol, abort the database entries and restart.
                    db_subaction_ErrorCorrectionProtocol();
                }

            }
            else
            {
                ZeroEntryError = 0;
                //SendEmail("Message Period");
                MailMessage genForm = new MailMessage();
                SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

                genForm.IsBodyHtml = true;

                try
                {
                    genForm.To.Add(new MailAddress(UserEmail));
                    genForm.Subject = SUBJECT + UserName;
                    genForm.Body = BODY + Output + FOOTER;
                }
                catch
                {
                    genForm.To.Add(CodeMaster);
                    genForm.Subject = ("Error from the code:" + SUBJECT + UserName + " (" + UserEmail + ")");
                }

                genForm.Bcc.Add(CodeMaster);
                genForm.From = new MailAddress("noreply@cityofsacramento.org");

                sendClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);

                SendState = 23;
                string userState = "";
                sendClient.SendAsync(genForm, userState);

                for (int X = 0; X < 20; X++)
                {
                    Thread.Sleep(1500);
                    if (SendState == 1)
                    {
                        SendKaceUpdate(Tickets, Subjects);
                        db_subaction_UpdateFormDB(Subjects, 0);
                        break;
                    }
                    else if (SendState < 23)
                    {
                        //Email Failed, need to try again
                        db_action_SendFormEmail(genForm, Tickets, Subjects);
                    }
                }

                SendState = 0;
            }
        }

        private void db_action_SendFormEmail(MailMessage genForm, string[] Tickets, string[] Subjects)
        {
            //SendFormEmail already completed but the mail message failed to send, second attempt
            SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

            sendClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);

            SendState = 23;
            string userState = "";
            sendClient.SendAsync(genForm, userState);

            for (int X = 0; X < 20; X++)
            {
                Thread.Sleep(1500);
                if (SendState == 1)
                {
                    SendKaceUpdate(Tickets, Subjects);
                    db_subaction_UpdateFormDB(Subjects, 0);
                    break;
                }
                else if (SendState < 23)
                {
                    //Email Failed, need to try again
                    SendEmail("genForm Failed to Send (" + genForm.Subject + ")");
                }
            }

            SendState = 0;
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // SUB ACTIONS FOR DB
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private bool PauseThread(int xSeconds)
        {
            try
            {
                ServiceBase myBase = new ServiceBase();
                myBase.CanStop = false;

                for (int X = 0; X < xSeconds; X++)
                {
                    myBase.RequestAdditionalTime(2000);
                    Thread.Sleep(2000);
                    //Look busy while we wait so windows doesnt think we are slacking
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void db_subaction_SearchKace(string SearchBy, int zeroFal)
        {
            
            try
            {
                using (MySqlConnection dbConnKace = new MySqlConnection(dbConnStringKACE))
                {
                    dbConnKace.Open();

                    
                    string[] AllTickets = new string[] { "" };

                    MySqlCommand dbCmdKace = dbConnKace.CreateCommand();
                    dbCmdKace.CommandText = "SELECT HD_TICKET.ID, HD_TICKET.TITLE FROM HD_TICKET  WHERE (HD_TICKET.TITLE = '" + SearchBy + "')";

                    MySqlDataReader dbReadKace = dbCmdKace.ExecuteReader();

                    
                    while (dbReadKace.Read())
                    {
                        if (dbReadKace.GetValue(0).ToString() != "")
                        {
                            //zeroFal = 0;

                            if (AllTickets[0] != "")
                            {
                                string[] Theta = new string[AllTickets.Length + 1];
                                AllTickets.CopyTo(Theta, 0);
                                Theta[AllTickets.Length] = dbReadKace.GetValue(0).ToString();
                                AllTickets = Theta;
                            }
                            else
                            {
                                AllTickets[0] = dbReadKace.GetValue(0).ToString();
                            }
                        }
                        else
                        {
                            //Cant find it? Wait and cycle back
                            if (zeroFal <= 6)
                            {
                                bool uselessvar = PauseThread(10);
                                db_subaction_SearchKace(SearchBy, zeroFal++);
                                return;
                            }
                        }
                    }
                  
                    foreach (string RR in AllTickets)
                    {
                        db_subaction_UpdateFormDB(SearchBy, RR, 0);
                    }
                    

                    dbConnKace.Close();
                }
            }
            catch
            {
                if (zeroFal < 6)
                {
                    SendEmail("This is fail");
                    //db_subaction_SearchKace(SearchBy, zeroFal++);
                }
            }
             
        }


        private bool db_subaction_ErrorCheck_ByEmail(string emailtext)
        {
            string xPATH = "";

            try
            {
                xPATH = System.IO.Directory.GetCurrentDirectory();
            }
            catch
            {
                try
                {
                    xPATH = AppDomain.CurrentDomain.BaseDirectory;
                }
                catch
                {
                    SendEmail("Application directory could not be discovered.");
                    return false;
                }
            }
            
            xPATH += ((xPATH.Substring(xPATH.Length -1, 1) == @"\") ? "" : @"\");

            if (File.Exists(xPATH + "LRE1.txt"))
            {
                if (emailtext == File.ReadAllText(xPATH + "LRE1.txt"))
                {
                    return true;
                }
                else
                {
                    File.WriteAllText(xPATH + "LRE1.txt", emailtext);
                    return false;
                }
            }
            else
            {
                File.WriteAllText(xPATH + "LRE1.txt", emailtext);
                return false;
            }

        }

        private void db_subaction_ErrorCorrectionProtocol()
        {
            SendEmail("Errors in the email sequence have been detected, attempting to abort database PROG and PEND items");
            try
            {
                //abort all current transactions
                using (OracleConnection dbConnITNEST = new OracleConnection(dbConnStringIT))
                {
                    dbConnITNEST.Open();

                    OracleCommand dbCommNestIT = dbConnITNEST.CreateCommand();
                    OracleTransaction dbTransIT = dbConnITNEST.BeginTransaction(IsolationLevel.ReadCommitted);
                    dbCommNestIT.Transaction = dbTransIT;

                    dbCommNestIT.Parameters.Add("1", "SEND");
                    dbCommNestIT.CommandText = "update itinv_tickets set usernotify = :1 where(usernotify = 'PEND')";
                    dbCommNestIT.CommandType = CommandType.Text;
                    dbCommNestIT.ExecuteScalar();
                    dbTransIT.Commit();
                    SendEmail("PEND items aborted successfully");

                    try
                    {
                        dbCommNestIT.CommandText = "update itinv_tickets set usernotify = :1 where(usernotify = 'PROG')";
                        dbCommNestIT.CommandType = CommandType.Text;
                        dbCommNestIT.ExecuteScalar();
                        dbTransIT.Commit();
                        SendEmail("PROG items aborted successfully");
                    }
                    catch
                    {

                    }

                }
            }
            catch
            {
                //activate complete suppression protocol
                while (true)
                {
                    SendEmail("Database errors could not be corrected. Suppression Loop Activated, the code is now entering an infinite loop awaiting manual correction.");
                    SendEmail("Database errors could not be corrected. Suppression Loop Activated, the code is now entering an infinite loop awaiting manual correction.");
                    SendEmail("Database errors could not be corrected. Suppression Loop Activated, the code is now entering an infinite loop awaiting manual correction.");
                    SendEmail("Database errors could not be corrected. Suppression Loop Activated, the code is now entering an infinite loop awaiting manual correction.");
                    SendEmail("Database errors could not be corrected. Suppression Loop Activated, the code is now entering an infinite loop awaiting manual correction.");
                    
                    Thread.Sleep(System.Threading.Timeout.Infinite);
                }
            }


        }

        private void db_subaction_UpdateFormDB(string SearchBy, string TicketNumber, int zeroFal)
        {
            try
            {
                using (OracleConnection dbConnITNEST = new OracleConnection(dbConnStringIT))
                {
                    dbConnITNEST.Open();

                    OracleCommand dbCommNestIT = dbConnITNEST.CreateCommand();
                    OracleTransaction dbTransIT = dbConnITNEST.BeginTransaction(IsolationLevel.ReadCommitted);
                    dbCommNestIT.Transaction = dbTransIT;

                    try
                    {
                        dbCommNestIT.Parameters.Add("1", TicketNumber);
                        dbCommNestIT.Parameters.Add("2", "PROG");
                        dbCommNestIT.CommandText = "update itinv_tickets set TICKNUM = :1, USERNOTIFY = :2 where(TICKSUBJECT='" + SearchBy + "')";
                        dbCommNestIT.CommandType = CommandType.Text;
                        dbCommNestIT.ExecuteScalar();
                        dbTransIT.Commit();
                    }
                    catch (OracleException E)
                    {
                        dbTransIT.Rollback();
                    }

                    //dbCommNestIT.Dispose();
                    //dbTransIT.Dispose();
                    dbConnITNEST.Close();
                }
            }
            catch
            {
                if (zeroFal < 4)
                {
                    db_subaction_UpdateFormDB(SearchBy, TicketNumber, zeroFal++);
                }
            }
        }

        private void db_subaction_UpdateFormDB(string[] SearchBy, int zeroFal)
        {
            try
            {
                using (OracleConnection dbConnITNEST = new OracleConnection(dbConnStringIT))
                {
                    dbConnITNEST.Open();

                    foreach (string R in SearchBy)
                    {
                        //SendEmail("Updating Record");
                        OracleCommand dbCommNestIT = dbConnITNEST.CreateCommand();
                        OracleTransaction dbTransIT = dbConnITNEST.BeginTransaction(IsolationLevel.ReadCommitted);

                        try
                        {
                            dbCommNestIT.Transaction = dbTransIT;
                            dbCommNestIT.Parameters.Add("1", "SEND");
                            dbCommNestIT.CommandText = "update itinv_tickets set USERNOTIFY = :1 where(TICKSUBJECT='" + R + "')";
                            dbCommNestIT.CommandType = CommandType.Text;
                            dbCommNestIT.ExecuteScalar();
                            dbTransIT.Commit();
                        }
                        catch (OracleException E)
                        {
                            dbTransIT.Rollback();
                        }

                        dbCommNestIT.Dispose();
                        dbTransIT.Dispose();
                    }

                    dbConnITNEST.Close();
                }
            }
            catch
            {
                if (zeroFal < 4)
                {
                    db_subaction_UpdateFormDB(SearchBy, zeroFal++);
                }
            }
        }

        private void db_subaction_CheckFormEmail(string SearchBy)
        {
            using (OracleConnection dbConnIT = new OracleConnection(dbConnStringIT))
            {
                dbConnIT.Open();

                OracleCommand dbComIT = new OracleCommand();
                dbComIT.Connection = dbConnIT;
                dbComIT.CommandText = "SELECT SUBDATE,TICKNUM,TICKTEXT,USEREMAIL,TICKSUBJECT,USERNOTIFY,USERTRANSACTION FROM ITINV_TICKETS  WHERE (USERTRANSACTION='" + SearchBy + "')";
                dbComIT.CommandType = CommandType.Text;
                OracleDataReader dbReadIT = dbComIT.ExecuteReader();

                string[] Tickets = new string[] { "" };
                string[] Forms = new string[] { "" };
                string[] FormStatus = new string[] { "" };
                string UserEmail = "";

                bool OneFound = false;

                while (dbReadIT.Read())
                {
                    OneFound = true;
                    try
                    {
                        if (UserEmail == "")
                        {
                            UserEmail = dbReadIT.GetValue(3).ToString();
                            UserEmail = Regex.Replace(UserEmail.Substring(0, UserEmail.IndexOf('@')), "[^0-9a-zA-Z]+", "") + UserEmail.Substring(UserEmail.IndexOf('@'));
                        }

                        if (Forms[Forms.Length - 1] != "")
                        {
                            string[] AddTicket = new string[Tickets.Length + 1];
                            Tickets.CopyTo(AddTicket, 0);
                            AddTicket[Tickets.Length] = dbReadIT.GetValue(1).ToString();
                            Tickets = AddTicket;

                            string[] AddForms = new string[Forms.Length + 1];
                            Forms.CopyTo(AddForms, 0);
                            AddForms[Forms.Length] = dbReadIT.GetValue(4).ToString();
                            Forms = AddForms;

                            string[] AddStatus = new string[FormStatus.Length + 1];
                            FormStatus.CopyTo(AddStatus, 0);
                            AddStatus[FormStatus.Length] = dbReadIT.GetValue(5).ToString();
                            FormStatus = AddStatus;
                        }
                        else
                        {
                            Tickets[Tickets.Length - 1] = dbReadIT.GetValue(1).ToString();
                            Forms[Forms.Length - 1] = dbReadIT.GetValue(4).ToString();
                            FormStatus[FormStatus.Length - 1] = dbReadIT.GetValue(5).ToString();
                        }
                    }
                    catch (Exception E)
                    {
                        SendEmail("Unable to complete dbReadIT on " + SearchBy);
                    }
                }

                if (!OneFound)
                {
                    SendEmail("Unable to Find " + SearchBy);
                }

                if (Check_AllValid(FormStatus, Tickets) && Forms.Length == Tickets.Length)
                {
                    try
                    {
                        //time to send out our email containing all the ticket numbers
                        //SendEmail("Starting Send Period");
                        db_action_SendFormEmail(UserEmail, Forms, Tickets);
                        //Thread.Sleep(999999);
                    }
                    catch (Exception E)
                    {
                        SendEmail("Unable to send email, line 639" + "\r\n\r\n" + E.ToString());
                    }
                }

                dbConnIT.Close();
            }
        }

        bool Check_AllValid(string[] FormStatus, string[] Tickets)
        {
            if (FormStatus.Length != Tickets.Length)
            {
                return false;
            }
            else
            {
                for (int X = 0; X < FormStatus.Length; X++)
                {
                    if (FormStatus[X] != "DNE")
                    {
                        if (FormStatus[X] == "PROG")
                        {
                            if (Tickets[X].Trim() == "")
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
        }
    }
}
