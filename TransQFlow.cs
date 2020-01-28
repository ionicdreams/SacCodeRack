using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.Web;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

//This is a SERVICE application without a GUI
namespace Zanghi_TransQFlow
{
    public partial class TransQFlow : ServiceBase
    {
        Thread oThread;
        TrackProgress UPDATE;

        string PROC_INPUT_DIR = File.ReadAllText(@"c:\Services\InternalReferences\CDD\WaitTimes\waittimes.ini");
        string PROC_OUTPUT_DIR = @"c:\Services\InternalReferences\CDD\WaitTimes";

        public TransQFlow()
        {
            InitializeComponent();

            UPDATE = new TrackProgress(PROC_OUTPUT_DIR, DateTime.Now.Date.ToShortDateString() + "@" + DateTime.Now.TimeOfDay.ToString());

        }

        protected override void OnStart(string[] args)
        {
            UPDATE.WriteProgress("TransQFlow has been initiated");
            try
            {
                SendStartEmail(UPDATE);
            }
            finally
            { }

            oThread = new Thread(new ThreadStart(TransInProg));
            oThread.Start();
        }

        protected override void OnStop()
        {
            UPDATE.WriteProgress("TransQFlow has been halted");
            try
            {
                string SourceLog = "Application";
                string SourceApp = "TransQFlow";

                EventLog.CreateEventSource(SourceApp, SourceLog);
                EventLog R = new System.Diagnostics.EventLog();
                R.Source = SourceApp;
                R.Log = SourceLog;
                R.WriteEntry("TransQFlow has been stopped by an external process.");
                oThread.Abort();

                SendEmail(UPDATE);
            }
            catch
            {

            }
        }


        public void TransInProg()
        {
            try
            {
                UPDATE.WriteProgress("TransInProg function started");
            }
            catch
            {
                UPDATE = new TrackProgress(PROC_OUTPUT_DIR, DateTime.Now.Date.ToShortDateString() + DateTime.Now.Date.ToShortTimeString());
                UPDATE.WriteProgress("TransInProg function started after redeclaration");
            }
            const int NumberOfElements = 7; //from LEFT to RIGHT on the QFLOW grid output

            bool[] ShowElement = new bool[NumberOfElements];
            string[] NameElement = new string[NumberOfElements];
            string[] OverwriteName = new string[NumberOfElements];

            string[] CDD = new string[] { "appointments", "building", "building otc com", "building otc res", "development engineering", "express", "fire", "housing", "minor permits", "planning", "planning otc", "signs", "utilities" };

            const string CDDHEAD = "CDD Counter";
            const int CDDOPEN = 900;
            const int CDDCLOSE = 1600;
            const int CDDLUNCH = 1200;

            const string REVENUE = "Sacramento Revenue Division";
            const string REVHEAD = "Revenue Counter";

            const int REVOPEN = 830;
            const int REVCLOSE = 1630;
            const int REVLUNCH = -1;  //rev currently does not close for a lunch hour
            string REVtoFORM = "";
            string CDDtoFORM = "";


            string SITECORE_DIR = PROC_INPUT_DIR.Substring(PROC_INPUT_DIR.ToLower().IndexOf("<sitecore$>") + 11, PROC_INPUT_DIR.ToLower().IndexOf("</sitecore$>") - (PROC_INPUT_DIR.ToLower().IndexOf("<sitecore$>") + 11));

            if (SITECORE_DIR.Substring(SITECORE_DIR.Length - 1, 1) != "\\")
            {
                SITECORE_DIR += "\\";
            }

            //##########################################################################################################################
            //--------------------------------------------------------- Turn on each elements visibility by removing the comment markers

            for (int X = 0; X < NumberOfElements; X++)
            {
                ShowElement[X] = false;
                NameElement[X] = "";
                OverwriteName[X] = "";
            }

            ShowElement[0] = true;
            NameElement[0] = "Service Name";
            OverwriteName[0] = "Service";

            ShowElement[1] = true;
            NameElement[1] = "Customers Waiting";
            OverwriteName[1] = "";

            // ShowElement[2] = true;   
            NameElement[2] = "Max Wait Time";
            OverwriteName[2] = "";

            ShowElement[3] = true;
            NameElement[3] = "Average Wait Time";
            OverwriteName[3] = "Average Wait";

            // ShowElement[4] = true;   
            NameElement[4] = "Customers in Service";
            OverwriteName[4] = "";

            // ShowElement[5] = true;   
            NameElement[5] = "Max Time In Service";
            OverwriteName[5] = "";

            // ShowElement[6] = true;   
            NameElement[6] = "Average Time in Service";
            OverwriteName[6] = "";

            //---------------------------------------------------------- Anything not listed defaults to false by init loop above
            //##########################################################################################################################
            UPDATE.WriteProgress("Standard variables defined, attempting to open web variables");

            HttpWebRequest WebRequestObject;
            StreamReader WebStream;

            FileInfo scribe;
            StreamWriter cpile;

            WebDataRow processData;
            WebDataRow processCDDData = null;

            int ErrCount = 0;
            bool TimerCount = true;

            ServiceBase myBase = new ServiceBase();
            myBase.CanStop = false;


            UPDATE.WriteProgress("Web variables defined, beginning infinite processing loop");


            while (true)
            {
                if (DateTime.Now.TimeOfDay.Hours >= 6 && DateTime.Now.TimeOfDay.Hours <= 18)
                {
                    if (TimerCount)
                    {
                        UPDATE.WriteProgress("Loading new repitition of infinite loop. It is now 5:59AM Mr. Connors.");

                        try
                        {

                            UPDATE.WriteProgress("Attempting to open web data streams");
                            // Open a connection
                            //WebRequestObject = (HttpWebRequest)HttpWebRequest.Create("http://10.100.7.69/dashboard/default.aspx"); (retired)
                            //WebRequestObject_alt1 = (HttpWebRequest)HttpWebRequest.Create("https://ewcddqf01.cityofsacramento.org/dashboard/default.aspx");

                            WebRequestObject = (HttpWebRequest)HttpWebRequest.Create("https://qflow.cityofsacramento.org/dashboard/default.aspx");

                            UPDATE.WriteProgress("__targetted web link");
                            // Open data stream:


                            WebRequestObject.KeepAlive = false;
                            WebRequestObject.Timeout = 1000000000;
                            WebRequestObject.ReadWriteTimeout = 1000000000;
                            WebRequestObject.AllowAutoRedirect = true;
                            //WebRequestObject.AllowReadStreamBuffering = true;
                            WebRequestObject.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Ssl3;
                            //ServicePointManager.Expect100Continue = false;
                            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

                            UPDATE.WriteProgress("__opening web link");

                            WebStream = new StreamReader(WebRequestObject.GetResponse().GetResponseStream());
                            UPDATE.WriteProgress("Web data stream reader is open");

                            // Open file stream to write shadow copy:
                            scribe = new FileInfo(SITECORE_DIR + @"Website\html\cdd\shadowQFlow.html");
                            UPDATE.WriteProgress("__targetted file stream reader");

                            cpile = scribe.CreateText();
                            //reset Revenue Output string
                            UPDATE.WriteProgress("File data streams are open");
                            REVtoFORM = "";

                            //declare string for the scope of this loop
                            string PC = "";

                            while ((PC = WebStream.ReadLine()) != null)
                            {
                                cpile.WriteLine(PC);

                                if (PC.Contains(REVENUE))
                                {
                                    //COPY THIS TO TEXT FILES ==================================================================ooooooooooooooooooooooooooooooooooooooooooooooo
                                    processData = new WebDataRow(NumberOfElements, PC.Replace("Sacramento", ""), REVHEAD);  //load the html into a string[] of elements
                                    //Process and Draw from the string of elements in HTML <TR> line:
                                    REVtoFORM = writeData(processData, REVOPEN, REVCLOSE, REVLUNCH, NumberOfElements, ShowElement, NameElement, OverwriteName, UPDATE);
                                }

                                if (DoesContain(PC, CDD))
                                {
                                    if (processCDDData == null)
                                    {
                                        processCDDData = new WebDataRow(NumberOfElements, PC.Replace("Sacramento", ""), CDDHEAD);  //load the html into a string[] of elements
                                    }
                                    else
                                    {
                                        processCDDData.Next = new WebDataRow(NumberOfElements, PC.Replace("Sacramento", ""), CDDHEAD);  //load the html into a string[] of elements
                                        processCDDData.Next.First = ((processCDDData.First == null) ? processCDDData : processCDDData.First);
                                        processCDDData = processCDDData.Next;
                                    }
                                }
                            }

                            WebStream.Close();
                            UPDATE.WriteProgress("Web data streams have closed");
                            cpile.Close();
                            UPDATE.WriteProgress("File data streams are open");

                            if (processCDDData != null)
                            {
                                CDDtoFORM = writeDataChunk(processCDDData.First, CDDOPEN, CDDCLOSE, CDDLUNCH, NumberOfElements, ShowElement, NameElement, OverwriteName, UPDATE);

                                scribe = new FileInfo(SITECORE_DIR + @"Website\html\cdd\CDDALL.html");
                                cpile = scribe.CreateText();
                                cpile.Write(CDDtoFORM);
                                cpile.Close();

                                processCDDData = null;
                            }

                            if (REVtoFORM != "")
                            {
                                scribe = new FileInfo(SITECORE_DIR + @"Website\html\cdd\REV.html");
                                cpile = scribe.CreateText();
                                cpile.Write(REVtoFORM);
                                cpile.Close();
                            }

                            UPDATE.WriteProgress("Processing queue data is complete. Error count reset to 0");
                            ErrCount = 0;
                        }
                        catch (Exception E)
                        {
                            ErrCount++;
                            UPDATE.WriteProgress("Error thrown: (Count=" + ErrCount.ToString() + ") " + E.ToString());
                            if (ErrCount == 1)
                            {
                                SendAlertEmail(E.ToString(), UPDATE);
                            }
                            else if (ErrCount % 10 == 0)
                            {
                                SendAlertEmail("Offline for over 10 minutes. " + E.ToString(), UPDATE);
                            }
                        }
                        finally
                        {
                            //return to the thread
                        }
                    }
                }

                TimerCount = !TimerCount;

                try
                {
                    UPDATE.WriteProgress("Entering sleep phase");
                    for (int X = 0; X < 10; X++)
                    {
                        //myBase.RequestAdditionalTime(2000);
                        //The above broke at some point, it used to work, but now throws exceptions
                        Thread.Sleep(2000);
                        //Look busy while we wait so windows doesnt think we are slacking
                    }
                }
                catch (Exception E)
                {
                    UPDATE.WriteProgress("Entering deep sleep phase");
                    //Sleeping for too long can sometimes confuse windows into thinking the thread is locked, sleeping in small chunks instead
                    for (int R = 0; R < 200; R++)
                    {
                        Thread.Sleep(500);
                    }

                    SendAlertEmail("Failure at Thread.Sleep() -> " + E.ToString(), UPDATE);
                }


            }
        }

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private bool DoesContain(string CheckString, string[] ArrayList)
        {
            string R = CheckString.ToLower();

            foreach (string X in ArrayList)
            {
                if (R.Contains(X))
                {
                    return true;
                }
            }

            return false;
        }

        private string writeDataChunk(WebDataRow R, int StartTime, int EndTime, int LunchTime, int NumberOfElements, bool[] ShowElement, string[] NameElement, string[] OverName, TrackProgress UPDATE)
        {
            UPDATE.WriteProgress("Beginning to write data chunks");
            int StartMin = StartTime % 100;
            int StartHour = (StartTime - StartMin) / 100;
            int EndMin = EndTime % 100;
            int EndHour = (EndTime - EndMin) / 100;
            int LunchMin = LunchTime % 100;
            int LunchHour = (LunchTime - LunchMin) / 100;

            string BuildHTML = "";

            //                             if this is true                                    or                       this is true                             or                        this is true                               then : IfNot
            bool IsOpen = ((DateTime.Now.Hour == StartHour && DateTime.Now.Minute > StartMin) || (DateTime.Now.Hour == EndHour && DateTime.Now.Minute < EndMin) || (DateTime.Now.Hour > StartHour && DateTime.Now.Hour < EndHour)) ? true : false;

            //Setup the HTML table for this line item
            BuildHTML = "<!DOCTYPE html><html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title></title><style type=\"text/css\"> .RowHeadAll { width: 135px !important; background-color: steelblue !important; color: white !important; text-align: center !important; border-top-left-radius: 5px !important; border-top-left-radius: 5px !important; border-top-right-radius: 5px !important; } .RowBod { width: 135px !important; text-align: center !important; box-shadow: 1px 1px 1px gray !important; border-radius: 5px !important; margin: 5px !important; padding: 5px !important; } .RowBodHead { width: 135px !important; text-align: left !important; box-shadow: 1px 1px 1px gray !important; border-radius: 5px !important; margin: 5px !important; padding: 5px !important; } .Footer { width: 300px !important; text-align: center !important; font-size: 12px !important; } </style></head><body><table><tr>";
            //.RowHeadLeft { width: 135px; background-color: steelblue; color: white; text-align: center; border-top-left-radius: 5px; } .RowHeadRight { width: 135px; background-color: steelblue; color: white; text-align: center; border-top-right-radius: 5px; } .RowHead { width: 135px; background-color: steelblue; color: white; text-align: center; } 
            UPDATE.WriteProgress("new HTML file built");

            for (int X = 0; X < NumberOfElements; X++)
            {
                if (ShowElement[X])
                {
                    //BuildHTML += "<td class=" + ((X == 0) ? "\"RowHeadLeft\"" : (X == (NumberOfElements - 1)) ? ("\"RowHeadRight\"") : ("\"RowHead\"")) + ">" + ((OverName[X] == "") ? NameElement[X] : OverName[X]) + "</td>";
                    BuildHTML += "<td class=\"RowHeadAll\">" + ((OverName[X] == "") ? NameElement[X] : OverName[X]) + "</td>";
                }
            }

            do
            {
                BuildHTML += "</tr><tr>";

                if (R.ThisData[0] != null && R.ThisData[0] != "")
                {
                    for (int X = 0; X < NumberOfElements; X++)
                    {
                        if (ShowElement[X])
                        {
                            BuildHTML += "<td " + ((X == 0) ? "class=\"RowBodHead\">" + R.ThisData[X + 1] + ":" : "class=\"RowBod\">" + (IsOpen ? R.ThisData[X + 1] : "CLOSED")) + "</td>";
                        }
                    }
                }

                if (R.Next != null)
                    R = R.Next;
                else
                    break;

            } while (true);

            string FixTimeStamp = DateTime.Now.TimeOfDay.ToString();
            FixTimeStamp = FixTimeStamp.Substring(0, FixTimeStamp.LastIndexOf(':'));
            BuildHTML += "</tr><tr><td class=\"Footer\" colspan=\"3\"><br />Wait time displayed as H:MM:SS (Hour:Minutes:Seconds) <br>Current as of: " + FixTimeStamp + " on " + DateTime.Now.ToShortDateString() + "</td></tr><table>";
            UPDATE.WriteProgress("returning HTML to main loop");
            return BuildHTML;
        }

        private string writeData(WebDataRow R, int StartTime, int EndTime, int LunchTime, int NumberOfElements, bool[] ShowElement, string[] NameElement, string[] OverName, TrackProgress UPDATE)
        {
            UPDATE.WriteProgress("Beginning data write stage");
            int StartMin = StartTime % 100;
            int StartHour = (StartTime - StartMin) / 100;
            int EndMin = EndTime % 100;
            int EndHour = (EndTime - EndMin) / 100;
            int LunchMin = LunchTime % 100;
            int LunchHour = (LunchTime - LunchMin) / 100;

            string BuildHTML = "";

            //                             if this is true                                    or                       this is true                             or                        this is true                               then : IfNot
            bool IsOpen = ((DateTime.Now.Hour == StartHour && DateTime.Now.Minute > StartMin) || (DateTime.Now.Hour == EndHour && DateTime.Now.Minute < EndMin) || (DateTime.Now.Hour > StartHour && DateTime.Now.Hour < EndHour)) ? true : false;

            if (R.ThisData[0] != null && R.ThisData[0] != "")
            {
                //Setup the HTML table for this line item
                BuildHTML = "<!DOCTYPE html><html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title></title><style type=\"text/css\"> .RowHeadAll { width: 135px !important; background-color: steelblue !important; color: white !important; text-align: center !important; border-top-left-radius: 5px !important; border-top-left-radius: 5px !important; border-top-right-radius: 5px !important; } .RowBod { width: 135px !important; text-align: center !important; box-shadow: 1px 1px 1px gray !important; border-radius: 5px !important; margin: 5px !important; padding: 5px !important; } .Footer { width: 300px !important; text-align: center !important; font-size: 12px !important; } </style></head><body><table><tr>";
                //.RowHeadLeft { width: 135px; background-color: steelblue; color: white; text-align: center; border-top-left-radius: 5px; } .RowHeadRight { width: 135px; background-color: steelblue; color: white; text-align: center; border-top-right-radius: 5px; } .RowHead { width: 135px; background-color: steelblue; color: white; text-align: center; } 
                UPDATE.WriteProgress("Writing data");

                for (int X = 0; X < NumberOfElements; X++)
                {
                    if (ShowElement[X])
                    {
                        //BuildHTML += "<td class=" + ((X == 0) ? "\"RowHeadLeft\"" : (X == (NumberOfElements - 1)) ? ("\"RowHeadRight\"") : ("\"RowHead\"")) + ">" + ((OverName[X] == "") ? NameElement[X] : OverName[X]) + "</td>";
                        BuildHTML += "<td class=\"RowHeadAll\">" + ((OverName[X] == "") ? NameElement[X] : OverName[X]) + "</td>";
                    }
                }

                BuildHTML += "</tr><tr>";
                for (int X = 0; X < NumberOfElements; X++)
                {
                    if (ShowElement[X])
                    {
                        BuildHTML += "<td class=\"RowBod\"><center>" + ((X == 0) ? R.ThisData[X + 1] : (IsOpen ? R.ThisData[X + 1] : "CLOSED")) + "</center></td>";
                    }
                }

                string FixTimeStamp = DateTime.Now.TimeOfDay.ToString();
                FixTimeStamp = FixTimeStamp.Substring(0, FixTimeStamp.LastIndexOf(':'));
                BuildHTML += "</tr><tr><td class=\"Footer\" colspan=\"3\"><br />Wait time displayed as H:MM:SS (Hour:Minutes:Seconds) <br>Current as of: " + FixTimeStamp + " on " + DateTime.Now.ToShortDateString() + "</td></tr><table>";
                UPDATE.WriteProgress("Leaving data write stage");
            }

            return BuildHTML;
        }

        public void SendEmail(TrackProgress UPDATE)
        {
            UPDATE.WriteProgress("Attempting to send email");
            MailMessage genForm = new MailMessage();
            SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

            genForm.IsBodyHtml = true;

            genForm.To.Add("jzanghi@cityofsacramento.org");


            genForm.Subject = "TransQFlow (The QFLOW Shadowcopy Service) has stopped. Please restart this service.";
            genForm.Body = "QFLOW Shadowcopy Service has stopped. Please restart this service to continue Live Wait Times on City web pages.";
            genForm.From = new MailAddress("noreply@CityOfSacramento.org");
            sendClient.Send(genForm);
            UPDATE.WriteProgress("Email sent");
        }

        public void SendStartEmail(TrackProgress UPDATE)
        {
            UPDATE.WriteProgress("Attempting to send email");
            MailMessage genForm = new MailMessage();
            SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

            genForm.IsBodyHtml = true;

            genForm.To.Add("jzanghi@cityofsacramento.org");


            genForm.Subject = "TransQFlow (The QFLOW Shadowcopy Service) has been started at " + DateTime.Now.ToShortTimeString();
            genForm.Body = "The process has been started.";
            genForm.From = new MailAddress("noreply@CityOfSacramento.org");
            sendClient.Send(genForm);
            UPDATE.WriteProgress("Email sent");
        }

        private void SendAlertEmail(string oMessage, TrackProgress UPDATE)
        {
            UPDATE.WriteProgress("Attempting to send email");
            try
            {
                MailMessage genForm = new MailMessage();
                SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

                genForm.IsBodyHtml = true;

                genForm.To.Add("jzanghi@cityofsacramento.org");


                genForm.Subject = "TransQFlow: -> " + oMessage.Replace('\r', ' ').Replace('\n', ' ');
                genForm.Body = "QFLOW Shadowcopy Service is having problems. [Returned Message: " + oMessage + "]";
                genForm.From = new MailAddress("noreply@CityOfSacramento.org");
                UPDATE.WriteProgress("__clicking send");
                sendClient.Send(genForm);
                UPDATE.WriteProgress("__Email sent");
            }
            catch (Exception E)
            {
                UPDATE.WriteProgress("Error sending email: " + E.ToString());
            }
        }
    }

    public class TrackProgress
    {
        string PROC_OUTPUT_DIR;
        string EXECUTION_TS;

        const string PROGRESSFILE = "\\PROGRESS.txt";
        string TEMPFILE = "\\TEMP.txt";

        public TrackProgress(string OUTPUT_DIR, string XECUTION_TS)
        {
            PROC_OUTPUT_DIR = OUTPUT_DIR;
            EXECUTION_TS = XECUTION_TS;

            Write("Opening Progress String for new Execution Time");
        }

        public void WriteProgress(string STAGE_DISC)
        {
            Write(STAGE_DISC);

            CheckLength();
        }

        private void Write(string Output)
        {
            if (!Directory.Exists(PROC_OUTPUT_DIR))
            {
                try
                {
                    Directory.CreateDirectory(Output);

                }
                catch
                {
                    return;
                }
            }

            try
            {
                File.AppendAllText(PROC_OUTPUT_DIR + PROGRESSFILE, "[" + EXECUTION_TS + "] " + Output + "\r\n");
            }
            catch
            {

            }
        }

        private void CheckLength()
        {
            try
            {
                string[] XR = File.ReadAllLines(PROC_OUTPUT_DIR + PROGRESSFILE);
                if (XR.Length > 999)
                {
                    Write("Shortening progress files");
                    try
                    {
                        File.Delete(PROC_OUTPUT_DIR + TEMPFILE);
                    }
                    catch
                    {
                        TEMPFILE = "temp" + DateTime.Now.TimeOfDay.TotalSeconds.ToString() + ".txt";
                    }

                    try
                    {
                        int bump = XR.Length - 623;

                        for (int Y = bump; Y < XR.Length; Y++)
                        {
                            File.AppendAllText(PROC_OUTPUT_DIR + TEMPFILE, XR[Y] + "\r\n");
                        }
                    }
                    catch (Exception E)
                    {
                        Write("__error setting temp file: " + E.ToString());
                    }

                    bool cancontinue = false;
                    int errorcount = 0;

                    while (!cancontinue && errorcount < 23)
                    {
                        try
                        {
                            File.Replace(PROC_OUTPUT_DIR + TEMPFILE, PROC_OUTPUT_DIR + PROGRESSFILE, PROC_OUTPUT_DIR + "\\BU_Prog");
                            cancontinue = true;
                        }
                        catch (Exception E)
                        {
                            Write("__error replacing process file: " + E.ToString());
                            errorcount++;
                            Thread.Sleep(200);
                        }
                    }

                }

            }

            catch
            {
                Write("Failed to shorten progress files");
            }
        }


    }

    class WebDataRow
    {
        public WebDataRow Next;
        public WebDataRow First;

        public string[] ThisData;

        //THIS FUNCTION WILL TAKE THE HEADER OUT OF THE DATA GIVEN
        public WebDataRow(int NumberOfElements, string[] NameOfElements)
        {
            ThisData = new string[NumberOfElements];
            for (int R = 0; R < NumberOfElements; R++)
            {
                ThisData[R] = NameOfElements[R];
            }
        }

        //THIS FUNCTION WILL OVERWRITE THE HEADER WITH OUR CHOICE STRING 'Header'
        public WebDataRow(int NumberOfElements, string DataLine, string Header)
        {
            ThisData = new string[NumberOfElements];
            LoadData(DataLine);
            ThisData[0] = Header;
        }

        public void LoadData(string DataLine)
        {
            bool offon = true;
            bool wasdata = false;
            int mineral = 0;

            foreach (Char X in DataLine.ToCharArray())
            {
                if (offon)
                {
                    if (X != '<')
                    {
                        if (mineral < 7)
                        {
                            ThisData[mineral] += X;
                            wasdata = true;
                        }
                        else
                        {
                            break; //we are out of bounds now, we can ignore the rest of the string
                        }
                    }
                    else
                    {
                        offon = false;
                        if (wasdata)
                        {
                            mineral++;
                            wasdata = false;
                        }
                    }
                }
                else
                {
                    if (X == '>')
                    {
                        offon = true;
                    }
                }
            }
        }
    }
}
