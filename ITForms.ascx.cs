using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Configuration;
using System.Web.UI.HtmlControls;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Data.Sql;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Data.Common;
using FC_Libs;
using System.Text.RegularExpressions;
using Telerik.Web.UI;

using Zanghi_ASPForms;

namespace Layouts.Itform_nhr
{
    //Project file (THIS IS THE CURRENTLY ACTIVE FILE)
    //This code was created by Jason Zanghi in the IT department
    //Please contact me at 808-8228 if you have any questions or concerns
    //Please do not modify this code without notifying me, as I may unintentionally overwrite your changes

    /// <summary>
    /// Summary description for Itform_nhrSublayout
    /// </summary>
    public partial class Itform_nhrSublayout : System.Web.UI.UserControl
    {
        System.Drawing.Color ButtonsBackColor = System.Drawing.Color.WhiteSmoke;

        //========================================================================================= Constant constructors used throughout the form =
        const string REM_FROM_BODY = "@cityofsacramento.org";

        // Replace these items (OG) if they are still in the email with the new items (NG) for cosmetic purposes.k
        string[] REPLACE_INEMAIL_OG = new string[] { EMAIL_REG_DT, EMAIL_REG_NORTH, EMAIL_REG_SOUTH };
        string[] REPLACE_INEMAIL_NG = new string[] { "Downtown Region", "North Region", "South Region" };

        const string EMAIL_SERVERTEAM = "ITServerTeam@cityofsacramento.org";
        const string EMAIL_ADMIN = "ITAdmin@cityofsacramento.org";
        const string EMAIL_ECAPS = "eCAPS@elkaceap01.cityofsacramento.org";
        const string EMAIL_REG_NORTH = "ITSupportNorth@cityofsacramento.org";
        const string EMAIL_NET_CORE = "Telecom@elkaceap01.cityofsacramento.org";
        const string EMAIL_REG_DT = "ITSupportDowntown@cityofsacramento.org";
        const string EMAIL_REG_SOUTH = "ITSupportSouth@cityofsacramento.org";
        const string EMAIL_SHADOW = "jzanghi@cityofsacramento.org";
        const string EMAIL_SMTP = "mail.cityofsacramento.org";
        const string EMAIL_SMTP_FROM = "noreply@cityofsacramento.org";
        const string EMAIL_JOHNNY = "jsetunyarut@cityofsacramento.org";
        const string EMAIL_PUBSAFEPD = "helpdesk@pd.cityofsacramento.org";
        const string EMAIL_PUBSAFEFD = "helpdesk@pd.cityofsacramento.org";
        const string EMAIL_Accela = "accelaannie@cityofsacramento.org";

        string[] GLOBAL_COFSDomains = new string[] { "cityofsacramento.org", "crockerartmuseum.org", "thediscovery.org", "fairytaletown.org", "powerhousesciencecenter.org", "saczoo.org", "sacramento365.com", "sfd.cityofsacramento.org", "pd.cityofsacramento.org", "visitsacramento.com", "waterforum.org" };

        const string NameIdentifier = "My name is:";
        const string EmailIdentifier = "My email is:";
        const string DeptIdentifier = "My department:";
        const string RegionIdentifier = "I am in this Region:";

        const string EmpEmailIdentifier = "Employee's Email:";

        //This variable set it so I only have to change one item instead of each section binding for each form
        string FORM_HireForm = "Set up new or current employee:";
        string FORM_SepForm = "Separate an Employee:";
        string FORM_TransForm = "Transfer an Employee:";
        string FORM_311VoipForm = "(send/attach) Cisco Agent Line and Verba:";
        string FORM_VoipForm = "(send/attach) Desk Phone request:";
        string FORM_VMForm = "(send/attach) Voicemail request:";
        string FORM_VPN = "(send/attach) SSL VPN Request:";
        string FORM_PCQuote = "Get a quote for a computer:";
        string FORM_MSOHUP = "Microsoft Office Home Use";
        string FORM_NameChange = "Request a Name Change:";
        string FORM_BatchSep = "Batch Separations:";
        string FORM_enterVo = "Request a S&B enterVo Account:";
        string FORM_Mobile = "Cellular device Request:";
        string FORM_Cham = "Chameleon Access Request";
     

        string URL_MSOHU_ORIG = "/html/files/msohup_original.pdf";
        string URL_API_5000 = @"c:\inetpub\wwwroot\sitecore\Website\html\files\api5000.pdf";
        string URL_VPN_HowTo = @"c:\inetpub\wwwroot\sitecore\Website\html\files\vpnhowto.pdf";

        string FORM_TransferHireText = "Employee New Hire";
        string FORM_TransferSepText = "Employee Separation";

        const string FID_Region = "<?>Select your Region:</?>";
        string[] FID_Region_Options = new string[] { "(Light Red Area)", "(White Area)", "(Blue Area)", "(Other)" };

        string[] DONOTEMAIL_Flag;
        string[] DNE_EmailCopies = new string[] { EMAIL_JOHNNY };

        //Form-wide Standards for the form sections
        string[] FS_Name = new string[] { "First:", "M:", "Last:" };


        System.Drawing.Color FS_NoteColor = System.Drawing.Color.SteelBlue;
        System.Drawing.Color FS_AlertColor = System.Drawing.Color.DarkRed;
        System.Drawing.Color FS_UnavailableColor = System.Drawing.Color.DarkGray;

        System.Drawing.Color FS_GoldStandard = System.Drawing.Color.Goldenrod;
        System.Drawing.Color FS_SilverStandard = System.Drawing.Color.LightSlateGray;


        const int NumTables = 20;
        //==========================================================================================================================================

        FormSection SECTION;
        CheckBoxList ApplySections = new CheckBoxList();

        int EmailPullCounter = 0;
        int CurTable = 0;
        //enter new information
        string FirstName;
        string LastName;
        string EmpFName;
        string EmpLName;
        string SelectedForms;
        string SelRegion;
        string UserEmail;
        string EmpEmail;
        string UserDept;

        string[] SelectedGroupings;
        string[] ActiveInGroupings;

        bool TicketsOnHold = false;
        string TicketsHeldFor = "";

        //protected void rcbFILTEROPTION_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    switch (Convert.ToInt32(rcbFILTEROPTION.SelectedValue))
        //    {
        //        case 0:
        //            rcbECAPS.Filter = RadComboBoxFilter.Contains;
        //            break;
        //        case 1:
        //            rcbECAPS.Filter = RadComboBoxFilter.StartsWith;
        //            break;
        //        default:
        //            rcbECAPS.Filter = RadComboBoxFilter.Contains;
        //            break;
        //    }
        //}

        private void CheckCustomFilters(string Form)
        {
            if (Form.ToUpper().Contains("MO-01001") && (Form.ToUpper().Contains(FORM_SepForm) || Form.ToUpper().Contains(FORM_HireForm) || Form.ToUpper().Contains(FORM_TransForm)))
            {
                if (UserEmail.ToLower() != "smizuno@cityofsacramento.org")
                {
                    TicketsOnHold = true;
                    TicketsHeldFor = "Stephanie Mizuno";
                }
            }
        }

        private void FUNCT_DONOTEMAIL_AddFlag(string Flag)
        {
            if (DONOTEMAIL_Flag == null)
            {
                DONOTEMAIL_Flag = new string[] { Flag };
            }
            else
            {
                string[] temp = new string[DONOTEMAIL_Flag.Length + 1];
                DONOTEMAIL_Flag.CopyTo(temp, 0);
                temp[DONOTEMAIL_Flag.Length] = Flag;
                DONOTEMAIL_Flag = temp;
            }
        }

        private void BUILDFORMCONTENT()
        {

            //This variable set is to name the sub-forms used for multiple forms
            string SubDate = "";
            string Sub = "";

            //This variable set is for independant cells that are not strongly tied to any form but exist in general for all forms
            string CelRegion = "Select your Region:";
            string CelNotes = "My additional notes for IT staff:";

            //This variable is for the main grouping, which started under sub-project name "Epsilon"
            string EPSILON = "EPSILON";

            //Subsection variables:
            string SUBFORM_sepcomp = "Employee's Computer:";
            string SUBFORM_sepdate = "Date of separation:";
            string SUBFORM_sepvoip = "Desk phone:";
            string SUBFORM_sepcell = "Cell Devices:";
            string SUBFORM_sepnetwork = "Email / Network accounts:";
            string SUBFORM_seppersonal = "OneDrive / Personal data:";
            string SUBFORM_sepecaps = "eCaps User ID:";
            string SUBFORM_hiredate = "Date of employment:";
            string SUBFORM_hirecomp = "Computer needed:";
            string SUBFORM_hireoffice = "Microsoft License:";
            string SUBFORM_transformersup = "Employee's former supervisor:";
            string SUBFORM_transdate = "Date of transfer:";
            string SUBFORM_transcomp = "Equipment being transferred:";
            string SUBFORM_transemail = "Employee's email account:";
            string SUBFORM_voipfund = "Funding for Phone/Voicemail:";
            string SUBFORM_enterVoAccess = "S&B enterVo Access:";
            string SUBFORM_mobiletype = "Device Requested:";
            string SUBFORM_chamsuper = "Chameleon - Requesting Supervisor and Dept Manager Approval:";
            


            string DoNotEmailCheck = "";
            // ################################################################
            // ############ MASTER TABLE, MUST BE VISIBLE #####################
            // ################################################################

            ITEM_addNewSection("My information:", ""); //This section cannot be hidden, iut is our master section.
            SECTION.Insert_BiText(NameIdentifier, FS_Name[0], FS_Name[2], true, true, true);
            //SECTION.Insert_EmailText(EmailIdentifier, true);
            SECTION.Insert_EmailWithSetDomains(STYLE_AddHelpButton(EmailIdentifier, "Only enter what is before the \"@\" symbol in the text box, then select a @domain"), GLOBAL_COFSDomains, true);
            SECTION.Insert_CascadeDataSourceDDL(DeptIdentifier, SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', true);
            SECTION.Insert_SingleText("My contact number:", false, true);
            FinishMainSection();


            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@ BEGIN HIDABLE TABLES (All others) @@@@@@@@@@@@@@@
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

            
           
            // DECLARE TABLE
            ITEM_addNewSection(FORM_HireForm, EPSILON);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_Information("*Important*", "Please do not fill out this form for yourself. This form should be submitted by the employee's supervisor or manager only.", FS_NoteColor);
            //SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            SECTION.Insert_DropList("Employment type:", new string[] { "Contractor", "Employee", "Intern", "Volunteer" }, false, true);

            SECTION.Insert_CascadeDataSourceDDL("Employee's department:", SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', false);
            SECTION.Insert_SingleText("Job title/classification:", false, true);
            SECTION.Insert_SingleText("Phone extension (Optional):", false, false);
            SECTION.Insert_SingleText("Location:", false, true);
            SECTION.Insert_SingleText("Mailstop:", false, false);


            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            //1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                //FORMTYPE = Employee New Hire
            SECTION.addEmailForm(EMAIL_ADMIN);
            SECTION.addEmailHighlight("HIREEMP_EXCHANGE", "Network, Email, and o365 License");
            SECTION.addEmailHeader(FORM_TransferHireText + " - ADMIN: Please perform the following:\r\n1. Allocate o365 license for new employee\r\n2. Transfer this ticket to the Server Team (TIS)\r\n\r\nSERVER TEAM: Please perform the following:\r\n1. Create AD account\r\n2. Create Network and Email accounts\r\n3. Reassign this ticket to " + FID_Region + "\r\n\r\n" + FID_Region + ": Please perform the following:\r\n1.Assign additional group memberships in AD\r\n2.Update employee's AD account (DeptID, Location, Phone, etc.)\r\n3.Contact requestor with first time logon information");
            SECTION.addEmailSections(new string[] { FORM_HireForm, SUBFORM_hiredate, SUBFORM_hireoffice, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //2. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("HIREEMP_EQUIPMENT", "Computer Equipment");
            SECTION.addEmailHeader(FORM_TransferHireText + " - This ticket is for equipment only. Please do not forward to Server Team. The server team already has a ticket to create user accounts.\r\n\r\nPlease perform the following with the below information:\r\n1.Complete any physical changes needed to the PC (I.E. new mouse, move the pc, etc.)\r\n2.Complete any software changes needed (I.E. updates, install printers, etc.)\r\n3.Update equipment assignments in Kace Assets\r\n4.Verify MDM is configured for any mobile devices assigned to the user\r\n5.Run OneDrive setup with user/verify they can connect to OneDrive and O365\r\n6.Setup scanning B: drive on NSFS1->CityShare\r\n7.Update copier to save scans into users B drive\r\n8.If laptop or mobile device - verify BitLocker is ON and configured");
            SECTION.addEmailSections(new string[] { FORM_HireForm, SUBFORM_hiredate, SUBFORM_hirecomp, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_hiredate, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_HireForm);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_DateDDL("Active as of:");
            SECTION.Insert_SingleText("Special Instructions:", true, false);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();


            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(FORM_SepForm, EPSILON);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_Information("*Important*", "Per API 3801 (Employee Separation Policy) ALL persons separated from the City MUST be reported to <a href=\"mailto:citywideseparationteam@cityofsacramento.org\" style=\"color:white; text-decoration: underline;\">City Wide Separation Team</a> with basic employee information as an alert. This includes Temps, Interns, and people not given computer access.", FS_NoteColor);
            SECTION.Insert_Spacer();
            SECTION.Insert_Information("*Important*", "If employee is under investigation or being terminated, please contact us immediately at x7111", FS_NoteColor);
            SECTION.Insert_Information("*Notice*", "All false reports of a separation will be reported to management.", FS_AlertColor);
            SECTION.Insert_Spacer();

            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            SECTION.Insert_SingleText("Job title/classification:", false, true);
            //SECTION.Insert_RadioField(STYLE_AddHelpButton("Employment type:", "If an intern is on a contract, please still choose Intern"), new string[] { "City Employee", "Non-City Employee", "Contractor", "Volunteer", "Intern" });
            SECTION.Insert_DropList(STYLE_AddHelpButton("Employment type:", "If an intern is on a contract, please still choose Intern"), new string[] { "City Employee", "Non-City Employee", "Contractor", "Volunteer", "Intern" }, false, true);
            SECTION.Insert_RadioTextOnSelect("eCaps Account:", new string[] { "User has an eCaps account", "User was not in the eCaps system" }, "eCaps User ID:", 0, false, true);
            SECTION.Insert_SingleText("Assigned phone (Optional):", false, false);
            SECTION.Insert_CascadeDataSourceDDL("Employee's department:", SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', false);



            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            //1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_ADMIN);
            SECTION.addEmailHighlight("SEPEMP_EXCHANGE", "Network, Email, and o365 license");
            SECTION.addEmailHeader(FORM_TransferSepText + " - This ticket is for the Network and Email accounts, and was originated from region " + FID_Region + ", please perform the following:\r\n1.Update licensing\r\n2.Transfer this ticket to Server Team (TIS) for processing\r\n3.Assign OneDrive access as needed\r\nTransfer this ticket to Regional Support Engineer if OneDrive files need to be moved");
            SECTION.addEmailSections(new string[] { FORM_SepForm, SUBFORM_sepdate, SUBFORM_sepnetwork, SUBFORM_seppersonal, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //2. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("SEPEMP_EQUIPMENT", "Computer Equipment");
            SECTION.addEmailHeader(FORM_TransferSepText + " - This ticket is for the equipment only. Please do not forward to Server Team. The server team already has a ticket to deactivate user accounts.\r\n\r\nPlease perform the following with the below information:\r\n1. Search for assets assigned to the employee in Kace and verify no devices are assigned. If devices are assigned to the employee, notify requesting manager and request disposition of the device (e.g. assign to another employee, stock, surplus, Not Assigned, etc). \r\n 2.Make any necessary computer equipment changes indicated.");
            SECTION.addEmailSections(new string[] { FORM_SepForm, SUBFORM_sepdate, SUBFORM_sepcomp, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //3. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_ECAPS);
            SECTION.addEmailHighlight("SEPEMP_ECAPS", "eCaps");
            SECTION.addEmailHeader(FORM_TransferSepText + " - This ticket is for ecaps. Please deactivate the below user accounts or confirm they did not have an account:");
            SECTION.addEmailSections(new string[] { FORM_SepForm, SUBFORM_sepdate, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //4. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("SEPEMP_VOIP", "Phones");
            SECTION.addEmailHeader(FORM_TransferSepText + " - This ticket is for the VOIP phone and voicemail.\r\n\r\nPlease perform the following with the below information:\r\n1.Make any necessary changes to VOIP line.\r\n2.If the phone lines are all to be removed, please return the phone to IT stock.\r\n3.Update the 'End User' in CallManager to update the Corp Directory on all phones.\r\n4.Delete or update voicemail account.");
            SECTION.addEmailSections(new string[] { FORM_SepForm, SUBFORM_sepdate, SUBFORM_sepvoip, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //5. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //SECTION.addEmailForm(FID_Region);
            //SECTION.addEmailHighlight("SEPEMP_HOMEDIR", "OneDrive and or Network Files");
            //SECTION.addEmailHeader(FORM_TransferSepText + " - Please assign to a Regional Support Engineer, do not forward to Server Team. This ticket is for employee's OneDrive.  \r\n\r\nPlease perform the following with the below information:\r\n 1.Please delete or transfer the employees personal OneDrive/network files with the options selected below.");
            //SECTION.addEmailSections(new string[] { FORM_SepForm, SUBFORM_sepdate, SUBFORM_seppersonal, CelNotes });
            //SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //6. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("SEPEMP_CELLULAR", "Cellular Devices");
            SECTION.addEmailHeader(FORM_TransferSepText + " - This ticket is for the cellular devices.\r\n\r\nPlease have liaison perform the standard procedures with the below information:");
            SECTION.addEmailSections(new string[] { FORM_SepForm, SUBFORM_sepdate, SUBFORM_sepcell, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //7. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_Accela);
            SECTION.addEmailHighlight("SEPEMP_ACCELA", "Accela Annie");
            SECTION.addEmailHeader(FORM_TransferSepText + " - This email is for Accela Annie.\r\n\r\nPlease note the following employee below has separated from the City:");
            SECTION.addEmailSections(new string[] { FORM_SepForm, SUBFORM_sepdate, SUBFORM_sepcell, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(FORM_BatchSep, EPSILON);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_Information("*Important*", "Per API 3801 (Employee Separation Policy) ALL persons separated from the City MUST be reported to <a href=\"mailto:citywideseparationteam@cityofsacramento.org\" style=\"color:white; text-decoration: underline;\">City Wide Separation Team</a> with basic employee information as an alert. This includes Temps, Interns, and people not given computer access.", FS_NoteColor);
            SECTION.Insert_Spacer();
            SECTION.Insert_Information("*Notice*", "All batch separations must meet the specified criteria below, separations that do not will be flagged and sent back to you to complete a full Employee Separation Form.", FS_AlertColor);
            SECTION.Insert_Spacer();
            SECTION.Insert_CheckCriteriaField("Employees Listed Below:", new string[] { "Currently DO NOT have a DESK PHONE", "Currently DO NOT have an assigned COMPUTER <i>(or will be later reassigned)</i>", "Currently DO NOT have a City issued CELL PHONE", "Currently DO NOT have any OTHER IT issued equipment" });
            SECTION.Insert_CheckCriteriaField("Below Employees:", new string[] { "Only have a Network/email/eCAPs account, and I confirm no further actions will be required by IT because the employee was either never assigned above equipment OR such equipment has already been reassigned." });

            SECTION.Insert_CascadeDataSourceDDL(STYLE_AddHelpButton("Employees' department:", "If the employees fall under different groups of the department, just select the first group and we can process from there. Complete a separate batch for different departments."), SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', false);
            SECTION.Insert_DateDDL("Process separations as of:");
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(1) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            SECTION.Insert_RadioField(STYLE_AddHelpButton("Job type:", "Choose the closest match. Field Workers are such jobs as Parking Enforcement, Utility Workers, etc. Off-Network is such jobs as START, School Programs, Box Office, etc."), new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, true);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(2) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, false, false, false);
            SECTION.Insert_RadioField("Job type:", new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, false);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(3) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, false, false, false);
            SECTION.Insert_RadioField("Job type:", new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, false);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(4) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, false, false, false);
            SECTION.Insert_RadioField("Job type:", new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, false);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(5) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, false, false, false);
            SECTION.Insert_RadioField("Job type:", new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, false);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(6) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, false, false, false);
            SECTION.Insert_RadioField("Job type:", new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, false);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(7) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, false, false, false);
            SECTION.Insert_RadioField("Job type:", new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, false);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(8) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, false, false, false);
            SECTION.Insert_RadioField("Job type:", new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, false);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(9) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, false, false, false);
            SECTION.Insert_RadioField("Job type:", new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, false);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("(10) Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, false, false, false);
            SECTION.Insert_RadioField("Job type:", new string[] { "Field Worker", "Intern/Temp/Consultant", "Off-Network Employee" }, false);
            SECTION.Insert_Spacer();

            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            //1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_ADMIN);
            SECTION.addEmailHighlight("BSEPEMP_EXCHANGE", "Batch Network, Email, and o365 Licenses");
            SECTION.addEmailHeader("Batch Employee Separations - This ticket is for multiple employee Network and Email accounts, and was originated from region " + FID_Region + ":\r\n1.Update licensing for the following employees\r\n2.Transfer this ticket to Server Team (TIS) for processing");
            SECTION.addEmailSections(new string[] { FORM_BatchSep, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //2. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("BSEPEMP_EQUIPMENT", "Verify no IT Equipment");
            SECTION.addEmailHeader("Batch Employee Separations - This ticket is for checking references only. Please do not forward to Server Team, they already have a ticket to deactivate these user accounts.\r\n\r\nPlease verify the below employees did not have any IT equipment assigned to them:\r\n1.Reject any employees with assigned IT equipment back to the supervisor or manager.");
            SECTION.addEmailSections(new string[] { FORM_BatchSep, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //3. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_ECAPS);
            SECTION.addEmailHighlight("BSEPEMP_ECAPS", "Batch Separations for eCaps");
            SECTION.addEmailHeader("Batch Employee Separation - This ticket is for ecaps accounts. Please deactivate the below user accounts:");
            SECTION.addEmailSections(new string[] { FORM_BatchSep, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //7. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_Accela);
            SECTION.addEmailHighlight("BSEPEMP_ACCELA", "Accela Annie");
            SECTION.addEmailHeader("Batch Employee Separations - This email is for Accela Annie.\r\n\r\nPlease note the following employees below have separated from the City:");
            SECTION.addEmailSections(new string[] { FORM_BatchSep, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();
            //// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            //// DECLARE TABLE
            //ITEM_addNewSection(SUBFORM_sepecaps, "");

            //// DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            //SECTION.Bind(FORM_SepForm);

            //// ADD TABLE/FORM ELEMENTS
            //SECTION.Insert_SingleText(STYLE_AddHelpButton("Employee's eCAPS user ID:", "Required for City Employees"), false, false);

            //// CAP THE BOTTOM OF THE TABLE OFF
            //ITEM_endNewSection();


            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_sepdate, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_SepForm);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_DateDDL("Begin separation as of:");
            SECTION.Insert_TimeBox("At this time:", false);
            SECTION.Insert_SingleText("Special Instructions:", true, false);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_sepnetwork, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_SepForm);

            // ADD TABLE/FORM ELEMENTS
            //SECTION.Insert_DropList("Email account:", new string[] { "Disable and delete account", "Disable account, set 'out of office' message, delete after 30 days"}, true, true);

            //SECTION.Insert_DropList_WithDisabler("Email account:", new string[] { "Disable and delete account", "Disable account, set 'out of office' message, delete after 30 days" }, new bool[] { true, false }, true, true);
            //SECTION.Insert_DropWithTextOnSelect("Email account:", new string[] { "Disable and delete account", "Disable account, set 'out of office' message, delete after 30 days" }, "Enter 'Out of office' message:", 1, true, true);
            string OutOfOfficeDefault = "Thank you for contacting the City of Sacramento.\r\n\r\nThis employee no longer works for the City of Sacramento, but we would be happy to help you contact one of our departments at our 311 Call Center.\r\n\r\nTo contact 311, you can call 311 on your phone (or 916-808-5011), or you can email us at 311@cityofsacramento.org and we will reply to you as soon as we are able.\r\n\r\nThank you once again for contacting the City of Sacramento.";
            SECTION.Insert_DropList_WithGhostBox("Email account:", new string[] { "Disable and delete account", "Disable account, set 'out of office' message, delete after 30 days", "See special instructions below" }, new bool[] { false, true, false }, "Out of office message:", "", true, true, true);

            SECTION.Insert_SingleText("Special Instructions:", true, false);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_seppersonal, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_SepForm);

            // ADD TABLE/FORM ELEMENTS
            //SECTION.Insert_DropList(
            //    STYLE_AddHelpButton("Home directory files:", "These are the employee's personal network files that are not shared with other employees."),
            //    new string[] { "Delete", "Transfer files to another employee" },
            //    true,
            //    true
            //);

            SECTION.Insert_DropList_WithGhostBox(
                STYLE_AddHelpButton("Home directory files:", "These are the employee's personal OneDrive/network files that are not shared with other employees."),
                new string[] { "Delete", "Transfer files to another employee" }, new bool[] { false, true }, "Employee to receive files:", "", false, true, true);

            SECTION.Insert_Spacer();

            SECTION.Insert_SingleText("Special Instructions:", true, false);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_sepvoip, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_SepForm);

            // ADD TABLE/FORM ELEMENTS
            DoNotEmailCheck = "This person did not have a phone";

            SECTION.Insert_DropList_WithDisabler("Phone extension:",
                new string[] { "Delete extension and return number to pool", "Keep this extension active, I will reassign it later", DoNotEmailCheck },
                new bool[] { false, false, true },
                false, true);

            FUNCT_DONOTEMAIL_AddFlag(DoNotEmailCheck); //will not create VOIP ticket if this option is checked


            SECTION.Insert_RadioTextOnSelect("Voicemail account:", new string[] { "Delete messages and account", "Keep this voicemail active, I will reassign it later", "This person did not have a voicemail account" }, "Phone Extension:", new int[] { 0, 1 }, true, true);
            //SECTION.Insert_DropList("Voicemail account:", new string[] { "Delete messages and account", "Keep this voicemail active, I will reassign it later", "This person did not have a voicemail account" }, true, true);
            SECTION.Insert_SingleText("Special Instructions:", true, false);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_sepcell, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_SepForm);

            // ADD TABLE/FORM ELEMENTS
            DoNotEmailCheck = "No cell phones or other cellular devices";
            SECTION.Insert_DropList_WithDisabler("Employee was given:",
                new string[] { DoNotEmailCheck, "A cell phone", "A tablet with cellular service", "A pager", "Something else I will describe below", "Multiple devices I will list below" },
                new bool[] { true, false, false, false, false, false },
                false, true);
            FUNCT_DONOTEMAIL_AddFlag(DoNotEmailCheck);
            SECTION.Insert_SingleText("Phone numbers of devices:", false, true);
            SECTION.Insert_RadioVerticalField(
                STYLE_AddHelpButton("All devices recovered:", "Were these cellular all recovered from this employee? If the employee was not given any cellular devices then please select 'Yes'"),
                new string[] { "Yes I have received them all", "No, employee did not return all devices" }, true
            );
            SECTION.Insert_RadioField("Carrier of devices:", new string[] { "AT&T", "Sprint", "Verizon", "None", "I'll describe below" }, true);

            SECTION.Insert_RadioTextOnSelect("Service:", new string[] { "Cancel service (cancels monthly billing after any charges)", "Modify service (provide special instructions in \"Description of devices\" below)", "Keep service, reassign phone (provide special instructions in \"Description of\" below)" }, "Re-assign to:", 2, true, true);

            SECTION.Insert_SingleText("Description of devices:", true, false);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_sepcomp, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_SepForm);

            // ADD TABLE/FORM ELEMENTS

            SECTION.Insert_DropList_WithDisabler("Physical computer:", new string[] { "Computer and attached devices will be reassigned", "This person did not have a computer or any other devices", "Return equipment to IT inventory (we will pick up)", "Equipment is old and needs to be removed" }, new bool[] { false, true, false, false }, false, true);
            SECTION.Insert_RadioTextOnSelect(STYLE_AddHelpButton("Reassign Devices:", "If IT finds any remaining devices belonging to this employee, please mark how we should proceed."), new string[] { "Reassign any remaining devices to another employee", "Mark the devices as Not Assigned for your department", "Contact me about these devices or see notes below" }, "Reassign to:", 0, true, true);
            SECTION.Insert_SingleText("Equipment notes:", true, false);
            FUNCT_DONOTEMAIL_AddFlag(DoNotEmailCheck);
            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(FORM_TransForm, EPSILON);

            // ADD TABLE/FORM ELEMENTS
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_BiText("Former supervisor:", FS_Name[0], FS_Name[2], true);
            SECTION.Insert_Spacer();
            SECTION.Insert_TriText("Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            SECTION.Insert_SingleText("New job title/classification:", false, true);
            SECTION.Insert_SingleText("Phone extension (Optional):", false, false);
            SECTION.Insert_CascadeDataSourceDDL("New department:", SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', false);
            SECTION.Insert_SingleText("New location:", false, true);
            SECTION.Insert_SingleText("New mailstop:", false, false);

            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            // 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("EMPTRANS_EQUIPMENT", "Computer Equipment");
            SECTION.addEmailHeader("Employee Transfer - This ticket is for the equipment: \r\n\r\n1. Please transfer the users equipment from old location to new if it is to be moved. If the VOIP Phone was moved, please create a ticket for updating billing information for that phone.");
            SECTION.addEmailSections(new string[] { FORM_TransForm, SUBFORM_transdate, SUBFORM_transcomp, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
            //2. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_ADMIN);
            SECTION.addEmailHighlight("EMPTRANS_EXCHANGE", "Network and Email");
            SECTION.addEmailHeader("Employee Transfer - This ticket is for employee accounts. Please perform the following: \r\n\r\n"
                + "ADMIN:\r\n"
                + "1. Allocate o365 license to new department\r\n"
                + "2. Check for a license change\r\n"
                + "3. If NO LICENSE CHANGE is needed, transfer this ticket to " + FID_Region + " in Kace\r\n"
                + "3. If YES LICESNE Change is needed, transfer this ticket to Server Team (TIS) for processing\r\n"
                + "\r\n"
                + "SERVER TEAM:\r\n"
                + "1. Assign o365 license\r\n"
                + "2. Transfer this ticket to " + FID_Region + " in Kace\r\n"
                + "\r\n"
                + FID_Region + ":\r\n"
                + "1. Please transfer the user's account to the new COFS->DEPT folder in AD (if needed) and update with the following:\r\n"
                + "1.A. Change the user's Logon Script under the PROFILE tab in the AD account properties\r\n"
                + "1.B. Transfer the user's Home Directory to match the correct path for the logon script. (may need to move from server to server)\r\n"
                + "1.C. Change the user's group memberships in the MEMBER OF tab in the AD account properties\r\n"
                + "1.D. Change JOB TITLE and DEPARTMENT in the ORGANIZATION tab in the AD account properties\r\n"
                + "1.E. Change OFFICE in the GENERAL tab in the AD account properties\r\n\r\n"
                + "2. Update the email account information in Exchange ECP with the following:\r\n"
                + "2.A. WORK PHONE (under CONTACT INFORMATION)\r\n"
                + "2.B. TITLE and DEPARTMENT (under ORGANIZATION)\r\n"
                );
           

            SECTION.addEmailSections(new string[] { FORM_TransForm, SUBFORM_transdate, SUBFORM_transemail, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");
           
            //3. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_Accela);
            SECTION.addEmailHighlight("EMPTRANS_ACCELA", "Accela Annie");
            SECTION.addEmailHeader("Employee Transfer - This email is for Accela Annie.\r\n\r\nPlease note the following employee below has transferred to a new position:");
            SECTION.addEmailSections(new string[] { FORM_TransForm, SUBFORM_transdate, CelNotes });
            SECTION.addEmailFooter("*The 'additional notes' are generic for this form and may not pertain to the work your division is performing.*");


            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            //// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            //// DECLARE TABLE
            //ITEM_addNewSection(FORM_Cham, "");

            //// ADD TABLE/FORM ELEMENTS
            //// ADD TABLE/FORM ELEMENTS
            //SECTION.Insert_Information("*Important*", "Please do not fill out this form for yourself. This form should be submitted by the employee's supervisor or manager only.", FS_NoteColor);

            //SECTION.Insert_DropList(STYLE_AddHelpButton("Request Type:", "Select Applicable Access"), new string[] { "New User", "Change Current User", "Re-enable User" }, false, true);
            //SECTION.set_ClearOnSubmit_NextItem();
            //SECTION.Insert_TriText("Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            //SECTION.Insert_SingleText("Computer Login ID:", false, true);
            //SECTION.Insert_SingleText("Email Address:", false, true);
            //SECTION.Insert_SingleText("Job title:", false, true);
            //SECTION.Insert_SingleText("Phone number:", false, true);
            //SECTION.Insert_SingleText("Dept./Organization:", false, true);
            //SECTION.Insert_SingleText("Location (Address):", false, true);
            //SECTION.Insert_Spacer();
            //SECTION.Insert_RadioField("Is this a volunteer?", new string[] { "NO", "YES" }, true);
            //SECTION.Insert_Spacer();
            //SECTION.Insert_SingleText("Current staff member in your workgroup that access can be modeled after:", false, false);
            //SECTION.Insert_Spacer();
            //SECTION.Insert_DateDDL("Effective Date:");
            //SECTION.Insert_SingleText("Reason for new access or modification to current access level:", true, true);

            //// CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            ////1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            ////FORMTYPE = Employee New Hire
            //SECTION.addEmailForm(EMAIL_REG_DT);
            //SECTION.addEmailHighlight("CHAMELEON_ACCESS", "CHAMELEON ACCESS REQUEST");
            //SECTION.addEmailHeader(FORM_Cham + " - DT: Please process the following form:\r\n");
            //SECTION.addEmailSections(new string[] { FORM_Cham, SUBFORM_chamsuper, CelNotes });
            //SECTION.addEmailFooter("*The 'additional notes' are generic and will be included in all emails.*");

            ////2. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            ////FORMTYPE = Employee New Hire
            //SECTION.addEmailForm("nlo@cityofsacramento.org");
            //SECTION.addEmailHighlight("CHAMELEON_ACCESS", "CHAMELEON ACCESS REQUEST");
            //SECTION.addEmailHeader(FORM_Cham + " - (Nicole's copy):\r\n");
            //SECTION.addEmailSections(new string[] { FORM_Cham, SUBFORM_chamsuper, CelNotes });
            //SECTION.addEmailFooter("*The 'additional notes' are generic and will be included in all emails.*");

            ////3. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            ////FORMTYPE = Employee New Hire
            //SECTION.addEmailForm("mchacon@cityofsacramento.org");
            //SECTION.addEmailHighlight("CHAMELEON_ACCESS", "CHAMELEON ACCESS REQUEST");
            //SECTION.addEmailHeader(FORM_Cham + " - (Mollie's copy):\r\n");
            //SECTION.addEmailSections(new string[] { FORM_Cham, SUBFORM_chamsuper, CelNotes });
            //SECTION.addEmailFooter("*The 'additional notes' are generic and will be included in all emails.*");

            //// CAP THE BOTTOM OF THE TABLE OFF
            //ITEM_endNewSection();

            //// @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            //// DECLARE TABLE
            //ITEM_addNewSection(SUBFORM_chamsuper, "");

            //// DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            //SECTION.Bind(FORM_Cham);

            //// ADD TABLE/FORM ELEMENTS
            //SECTION.Insert_SingleText("Supervisor Name:", false, true);
            //SECTION.Insert_SingleText("Division Manager Name:", false, true);
            //SECTION.Insert_SingleText("Animal Care Services Manager:", false, true);

            //// CAP THE BOTTOM OF THE TABLE OFF
            //ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(FORM_Mobile, "");

            // ADD TABLE/FORM ELEMENTS
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("User's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            SECTION.Insert_CascadeDataSourceDDL("User's department:", SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', false);
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_SingleText("Phone extension (Optional):", false, false);
            SECTION.set_ClearOnSubmit_NextItem();
            //SECTION.Insert_EmailWithSetDomains(STYLE_AddHelpButton(EmpEmailIdentifier, "Only enter what is before the \"@\" symbol in the text box, then select a @domain"), GLOBAL_COFSDomains, true);
            //SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_Spacer();

            string[] DevOpt = new string[] {
            
                "New Device - AT&T, Smartphone",
                "New Device - AT&T, Tablet",
                "New Device - AT&T, Other Device",
                //"New Device - Sprint, Smartphone",
                //"New Device - Sprint, Tablet",
                //"New Device - Sprint, Other Device",
                "New Device - Verizon, Smartphone",
                "New Device - Verizon, Tablet",
                "New Device - Verizon, Other Device",

                "Upgrade Device - AT&T, Smartphone",
                "Upgrade Device - AT&T, Tablet",
                "Upgrade Device - AT&T, Other Device",
                //"Upgrade Device - Sprint, Smartphone",
                //"Upgrade Device - Sprint, Tablet",
                //"Upgrade Device - Sprint, Other Device",
                "Upgrade Device - Verizon, Smartphone",
                "Upgrade Device - Verizon, Tablet",
                "Upgrade Device - Verizon, Other Device",

                "Transfer Device - AT&T, Smartphone",
                "Transfer Device - AT&T, Tablet",
                "Transfer Device - AT&T, Other Device",
                //"Transfer Device - Sprint, Smartphone",
                //"Transfer Device - Sprint, Tablet",
                //"Transfer Device - Sprint, Other Device",
                "Transfer Device - Verizon, Smartphone",
                "Transfer Device - Verizon, Tablet",
                "Transfer Device - Verizon, Other Device",

                "Change Service - AT&T, Smartphone",
                "Change Service - AT&T, Tablet",
                "Change Service - AT&T, Other Device",
                //"Change Service - Sprint, Smartphone",
                //"Change Service - Sprint, Tablet",
                //"Change Service - Sprint, Other Device",
                "Change Service - Verizon, Smartphone",
                "Change Service - Verizon, Tablet",
                "Change Service - Verizon, Other Device"
                
            };

            SECTION.Insert_CascadeDDL("Device:", DevOpt, '-');
            SECTION.Insert_SingleText("Request and Device:", true, true);

            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            // 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("CELL_REQUEST", "Cellular Request");
            SECTION.addEmailHeader("Cellular Device Request: \r\n\r\n");
            SECTION.addEmailSections(new string[] { FORM_Mobile });
            SECTION.addEmailFooter("");


            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            //ITEM_addNewSection(SUBFORM_transformersup, "");

            //// DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            //SECTION.Bind(FORM_TransForm);

            //// ADD TABLE/FORM ELEMENTS

            ////SECTION.Insert_SingleText("Email:", false, true);
            ////SECTION.Insert_CascadeDataSourceDDL("Department:", SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', false);
            ////SECTION.Insert_SingleText("Contact number:", false, true);

            //// CAP THE BOTTOM OF THE TABLE OFF
            //ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_transdate, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_TransForm);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_DateDDL("Active as of:");
            SECTION.Insert_SingleText("Special Instructions:", true, false);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_transcomp, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_TransForm);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_Information("*Important*", "If the employee needs a new phone or computer, you can check those options at the top of this form.", FS_NoteColor);
            //SECTION.Insert_Spacer();
            SECTION.Insert_RadioField("Current Phone:", new string[] { "Needs to be transferred", "Does not have a phone", "Will not be transferred" }, true);
            SECTION.Insert_RadioField("Current Computer:", new string[] { "Needs to be transferred", "Does not have a computer", "Will not be transferred" }, true);
            SECTION.Insert_SingleText("Phone extension and instructions:", true);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            ITEM_addNewSection(SUBFORM_transemail, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_TransForm);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_Information("*Important*", "All current email groups that include this person when sending email to that group will no longer do so. If you know of any groups for which the employee will need to receive email, please indicate them in this section.", FS_NoteColor);
            SECTION.Insert_Spacer();
            SECTION.Insert_SingleText("Add to email groups:", true);
            SECTION.Insert_SingleText("Remove access to these calendars:", true);
            SECTION.Insert_SingleText("Add access to these calendars:", true);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_hirecomp, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_HireForm);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_DropList_WithDisabler("Computer Options:",
                new string[] { "Assign an existing computer to this employee (please give machine name)", "Employee will use a shared computer already in use", "Employee will receive a new computer (please select \"" + FORM_PCQuote.Replace(":", "") + "\" from above)" },
                new bool[] { false, true, true },
                false, true);
            SECTION.Insert_SingleText(STYLE_AddHelpButton("Computer Name:", "If this employee will use an existing computer, please provide either the machine name (on front of PC) or the asset tag (on top or back)"), false, true);
            SECTION.Insert_SingleText("Special Instructions:", true);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_hireoffice, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_HireForm);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_Information("Upcoming MS Office 365", "For our upcoming change to Microsoft Office 365, different types of licenses are available. Please read the description and select from the following:", FS_NoteColor);
            SECTION.Insert_Spacer();
            SECTION.Insert_Information("", "<strong>G3 (Approximately $150 per year per user)</strong><br><ul><li>Includes MS Office applications for computer, tablet, and smartphone</li><li>Includes a City email account; email can sync with smartphone and tablet</li></ul>", FS_GoldStandard);
            SECTION.Insert_Information("", "<strong>G1 (Approximately $100 per year per user)</strong><br><ul><li>Online only versions of MS Word, Excel, PowerPoint, Outlook available</li><li>No installed MS Office applications for computer, smartphone, or tablet</li><li>Includes a City email account;  email can sync with smartphone and tablet</li></ul>", FS_SilverStandard);
            SECTION.Insert_RadioVerticalField("Microsoft Office License:", new string[] { "G3 - $150 per year per user (Some features not available until summer, 2016)", "G1 - $100 per year per user (online only, email can sync with smartphone and tablet)", "Network logon only.  No email account or Microsoft Office access needed." }, true);
            //STYLE_AddHelpButton("Microsoft Office License:", "G3 (Approximately $150 per year per user)<br><ul><li>Fully installed MS Office applications available on their computer.</li><li>MS Office applications for tablets and phones available.</li></ul><br>G1 (Approximately $100 per year per user)<br><ul><li>Only online versions MS Word, Excel, PowerPoint, Outlook available.</li><li>No installed MS Office applications on computer.</li><li>No MS Office on tablets or phones.</li></ul>"),

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(FORM_PCQuote, "");

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_CascadeDDL("Computer type:", new string[] {
                     "Desktop Computer - no monitors needed", 
                     "Desktop Computer - with one monitor",
                     "Desktop Computer - with two monitors",

                     "Laptop Computer - Standard (cost effective)",
                     "Laptop Computer - Ultra Portable (thinner and lighter)", 
                     "Laptop Computer - Portable with a touch screen",
                     "Laptop Computer - Tablet hybrid (removable keyboard)",
                 },
                 '-'
            );

            SECTION.Insert_CheckVerticalField("Additional options:", new string[] {
                "Laptop docking station and monitor",
                "Ergonomic wave keyboard"},
            false, false);

            SECTION.Insert_CheckVerticalField("PC will be used for:", new string[]{
                "Microsoft Office, Adobe Acrobat, or other applications",
                "Web applications such as eCaps, 7i, CCM, etc",
                "Autocad and/or GIS type of applications"
            }, false, true);

            SECTION.Insert_SingleText(STYLE_AddHelpButton("Questions or instructions:", "If you would like to request specific items to add or add additional computers, you can tell us here."), true);

            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            // 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("REQ_PC", "PC Quote");
            SECTION.addEmailHeader("This ticket is to request a quote for a new computer. Please take this information and provide a quote based upon the answers for what machine would best suit their needs.");
            SECTION.addEmailSections(new string[] { FORM_PCQuote });
            SECTION.addEmailFooter("If you have any questions about what kind of computer to order, please speak with either Kirk Rexin on your supervisor.");

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            //// DECLARE TABLE
            ITEM_addNewSection(FORM_MSOHUP, EPSILON);

            ////// ADD TABLE/FORM ELEMENTS
            //SECTION.Insert_Information("*Important*", "Thank you for your interest in the Microsoft Home Use Program.  We are sorry to report that the program is no longer available.  Our sincerest apologies for the inconvenience.  For other exclusive offers to City employees, check out the Department Advertising section located on the bottom right-hand side of the <a href=\"http://citynet/home/view.cfm\" target=\"_blank\"><b>CityNet homepage</b></a>.", FS_UnavailableColor);
            SECTION.Insert_Information("*Important*", "The Microsoft Home Use Program is once again available to City employees. Thank you for your patience while we upgraded to Office 365, and we again apologize for any inconvenience this may have caused.", FS_NoteColor);
            SECTION.Insert_Spacer();
            SECTION.Insert_Information("CURRENT VERSION:", "<strong>The Current Version of the MSHUP is Microsoft Office 2016.</strong><br><i>(This is the only version available at this time)</i>", FS_GoldStandard);
            SECTION.Insert_Spacer();
            //SECTION.Insert_TrainWreck();
            //SECTION.Insert_CheckCriteriaField("Required:", new string[] { "I agree to these outlined <a href=\"" + URL_MSOHU_ORIG + "\" target=\"_blank\">Terms and Conditions</a>" });
            //SECTION.Insert_Spacer();
            //SECTION.Insert_DateDDL(
            //STYLE_AddHelpButton("Date:", "Date for your MS Home Use purchase (usually today's date)");
            //);
            SECTION.Insert_CheckCriteriaField("Accept", new string[] { "I have read the above and would like the current version."});
            SECTION.Insert_SingleText("Questions or instructions:", true);

            //// CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            //// 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_REG_DT);
            SECTION.addEmailHighlight("REQ_MSHUP", "MSHUP");
            SECTION.addEmailHeader("Please assign to Ryan Gilly in Downtown Queue - this ticket is to request a MS Office Home Use purchase:");
            SECTION.addEmailSections(new string[] { FORM_MSOHUP });
            SECTION.addEmailFooter("");

            ////// CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(FORM_VoipForm, "");

            // ADD TABLE/FORM ELEMENTS

            SECTION.Insert_RadioTextOnSelect("Phone request type:", new string[] { "New desk phone and extension", "New phone extension only (already have phone)", "Transfer phone and extension to another employee", "Return phone and extension to IT (cancels monthly billing)" }, "Existing Phone Extension:", new int[] { 2, 3 }, true, true);
            //SECTION.Insert_RadioVerticalField("Phone request type:", new string[] { "New desk phone and extension", "New phone extension only (already have phone)", "Transfer phone and extension to another employee", "Return phone and extension to IT (cancels monthly billing)" });
            //SECTION.Insert_SingleText("Extension (if not New):", false);
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_BiText("Name as it should appear on extension:", FS_Name[0], FS_Name[2], false, true, true);
            SECTION.Insert_SingleText("Brief description of the work you would like performed:", true, true);

            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            // 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("REQ_VOIP", "Phone Request");
            SECTION.addEmailHeader("This ticket is for a VOIP request:");
            SECTION.addEmailSections(new string[] { FORM_VoipForm, SUBFORM_voipfund });
            SECTION.addEmailFooter("");


            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(FORM_311VoipForm, "");

            // ADD TABLE/FORM ELEMENTS

            SECTION.Insert_RadioField("DOU or 311?", new string[] { "311", "DOU" }, true);
            SECTION.Insert_RadioVerticalField("Agent type:", new string[] { "Supervisor: Supervisor CAD + Agent Line + Verba Login", "Specialist: Supervisor CAD + Agent Line + Verba Login", "Agent:  CAD + Agent Line" }, true);
            SECTION.Insert_Spacer();
            SECTION.Insert_Information("Cisco Agent Line:", "<strong>The Cisco agent (CAD) will include:</strong><br><u><li>Cisco Agent ID with or without Supervisor Role</li><li>Unique Agent Extension</li><li>Device Profile for Cisco Phone</li><li>Verba Recording Profile</li></u>", FS_NoteColor);
            SECTION.Insert_Spacer();
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_SingleText("Brief description of any additional work you would like performed:", true, false);

            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            // 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_REG_SOUTH);
            SECTION.addEmailHighlight("REQ_311VOIP", "Cisco Agent Line Request");
            SECTION.addEmailHeader("This ticket is to request an Agent Line which consists of the following: \r\n1. Regional Engineer creates the agent ID and line as requested, note in ticket if Sup/Spec/Agent \r\n2.Configure switchport and port-security for Verba VLAN \r\n3.Install Cisco Agent Desktop if needed, and verify agent line is working for user \r\n4. If agent, then configure VoIP phone and line appearance (extension) accordingly for Verba \r\n5. If supervisor or specialist then note the IP address of the user to add to ACL before transferring ticket to NCH \r\n\r\n6. Route ticket to Network Core Services \r\n7. If agent, complete Verba recording configurations\r\n8. Add user to respective Verba Sup/Spec/Agent Active Directory group\r\n9. If supervisor or specialist then add IP address to ACL and add DHCP reservation\r\n10. Contact requestor and Cc Regional Engineer  \r\n\r\n Note: If a new VOIP phone is also being requested, that ticket will be in the regional support queue.");
            SECTION.addEmailSections(new string[] { FORM_311VoipForm, SUBFORM_voipfund });
            SECTION.addEmailFooter("");


            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(FORM_VMForm, "");

            string VM_Trans = "Transfer Voicemail to another employee";
            string VM_Del = "Delete this voicemail, phone is being returned";

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_RadioVerticalField("Voicemail:", new string[] { "New Voicemail", VM_Trans, VM_Del });
            SECTION.Insert_SingleText("Extension:", false);
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_BiText("Name for account:", FS_Name[0], FS_Name[2], false, true, true);

            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            // 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("REQ_VM", "Voicemail Request");
            SECTION.addEmailHeader("This ticket is for a Voicemail request:");
            SECTION.addEmailSections(new string[] { FORM_VMForm, SUBFORM_voipfund });
            SECTION.addEmailFooter("");

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();


            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            //// DECLARE TABLE
            //ITEM_addNewSection(FORM_VPN, "");

            //// ADD TABLE/FORM ELEMENTS
            //SECTION.Insert_Information("*Important*", "This form must be filled out by a Manager (verified before completion)", FS_AlertColor);
            //SECTION.Insert_Information("Acknowledgement", "SSL VPN users are responsible for the protection and security of Information Technology Resources accessed while logged on to those resources from outside the City's network. Information Technology Resources shall be protected, to the extent reasonably possible, from misuse, including, but not limited to: theft, unauthorized access and data transfers, fraudulent manipulation or alteration of data, attempts to circumvent the security controls, and any activity that could compromise the integrity or availability of data. Users are prohibited from introducing any unauthorized Information Technology Resources into the City's environment or infrastructure. Furthermore, the introduction of any Information Technology Resources that could disrupt any operations is prohibited. Users are prohibited from violating any established written policies or guidelines that are designed to control or enforce the Information Technology Resource policy.", FS_NoteColor, "I Understand and Agree");
            ////SECTION.Insert_CheckCriteriaField("Confirm:", new string[] { "I Understand and Agree" });
            //SECTION.Insert_Spacer();
            //SECTION.set_ClearOnSubmit_NextItem();
            //SECTION.Insert_TriText("Employee's Name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            //SECTION.set_ClearOnSubmit_NextItem();
            //SECTION.Insert_EmailWithSetDomains(STYLE_AddHelpButton(EmpEmailIdentifier, "Only enter what is before the \"@\" symbol in the text box, then select a @domain"), GLOBAL_COFSDomains, true);
            //SECTION.set_ClearOnSubmit_NextItem();
            //SECTION.Insert_CascadeDataSourceDDL("Employee's department:", SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', false);
            //SECTION.set_ClearOnSubmit_NextItem();
            //SECTION.Insert_SingleText(STYLE_AddHelpButton("Employee's Login ID:", "ECOFS Domain Login ID used by the employee to log into Windows. Usually First Initial plus Last Name."), false, true);
            //SECTION.Insert_Spacer();
            //SECTION.Insert_Information("Requirement:", "The above employee agrees to take all reasonable precautions to assure the City's internal information, or information that has been entrusted to the City by third parties (such as customers), will not be disclosed to  unauthorized  persons  unless  required  by  law.  At the end of my employment, appointment, or contract, with the City, they agree to return to the City all Information Technology Resources to which they have had access in order to do their job. They understand that they are not authorized to use any Information Technology Resource for non-employment related purposes, nor are at liberty to provide any Information Technology Resource to third parties without the express written consent of the City Manager and/or designee.", FS_NoteColor, "Confirmed with employee");
            //SECTION.Insert_Spacer();
            //SECTION.Insert_Information("Requirement:", "The above employee has access to a copy of the City's Information Technology Resource Policy (API #5000). They have read and understand this policy and its relationship to their job. They understand and agree that violation of the City's Information Technology Resource Policy (API #5000) may be grounds for discipline up to and including termination of employment, and they agree to abide by the Policy as a condition of their employment.", FS_NoteColor, "Confirmed with employee");
            //SECTION.Insert_Spacer();
            //SECTION.Insert_Information("Requirement:", "The above employee understands that written Information Technology Resource Policies will be established for Information Technology Resources, in conjunction with this policy, and that the written policies will be made available by the Information Technology Department on the City's Intranet web site. Information Technology Resource policies will be updated and communicated to all users of the resource.", FS_NoteColor, "Confirmed with employee");
            //SECTION.Insert_Spacer();
            //SECTION.Insert_Information("Requirement:", "The above employee understands and agrees that it is their responsibility to read the policies and all updates as they become available, and they agree to be bound by and adhere to those policies. Printed copies of the current policies are available through the City's Information Security Office.", FS_NoteColor, "Confirmed with employee");
            //SECTION.Insert_Spacer();
            //SECTION.Insert_Information("Requirement:", "The above employee understands that non­compliance may be cause for system privilege revocation, disciplinary action up to and including termination, as well as criminal or civil penalties.", FS_NoteColor, "Confirmed with employee");
            //SECTION.Insert_Spacer();
            //SECTION.Insert_Information("Requirement:", "The above employee also agrees to promptly report all violations or suspected violations of Information Technology Resource Policies and Guidelines to either you or their supervisor, who shall notify the CIO or his or her designee.", FS_NoteColor, "Confirmed with employee");
            //SECTION.Insert_Spacer();
            //SECTION.Insert_SingleText("Please describe the VPN access needed:", true);
            //SECTION.Insert_Spacer();


            //// CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            //// 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //SECTION.addEmailForm(EMAIL_SERVERTEAM);

            //SECTION.addEmailHighlight("REQ_VPN", "SSL VPN Request");
            //SECTION.addEmailHeader("This ticket is for SSL VPN access:");
            //SECTION.addEmailSections(new string[] { FORM_VPN });
            //SECTION.addEmailFooter("");

            //// CAP THE BOTTOM OF THE TABLE OFF
            //ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_voipfund, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_VoipForm);
            SECTION.Bind(FORM_VMForm);
            SECTION.Bind(FORM_311VoipForm);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_DropList_WithDisabler("Are we adding a new Phone<br>line  or Voicemail account?",
                new string[] { "Yes", "No" },
                new bool[] { false, true },
                true, true);
            SECTION.Insert_Spacer();
            SECTION.Insert_BiText("Fund for Account 453011:", "Op Unit:", "Fund:", false, true, true);
            SECTION.Insert_CascadeDataSourceDDL("Department:", SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', false);
            SECTION.Insert_BiText("Project or Grant:", "P/G Number:", "Activity:", false);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(FORM_enterVo, "");

            // ADD TABLE/FORM ELEMENTS
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("Employee's name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            SECTION.Insert_SingleText("Job title/classification:", false, true);
            SECTION.Insert_CascadeDataSourceDDL("Department:", SDS_DEPARTMENT, "DEPTNAME", "DEPTNO", '-', false);
            SECTION.Insert_DateDDL("Start Date:");
            SECTION.Insert_SingleText("Phone extension:", false, false);
            SECTION.Insert_SingleText("Cell Phone:", false, false);
            SECTION.Insert_SingleText("Location:", false, true);

            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            // 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_REG_NORTH);
            SECTION.addEmailHighlight("REQ_enterVo", "enterVo Account Request");
            SECTION.addEmailHeader("This ticket is for an enterVo account:");
            SECTION.addEmailSections(new string[] { FORM_enterVo, SUBFORM_enterVoAccess, CelNotes });
            SECTION.addEmailFooter("");
            // 2. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_JOHNNY);
            SECTION.addEmailHighlight("REQ_enterVo", "enterVo Request");
            SECTION.addEmailHeader("This request is for an enterVo account:");
            SECTION.addEmailSections(new string[] { FORM_enterVo, SUBFORM_enterVoAccess, CelNotes });
            SECTION.addEmailFooter("");

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(SUBFORM_enterVoAccess, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_enterVo);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_SingleText("User access profile needed:", true, "Examples: Cash machine server, Standard Cashier 1/2, Auditor, Operator, Supervisor, etc.", true);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();


            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE


            // DECLARE TABLE
            ITEM_addNewSection(FORM_NameChange, EPSILON);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_Information("*Important*", "The HR Benefits department must be contacted at 5665 to submit the paperwork for an official name change PRIOR to requesting a name change from IT, which will change eCAPS.", FS_AlertColor);
            SECTION.Insert_Information("*About*", "This form will request a name change for IT accounts, such as network user name and email, as well as IT equipment such as a desk phone.", FS_NoteColor);
            SECTION.Insert_Spacer();
            SECTION.Insert_TriText("Current Name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_TriText("New Name:", FS_Name[0], FS_Name[1], FS_Name[2], false, true, false, true);
            SECTION.Insert_CheckVerticalField("Items needing change:", new string[] { "Desk Phone", "Email", "Network Scanner", "Network Folders", "Others - I will specify below" }, true, true);
            SECTION.Insert_SingleText("Other Items:", true);
            SECTION.Insert_Spacer();
            SECTION.Insert_SingleText("Additional Notes:", true);

            // CONFIGURE EMAILS THAT WILL BE SENT UPON SUBMIT
            // 1. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(FID_Region);
            SECTION.addEmailHighlight("REQ_NC", "Name Change Request for using IT Equipment");
            SECTION.addEmailHeader("This ticket is for a Name Change request:");
            SECTION.addEmailSections(new string[] { FORM_NameChange });
            SECTION.addEmailFooter("");
            //2. ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            SECTION.addEmailForm(EMAIL_SERVERTEAM);
            SECTION.addEmailHighlight("REQ_NC", "AD and Exchange - Name Change Request");
            SECTION.addEmailHeader("Name Change - This ticket is for a Name Change request for AD and Exchange. Employee is in '" + FID_Region + "' region, please complete with the below information:");
            SECTION.addEmailSections(new string[] { FORM_NameChange });
            SECTION.addEmailFooter("*Please take note of any network folders or additional things that may need to be changed as well*");

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            //

            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // Below are "CELL" sections, not strongly tied to any one form, but rather exist for all forms to use as needed.

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(CelNotes, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_HireForm);
            SECTION.Bind(FORM_SepForm);
            SECTION.Bind(FORM_BatchSep);
            SECTION.Bind(FORM_enterVo);

            // ADD TABLE/FORM ELEMENTS
            SECTION.set_ClearOnSubmit_NextItem();
            SECTION.Insert_SingleText("Comments:", true);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ TABLE DIVIDE

            // DECLARE TABLE
            ITEM_addNewSection(RegionIdentifier, "");

            // DECLARE ANY OTHER SECTIONS THIS ONE IS BOUND TO (if the other section is visible, so will this one. if none are, then this one will hide)
            SECTION.Bind(FORM_HireForm);
            SECTION.Bind(FORM_TransForm);
            SECTION.Bind(FORM_SepForm);
            SECTION.Bind(FORM_VoipForm);
            SECTION.Bind(FORM_PCQuote);
            SECTION.Bind(FORM_NameChange);
            SECTION.Bind(FORM_Mobile);

            // ADD TABLE/FORM ELEMENTS
            SECTION.Insert_ImgDropList("If your location is not in the map, please choose the region nearest to your location.", EDIT_addRegionsIMG(), new string[] { "North Region " + FID_Region_Options[0], "Downtown Region " + FID_Region_Options[1], "South Region " + FID_Region_Options[2],  }, true, true);
            //SECTION.Insert_ImgDropList("If your location is not in the map, please choose the region nearest to your location.", EDIT_addRegionsIMG(), new string[] { "North Region " + FID_Region_Options[0], "Downtown Region " + FID_Region_Options[1], "South Region " + FID_Region_Options[2], "Public Safety " + FID_Region_Options[3] }, true, true);

            // CAP THE BOTTOM OF THE TABLE OFF
            ITEM_endNewSection();

            FUNCT_addUserOptions();
        }

        private string STYLE_AddHelpButton(string Text, string HelpText)
        {
            return Text + "#H#" + HelpText;
        }

        private void FUNCT_hideAlerts()
        {
            lblSuccess.Visible = false;
            lblSuccessBottom.Visible = false;
            lblError.Visible = false;
            lblErrorBottom.Visible = false;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            FUNCT_hideAlerts();
            FUNCT_CheckCompleteThenContinue();
        }

        private bool FUNCT_ContainsErrorZ(string CheckForm)
        {
            if (CheckForm.Contains("#Z#"))
            {
                lblError.Text = "Please fill out the required fields marked with red below.<br />";
                lblErrorBottom.Text = "Please fill out the required fields marked with red above.<br /><br />";
            }
            else if (CheckForm.Contains("#ZEMAIL#"))
            {
                lblError.Text = "Please enter a valid email in the space marked with red below.<br />";
                lblErrorBottom.Text = "Please enter a valid email in the space marked with red above.<br /><br />";
            }
            else if (CheckForm.Contains("#ZECAPS#"))
            {
                lblError.Text = "Please select an eCaps entry below.<br />";
                lblErrorBottom.Text = "Please select an eCaps entry below above.<br /><br />";
            }
            else if (CheckForm.Contains("#ZECAPSINVALID#"))
            {
                lblError.Text = "Please select a valid eCaps entry above from the drop down list. Selection in drop down must be clicked.<br />";
                lblErrorBottom.Text = "Please select a valid eCaps entry above from the drop down list. Selection in drop down must be clicked.<br /><br />";
            }
            else if (CheckForm.IndexOf(">>>") == CheckForm.LastIndexOf(">>>"))
            {
                lblError.Text = "Please select at least one form action below.<br />";
                lblErrorBottom.Text = "Please select at least one form action above.<br /><br />";
            }
            else if (CheckForm.Contains("#ZCHECKS#"))
            {
                lblError.Text = "Please select an option in the fields marked with red below.<br />";
                lblErrorBottom.Text = "Please select an option in the fields marked with red above.<br /><br />";
            }
            else if (CheckForm.Contains("#ZFIRM#"))
            {
                lblError.Text = "All selections must be checked in the fields marked with red below.<br />";
                lblErrorBottom.Text = "All selections must be checked in the fields marked with red above.<br /><br />";
            }
            else
            {
                return false;
            }

            lblError.Visible = true;
            lblErrorBottom.Visible = true;
            return true;
        }

        private void FUNCT_CheckCompleteThenContinue()
        {
            string CheckForm;

            //IF ecaps request is visible, then check if we must have an entry. (If drop == 0 then we must have an entry OR if ecaps text is not empty then we need to check valid entry)
            try
            {
                if (ecapsISEMPLOYEE.Visible == true && (ecapsISEMPLOYEE.SelectedValue == "0" || !String.IsNullOrEmpty(rcbECAPS.SelectedItem.Value.ToString().Trim())))
                {
                    if (String.IsNullOrEmpty(rcbECAPS.SelectedItem.Value.ToString())) //Is the ecaps text empty? If so then we got here because we require an entry, so throw this error
                    {
                        rcbECAPS.BackColor = System.Drawing.Color.Pink;
                        CheckForm = "#ZECAPS#";
                    }
                    else //otherwise we need to check the text against our database
                    {
                        try
                        {
                            rcbECAPS.Items.FindItemByText(rcbECAPS.Text).Selected = true;
                            rcbECAPS.BackColor = System.Drawing.Color.White;
                            CheckForm = EDIT_createForm_HTML();
                        }
                        catch
                        {
                            rcbECAPS.BackColor = System.Drawing.Color.Pink;
                            CheckForm = "#ZECAPSINVALID#";
                        }
                    }
                }
                else //(If ecaps request is hidden, OR we do not require entry based upon drop down) AND (there is no text entered in ecaps text field) then its okay to continue.
                {
                    rcbECAPS.BackColor = System.Drawing.Color.White;
                    CheckForm = EDIT_createForm_HTML();
                }
            }
            catch
            {
                if (ecapsISEMPLOYEE.Visible == true && ecapsISEMPLOYEE.SelectedValue == "0")
                {
                    rcbECAPS.BackColor = System.Drawing.Color.Pink;
                    CheckForm = "#ZECAPSINVALID#";
                }
                else
                {
                    rcbECAPS.BackColor = System.Drawing.Color.White;
                    CheckForm = EDIT_createForm_HTML();
                }
            }

            if (!FUNCT_ContainsErrorZ(CheckForm))
            {
                string[] CheckForms = EDIT_createForm_RTF();
                bool SecondCheckPass = true;

                foreach (string CheckSub in CheckForms)
                {
                    if (FUNCT_ContainsErrorZ(CheckForm))
                    {
                        SecondCheckPass = false;
                    }
                }

                if (SecondCheckPass)
                {
                    FUNCT_processEmail(CheckForms);
                }
            }

        }

        private void FUNCT_InsertInformation(TableCell SupplyCell, string Info)
        {
            Table myTable = new Table();
            TableRow myRow = new TableRow();
            TableCell myCellSpL = new TableCell();
            TableCell myCellA = new TableCell();
            TableCell myCellSpR = new TableCell();

            myTable.Width = 650;
            myCellA.Width = 550;
            myCellSpL.Width = 50;
            myCellSpR.Width = 50;
            myCellSpL.Text = "&nbsp;";
            myCellSpR.Text = "&nbsp;";

            Label Text = new Label();
            Text.Text = Info;

            myCellSpL.BackColor = FS_NoteColor;
            myCellA.BackColor = FS_NoteColor;
            myCellSpR.BackColor = FS_NoteColor;

            Text.BackColor = FS_NoteColor;
            Text.ForeColor = System.Drawing.Color.White;
            myCellA.Controls.Add(Text);
            myRow.Cells.Add(myCellSpL);
            myRow.Cells.Add(myCellA);
            myRow.Cells.Add(myCellSpR);
            myTable.Rows.Add(myRow);
            SupplyCell.Controls.Add(myTable);
        }

        private void FUNCT_pullUserData(string Form)
        {
            string First = Form.Substring(Form.IndexOf(FS_Name[0]) + FS_Name[0].Length);
            First = First.Substring(0, First.IndexOf("\r\n"));
            FirstName = First.Replace(" ", "");

            string Last = Form.Substring(Form.IndexOf(FS_Name[2]) + FS_Name[2].Length);
            Last = Last.Substring(0, Last.IndexOf("\r\n"));
            LastName = Last.Replace(" ", "");

            try
            {
                string empFirst = Form.Substring(Form.LastIndexOf(FS_Name[0]) + FS_Name[0].Length);
                empFirst = empFirst.Substring(0, empFirst.IndexOf("\r\n"));
                EmpFName = empFirst.Replace(" ", "");

                string empLast = Form.Substring(Form.LastIndexOf(FS_Name[2]) + FS_Name[2].Length);
                empLast = empLast.Substring(0, empLast.IndexOf("\r\n"));
                EmpLName = empLast.Replace(" ", "");
            }
            catch
            {
                EmpFName = "employee";
                EmpLName = "";
            }

            string Email = Form.Substring(Form.IndexOf(EmailIdentifier) + EmailIdentifier.Length);
            UserEmail = FUNCT_catchEmailTypos(Email.Substring(0, Email.IndexOf("\r\n")).Trim());

            try
            {
                string empEmail = Form.Substring(Form.LastIndexOf(EmpEmailIdentifier) + EmpEmailIdentifier.Length);
                EmpEmail = FUNCT_catchEmailTypos(empEmail.Substring(0, empEmail.IndexOf("\r\n")).Trim());

            }
            catch
            {
                EmpEmail = UserEmail;
            }

            //perform check to find common typos here


            string Dept = Form.Substring(Form.IndexOf(DeptIdentifier) + DeptIdentifier.Length);
            UserDept = Dept.Substring(0, Dept.IndexOf("\r\n")).Trim();

            Session["FN"] = FirstName;
            Session["LN"] = LastName;
            Session["UE"] = UserEmail;

            CheckCustomFilters(Form);
        }

        private string FUNCT_catchEmailTypos(string SubmittedEmail)
        {
            try
            {
                string First = SubmittedEmail.Substring(0, SubmittedEmail.IndexOf('@')).ToLower();
                string Last = SubmittedEmail.Substring(SubmittedEmail.IndexOf('@') + 1).ToLower();

                string expect = "cityofsacramento.org";

                if (Last != expect)
                {
                    if (Last.Contains("cityof"))
                        Last = expect;
                    else
                    {
                        int score = 0;

                        for (int X = 0; X < Last.Length && X < expect.Length; X++)
                        {
                            if (Last.Substring(X, 1) == expect.Substring(X, 1)) score += 1;
                        }

                        if (score > 15)
                            Last = expect;
                        else
                        {
                            if (Last.Contains("crocker"))
                            {
                                Last = "crockerartmuseum.org";
                            }
                            else if (Last.Contains("water"))
                            {
                                Last = "waterforum.org";
                            }
                        }
                    }
                }

                return First + '@' + Last;
            }
            catch
            {
                return SubmittedEmail;
            }
        }

        private string EDIT_remDoubleColon(string htmlForm)
        {
            return htmlForm.Replace("::", ":");
        }

        protected void btnHideOne_Click(object sender, EventArgs e)
        {
            SECTION = SECTION.ReturnFirstSection();
            //lblTEST.Visible = true;


            //lblTEST.Text = EDIT_createForm_HTML();
        }

        private void FUNCT_processEmail(string[] Alpha)
        {
            //EDIT_remDoubleColon(CheckForm), FUNCT_pullUserData(CheckForm,false), 
            SmtpClient sendClient = new SmtpClient(EMAIL_SMTP);
            ContentType sendAs = new ContentType(MediaTypeNames.Text.RichText);
            ContentType sendformAs = new ContentType(MediaTypeNames.Text.RichText);

            Attachment[] printCopy = null;
            string FullSubmittedForm = "";
            string FullSubGraphic = "\r\n==== ==== ==== ==== ==== ==== ==== ==== ==== ==== ==== ==== ====\r\n";

            FUNCT_pullUserData(Alpha[0]);

            string DateMark = DateTime.Now.Date.ToShortDateString().Replace("/", "") + "-" + DateTime.Now.TimeOfDay.ToString().Replace(":", "").Replace(".", "");
            DateMark = DateMark.Substring(0, DateMark.Length - 6);




            string[] Beta = null;

            for (int R = 0; R < Alpha.Length; R++)
            {
                if (Alpha[R] != null && Alpha[R].Trim() != "")
                {
                    if (Alpha[R].Contains(FID_Region))
                    {

                        for (int Y = R; Y < Alpha.Length; Y++)
                        {
                            try
                            {
                                Alpha[Y] = Alpha[Y].Replace(FID_Region, SelRegion);
                            }
                            catch
                            { }
                        }
                    }

                    FullSubmittedForm += FullSubGraphic + "(START of FORM " + (R + 1).ToString() + ") - " + Alpha[R].Substring(0, Alpha[R].IndexOf("<!1>")).Replace(":", "").Replace("(send/attach)", "") + ")" + FullSubGraphic + "\r\n\r\n" +
                        Alpha[R] +
                        "\r\n" + FullSubGraphic + "(END of FORM " + (R + 1).ToString() + ") - " + Alpha[R].Substring(0, Alpha[R].IndexOf("<!1>")).Replace(":", "").Replace("(send/attach)", "") + ")" + FullSubGraphic + "\r\n\r\n";
                }
            }

            Attachment attachfullform = null;
            if (FullSubmittedForm != "")
            {
                MemoryStream SS = new MemoryStream();
                StreamWriter WW = new StreamWriter(SS);

                WW.Write(FullSubmittedForm);
                WW.Flush();
                SS.Position = 0;

                sendAs.Name = (FirstName.Substring(0, 1) + LastName + "_" + DateMark + "_all_forms.rtf").ToUpper();
                attachfullform = new Attachment(SS, sendAs);
            }

            string emSubject;
            string formName;
            string highlight;

            lblSuccess.Text = "";
            lblBottomInfo.Text = "";



            for (int X = 0; X < Alpha.Length; X++)
            {
                bool CatchFlag = false;
                string FlagDesc = "";
                foreach (string CheckDNE in DONOTEMAIL_Flag)
                {
                    try
                    {
                        if (Alpha[X].Contains(CheckDNE))
                        {
                            CatchFlag = true;
                            FlagDesc = CheckDNE;
                            break;
                        }
                    }
                    catch
                    {

                    }
                }

                foreach (string CheckDNE in DNE_EmailCopies)
                {
                    try
                    {
                        if (Alpha[X].Contains(CheckDNE))
                        {
                            CatchFlag = true;
                            FlagDesc = CheckDNE;
                            break;
                        }
                    }
                    catch
                    {

                    }
                }

                if (CatchFlag)
                {
                    try
                    {
                        //Update our personal database
                        SDS_FORMS.InsertParameters.Clear();
                        SDS_FORMS.InsertParameters.Add("1", FlagDesc);
                        SDS_FORMS.InsertParameters.Add("2", UserEmail);
                        SDS_FORMS.InsertParameters.Add("3", Alpha[X].Substring(0, Alpha[X].IndexOf("<!1>")).Replace(":", "").Replace("(send/attach)", "").Trim() + ", for: " + EmpFName + " " + EmpLName + " [ID=" + DateMark + "]");
                        SDS_FORMS.InsertParameters.Add("4", DateMark);

                        SDS_FORMS.InsertCommand = "BEGIN INSERT INTO ITINV_TICKETS(SUBDATE, TICKNUM, TICKTEXT, USEREMAIL, TICKSUBJECT, USERNOTIFY, USERTRANSACTION) Values (SYSDATE, null, :1 , :2, :3, 'DNE', :4 ); END;";
                        SDS_FORMS.Insert();

                    }
                    catch
                    {

                    }
                }
                else if (Alpha[X] != null && Alpha[X] != "")
                {
                    try
                    {
                        EmailPullCounter++;

                        Alpha[X] = Alpha[X].Replace("<br>", "");

                        MailMessage genForm = new MailMessage();

                        formName = Alpha[X].Substring(0, Alpha[X].IndexOf("<!1>")).Replace(":", "").Replace("(send/attach)", "");


                        highlight = Alpha[X].Substring(Alpha[X].IndexOf("<!2>") + 4, Alpha[X].IndexOf("<!3>") - (Alpha[X].IndexOf("<!2>") + 4));

                        Alpha[X] = Alpha[X].Replace(highlight, "").Replace("<!3>", "");

                        sendformAs.Name = (FirstName.Substring(0, 1) + LastName + "_" + DateMark + "_" + highlight.Replace(" ", "") + ".rtf").ToUpper();

                        emSubject = formName.Trim() + ", for: " + (formName.Contains(FORM_BatchSep.Replace(":", "")) ? "Multiple Accounts" : EmpFName + " " + EmpLName) + " [ID=" + DateMark + "]";

                        genForm.To.Add(EMAIL_SHADOW);
                        genForm.To.Add(Alpha[X].Substring(Alpha[X].IndexOf("<!1>") + 4, Alpha[X].IndexOf("<!2>") - (Alpha[X].IndexOf("<!1>") + 4)));

                        MemoryStream S = new MemoryStream();
                        StreamWriter W = new StreamWriter(S);

                        string AlphaFlush = Alpha[X].Replace("<!1>", " sent to: ").Replace("<!2>", "");

                        for (int Z = 0; Z < REPLACE_INEMAIL_OG.Length; Z++)
                        {
                            try
                            {
                                AlphaFlush = AlphaFlush.Replace(REPLACE_INEMAIL_OG[Z], REPLACE_INEMAIL_NG[Z]);
                            }
                            catch { }
                        }

                        W.Write(AlphaFlush);
                        W.Flush();
                        S.Position = 0;

                        Attachment attach = new Attachment(S, sendformAs);

                        if (chkPaperForms.Checked)
                        {
                            if (printCopy == null)
                            {
                                printCopy = new Attachment[1];
                                printCopy[0] = attach;

                            }
                            else
                            {
                                Attachment[] addone = new Attachment[printCopy.Length + 1];
                                printCopy.CopyTo(addone, 0);
                                addone[addone.Length - 1] = attach;
                                printCopy = addone;
                            }
                        }

                        genForm.From = new MailAddress(EMAIL_SMTP_FROM);
                        genForm.Subject = emSubject;
                        genForm.IsBodyHtml = false;
                        //"@submitter = " + UserEmail + 

                        genForm.Body = "@submitter = " + UserEmail + (TicketsOnHold ? " \r\n@status = Need More Info " : "") + "\r\n\r\n" + (TicketsOnHold ? "TICKET IS ON HOLD FOR APPROVAL\r\n\r\n Needs approval from: " + TicketsHeldFor + ", This ticket was not submitted by this user, or the user's email was filled out incorrectly. Please check that this form is okay to process.\r\n\r\n" : "") + "Form Submitted: " + sendAs.Name + "\r\n" + AlphaFlush;
                        genForm.Attachments.Add(attach);
                        if (attachfullform != null)
                        {
                            genForm.Attachments.Add(attachfullform);
                        }


                        //Update our personal database
                        SDS_FORMS.InsertParameters.Clear();
                        SDS_FORMS.InsertParameters.Add("1", genForm.Body.ToString());
                        SDS_FORMS.InsertParameters.Add("2", UserEmail);
                        SDS_FORMS.InsertParameters.Add("3", genForm.Subject.ToString());
                        SDS_FORMS.InsertParameters.Add("4", DateMark);


                        bool AffectDNE = false;
                        foreach (string R in DNE_EmailCopies)
                        {
                            if (genForm.To[0].Address == R && genForm.To.Count < 3)
                            {
                                AffectDNE = true;
                                break;
                            }
                        }

                        if (AffectDNE)
                        {
                            SDS_FORMS.InsertCommand = "BEGIN INSERT INTO ITINV_TICKETS(SUBDATE, TICKNUM, TICKTEXT, USEREMAIL, TICKSUBJECT, USERNOTIFY, USERTRANSACTION) Values (SYSDATE, null, :1 , :2, :3, 'DNE', :4 ); END;";
                        }
                        else
                        {
                            SDS_FORMS.InsertCommand = "BEGIN INSERT INTO ITINV_TICKETS(SUBDATE, TICKNUM, TICKTEXT, USEREMAIL, TICKSUBJECT, USERNOTIFY, USERTRANSACTION) Values (SYSDATE, null, :1 , :2, :3, 'PEND', :4 ); END;";
                        }

                        sendClient.Send(genForm);

                        try
                        {
                            lblSuccess.Text += formName + " sent to " + genForm.To[genForm.To.Count - 1] + " (" + highlight.Substring(highlight.IndexOf("_") + 1) + ")<br />";
                        }
                        catch
                        {
                            lblSuccess.Text += formName + " sent to " + genForm.To[genForm.To.Count - 1] + " (Success) <br />";
                        }

                        SDS_FORMS.Insert();


                        if (Alpha[X].Contains(FORM_VPN))
                        {
                            if (Alpha[X].Contains(FORM_VPN))
                            {
                                //email the user the requirements
                                genForm.Subject = "SSL VPN ACKOWLEDGEMENT NOTIFICATION";
                                genForm.To.Add(EmpEmail);
                                genForm.Body = "By using the City's SSL VPN you (the user) agree to the following:" +
                                    "\r\n\r\n" +
                                    "I, the user, agree to take all reasonable precautions to assure the City's internal information, or information that has been entrusted to the City by third parties (such as customers), will not be disclosed to  unauthorized  persons  unless  required  by  law.  At the end of my employment, appointment, or contract, with the City, I agree to return to the City all Information Technology Resources to which I have had access in order to do my job. I understand that I am not authorized to use any Information Technology Resource for non-employment related purposes, nor am I at liberty to provide any Information Technology Resource to third parties without the express written consent of the City Manager and/or designee." +
                                    "\r\n\r\n" +
                                    "I have access to a copy of the City's Information Technology Resource Policy (API #5000, Attached to this email). I have read and understand this policy and its relationship to my job. I understand and agree that violation of the City's Information Technology Resource Policy (API #5000) may be grounds for discipline up to and including termination of my employment, and I agree to abide by the Policy as a condition of my employment." +
                                    "\r\n\r\n" +
                                    "I understand that written Information Technology Resource Policies will be established for Information Technology Resources, in conjunction with this policy, and that the written policies will be made available by the Information Technology Department on the City's Intranet web site. Information Technology Resource policies will be updated and communicated to all users of the resource." +
                                    "\r\n\r\n" +
                                    "I understand and agree that it is my responsibility to read the policies and all updates as they become available, and I agree to be bound by and adhere to those policies. Printed copies of the current policies are available through the City's Information Security Office." +
                                    "\r\n\r\n" +
                                    "I understand that non­compliance may be cause for system privilege revocation, disciplinary action up to and including te1mination, as well as criminal or civil penalties." +
                                    "\r\n\r\n" +
                                    "I also agree to promptly report all violations or suspected violations of Information Technology Resource Policies and Guidelines to my supervisor, who shall notify the CIO or his or her designee." +
                                    "\r\n\r\n" +
                                    "\r\n\r\n";
                                genForm.Attachments.Clear();

                                genForm.Attachments.Add(new Attachment(URL_API_5000));
                                genForm.Attachments.Add(new Attachment(URL_VPN_HowTo));

                                sendClient.Send(genForm);

                                //Update our personal database
                                SDS_FORMS.InsertParameters.Clear();
                                SDS_FORMS.InsertParameters.Add("1", genForm.Body.ToString());
                                SDS_FORMS.InsertParameters.Add("2", UserEmail);
                                SDS_FORMS.InsertParameters.Add("3", genForm.Subject.ToString());
                                SDS_FORMS.InsertParameters.Add("4", DateMark);


                                SDS_FORMS.InsertCommand = "BEGIN INSERT INTO ITINV_TICKETS(SUBDATE, TICKNUM, TICKTEXT, USEREMAIL, TICKSUBJECT, USERNOTIFY, USERTRANSACTION) Values (SYSDATE, null, :1 , :2, :3, 'SENT', :4 ); END;";


                                SDS_FORMS.Insert();
                            }
                        }

                        S.Close();
                        W.Close();
                    }

                    catch (Exception E)
                    {
                        MailMessage genForm = new MailMessage();
                        genForm.To.Add(EMAIL_SHADOW);
                        genForm.From = new MailAddress(EMAIL_SMTP_FROM);
                        genForm.Subject = "Error on forms, Pull Count: " + EmailPullCounter.ToString();
                        genForm.Body = E.ToString() + "\r\n\r\nAlpha:" + Alpha[X];
                        sendClient.Send(genForm);
                    }
                }
            }

            //foreach (string R in Beta)
            //{
            //    lblBottomInfo.Text += R;
            //}


            //if (chkPaperForms.Checked)
            //{
            //    MailMessage gentForm = new MailMessage();
            //    gentForm.To.Add(UserEmail);
            //    gentForm.From = new MailAddress(EMAIL_SMTP_FROM);
            //    gentForm.Subject = "Printable copies of your forms sent to IT";

            //    for (int R = 0; R < printCopy.Length; R++)
            //    {
            //        gentForm.Attachments.Add(printCopy[R]);
            //    }

            //    gentForm.Body = "The attached documents were generated by the IT web forms with the information you submitted. "
            //        + "\r\n\r\n"
            //        + "It is broken up into different parts so that our different IT teams can perform their respective part to complete your request in a timely manner."
            //        + " You will receive an email from our ticket system when each ticket is closed, which indicates when each part has been done."
            //        + " When all tickets have been closed, your request should be fully completed. Please contact us at x7111 if you need additional help, or if anything is missing. "
            //        + "\r\n\r\n"
            //        + "\r\n\r\n"
            //        + "Your feedback is our best tool in our continuing efforts to make this process better. If you would like to leave us a comment or if you have an idea for these forms, please email Kirk Rexin at krexin@cityofsacramento.org.";

            //    //sendClient.Send(gentForm);
            //}

            lblSuccessBottom.Text = lblSuccess.Text + "<br /><br />";
            lblSuccess.Visible = true;
            lblSuccessBottom.Visible = true;

            FUNCT_AT_ToggleComments(true);
        }

        private string EDIT_createForm_HTML()
        {
            FormSection Transverse = SECTION.ReturnFirstSection();

            string OpenGraphic = "--------------------------------------------------------------------<br /> >>> ";
            string CloseGraphic = "<br />--------------------------------------------------------------------";

            string BuildForm = "";
            BuildForm += "<br /><br />";
            BuildForm += OpenGraphic + Transverse.myName + CloseGraphic + "<br />";
            BuildForm += Transverse.RecallFieldDataAsHTML() + "<br /><br />";

            SelectedForms = "";
            while (Transverse.IsNextSection())
            {
                Transverse = Transverse.ReturnNextSection();

                if (Transverse.myTable.Visible != false)
                {
                    SelectedForms += "[" + Transverse.myName + "]";
                    BuildForm += OpenGraphic + Transverse.myName + CloseGraphic + "<br />";
                    BuildForm += Transverse.RecallFieldDataAsHTML() + "<br /><br />";
                }
            }

            SelRegion = (BuildForm.Contains(FID_Region_Options[0]) ? EMAIL_REG_NORTH : (BuildForm.Contains(FID_Region_Options[2]) ? EMAIL_REG_SOUTH : EMAIL_REG_DT));

            return BuildForm;
        }

        private string[] EDIT_createForm_RTF()
        {
            FormSection Transverse = SECTION.ReturnFirstSection();
            FormSection Retroverse = Transverse;

            //string[] Alpha = new string[20];
            string[] Beta = new string[20];

            //int AlphaTrack = 0;
            int BetaTrack = 0;

            int SectTrack = 0;
            string SectData = "";

            string[] AllBindings;
            string[] getSections;

            string OpenGraphic = "--------------------------------------------------------------------\r\n >>> ";
            string CloseGraphic = "\r\n--------------------------------------------------------------------";

            string BuildForm = "";
            BuildForm += "\r\n\r\n";
            BuildForm += OpenGraphic + Transverse.myName + CloseGraphic + "\r\n";
            BuildForm += Transverse.RecallFieldDataAsRTF();

            //string GatherECAPS = "";


            if (rcbECAPS.Visible == true)
            {
                try
                {
                    if (!String.IsNullOrEmpty(rcbECAPS.SelectedItem.Value.ToString().Trim()))
                    {
                        try
                        {
                            string getECAPS = rcbECAPS.SelectedItem.Value.ToString().Trim();
                            getECAPS = getECAPS.Substring(getECAPS.LastIndexOf("(") + 3);
                            getECAPS = getECAPS.Substring(0, getECAPS.LastIndexOf(")"));
                            while (getECAPS.Length < 7)
                            {
                                getECAPS = "0" + getECAPS;
                            }

                            BuildForm += "\r\n\r\n" + OpenGraphic + "Employee eCaps ID (NOT the submitter)" + CloseGraphic + " \r\n Employee eCaps Information: " + rcbECAPS.SelectedItem.Value.ToString().Trim().Replace("(", " (") + " \r\n Employee ID: " + getECAPS + "\r\n\r\n";
                        }
                        catch
                        {
                            BuildForm += "\r\n\r\n" + OpenGraphic + "Employee eCaps ID (NOT the submitter)" + CloseGraphic + " \r\n Employee eCaps Information: " + rcbECAPS.SelectedItem.Value.ToString().Trim().Replace("(", " (") + "\r\n\r\n";
                        }
                    }
                    else if (ecapsISEMPLOYEE.Visible == true)
                    {
                        BuildForm += "\r\n" + CloseGraphic + "\r\nThis form required an eCaps ID to be submitted, but 'NOT a City employee' or 'HR not in system' was selected, and no eCaps information was given." + CloseGraphic + "\r\n\r\n";
                    }
                    else
                    {
                        BuildForm += "\r\n\r\n";
                    }
                }

                catch
                {
                    if (ecapsISEMPLOYEE.Visible == true)
                    {
                        BuildForm += "\r\n" + CloseGraphic + "\r\nThis form required an eCaps ID to be submitted, but 'NOT a City employee' or 'HR not in system' was selected, and no eCaps information was given." + CloseGraphic + "\r\n\r\n";
                    }
                    else
                    {
                        BuildForm += "\r\n\r\n";
                    }
                }
            }
            //Crawl through each form node to see what forms we are supposed to build with it
            //AlphaTrack = 0;
            while (Transverse.IsNextSection())
            {
                Transverse = Transverse.ReturnNextSection();
                //We are only going to pay attention to the form nodes that are active and visible
                if (Transverse.myTable.Visible != false)
                {
                    //For each form node, get the emails to generate for this node, using it as the top of the form
                    for (int R = 0; R < Transverse.getESectionsCount(); R++)
                    {
                        //Make sure there is space in our Physical form array
                        if (BetaTrack > 19)
                        {
                            string[] Theta = new string[Beta.Length];
                            Beta.CopyTo(Theta, 0);
                            Beta = Theta;
                        }

                        //Set the last element in the array to another email we need to generate, starting with BuildForm, which is the users information
                        //Add to BuildForm the Header information for this particular email/physical form
                        Beta[BetaTrack] = Transverse.myName.Replace(":", "") + " form - " + Transverse.getEmailSubject(R) + "<!1>" + Transverse.getEmailAddress(R) + "<!2>" + Transverse.getEmailHighlight(R) + "<!3>\r\n\r\n" + Transverse.getEmailHeader(R) + "\r\n\r\n" + BuildForm;

                        //Get the number of sections on this digital form that we are going to include in this phsyical form
                        getSections = Transverse.getESections(R);

                        //Track the number of sections so we dont send out a blank email
                        SectTrack = 0;
                        //loop through these sections so that we can grab each one from the digital form
                        for (int X = 0; X < getSections.Length; X++)
                        {
                            //start back at the beginning of our nodes, to pick out the nodes to put in our physical form
                            Retroverse = Transverse.ReturnFirstSection();
                            while (Retroverse.IsNextSection())
                            {
                                Retroverse = Retroverse.ReturnNextSection();

                                //if we find a match, add this digital section to our physical form we are building inside the Beta array and go back to the start
                                //there is no need to continue after finding this since we now have the form section we are looking for, and need to look for the next section
                                if (getSections[X] == Retroverse.myName)
                                {
                                    SectData = Retroverse.RecallFieldDataAsRTF();
                                    Beta[BetaTrack] += (SectData == "") ? "" : OpenGraphic + Retroverse.myName + CloseGraphic + "\r\n" + SectData + "\r\n\r\n";
                                    SectTrack += (SectData == "") ? 0 : 1;
                                    break;
                                }
                            }
                        }

                        //After we have all of our sections into the physical form being built in Beta array, add the footer to this email
                        Beta[BetaTrack] += "\r\n\r\n" + Transverse.getEmailFooter(R);

                        //process some basic formatting to make the email look better
                        Beta[BetaTrack] = EDIT_remDoubleSpace(Beta[BetaTrack]);

                        if (SectTrack > 0)
                        {
                            //Move on to the next physical form we want to build
                            BetaTrack++;
                        }
                        else
                        {
                            Beta[BetaTrack] = "";
                        }
                    }
                }

                Transverse.FormWasSubmitted();
            }

            return Beta;
        }

        private string EDIT_remDoubleSpace(string input)
        {
            return input.Replace("  ", " "); //.Replace("First:", "F:").Replace("Last:", "L:");
        }

        private void Page_Querries()
        {
            // http_this_com?Say=true
            // this generates Request.QueryString["Say"] with a value of "true"

            // http_this/forms.html?Sections=01020304
            // mark all the highlighted sections above as visible along with our default Master section

            //try
            //{
            //    string SubInQuerry = "";

            //    SubInQuerry = Request.QueryString["Sections"];
            //    if (SubInQuerry != null && SubInQuerry != "")
            //    {
            //        string[] Sects = new string[SubInQuerry.Length / 2];


            //    }
            //}
            //catch
            //{

            //}
        }

        private void Page_Load(object sender, EventArgs e)
        {
            Page.MaintainScrollPositionOnPostBack = true;

            BUTTONWINDOW.BackColor = ButtonsBackColor;
            ECAPSWINDOW.BackColor = FS_NoteColor;

            BUILDFORMCONTENT();

            if (Application["ITFORMS_DEPTS"] == null)
            {

                SqlDataSource SDS_DEPT = new SqlDataSource("ConnectionStrings:ITFormsDatabase.ProviderName", "ConnectionStrings:ITFormsDatabase", "SELECT DA.DEPT_SHORT_DESC, D.DEPTNO, D.DEPTNAME FROM &quot;DEPARTMENT&quot; DA, &quot;ITINV_DEPTIDS&quot; D WHERE (D.DID = DA.DID) ORDER BY &quot;DEPTNO&quot;");
                GridView gvDept = new GridView();
                gvDept.DataSource = SDS_DEPT;

                //lblAfterthoughts.Text += gvDept.Rows.Count.ToString();
                lblAfterthoughts.Visible = true;
            }
        }

        private void FUNCT_addUserOptions()
        {
            Label Flavor = new Label();
            Label FlavorCap = new Label();

            Flavor.Text = "I would like to fill out this form to";
            FlavorCap.Text = ": (select all that apply)";


            Flavor.Font.Bold = true;
            Flavor.Font.Underline = true;
            BUTTONWINDOW.Controls.Add(Flavor);
            BUTTONWINDOW.Controls.Add(FlavorCap);

            Table DivOptions = new Table();
            TableRow DivRow = new TableRow();
            TableCell DivLeft = new TableCell();
            //TableCell DivRight = new TableCell();

            DivOptions.Width = 800;
            DivLeft.Width = 800;
            //DivRight.Width = 400;

            ApplySections = new CheckBoxList();
            ApplySections.RepeatColumns = 2;
            ApplySections.Width = 750;


            FormSection BackToFirst = SECTION.ReturnFirstSection().ReturnNextSection();
            //Jumping to section FormSection, the first is not allowed to be hidden

            string[] parents;

            while (true)
            {
                parents = BackToFirst.Bind_ReturnAll();

                if (parents == null)
                {
                    ListItem newItem = new ListItem();

                    if (BackToFirst.mySelectionGroup != "")
                    {
                        if (SelectedGroupings != null)
                        {
                            if (!SelectedGroupings.Contains(BackToFirst.mySelectionGroup))
                            {
                                string[] Epsilon = new string[SelectedGroupings.Length + 1];
                                SelectedGroupings.CopyTo(Epsilon, 0);
                                SelectedGroupings[SelectedGroupings.Length - 1] = BackToFirst.mySelectionGroup;
                            }
                        }
                        else
                        {
                            SelectedGroupings = new string[1];
                            SelectedGroupings[0] = BackToFirst.mySelectionGroup;
                        }


                    }

                    newItem.Text = BackToFirst.myName.Replace(":", "");
                    newItem.Value = BackToFirst.mySelectionGroup;

                    if (BackToFirst.myTable.Visible)
                    {
                        newItem.Selected = true;
                        if (newItem.Text == FORM_HireForm.Replace(":", "")) //  || newItem.Text == FORM_SepForm.Replace(":", ""))
                        {
                            ECAPSWINDOW.Visible = true;
                        }
                    }
                    ApplySections.Items.Add(newItem);

                }


                if (BackToFirst.ReturnNextSection() != null)
                {
                    BackToFirst = BackToFirst.ReturnNextSection();
                }
                else
                {
                    break;
                }
            }

            if (ActiveInGroupings == null)
            {
                ActiveInGroupings = new string[SelectedGroupings.Length];

                for (int X = 0; X < ActiveInGroupings.Length; X++)
                {
                    ActiveInGroupings[X] = "";
                }
            }


            ApplySections.SelectedIndexChanged += new System.EventHandler(this.EVENT_formButton);
            ApplySections.AutoPostBack = true;


            DivLeft.Controls.Add(ApplySections);

            DivRow.Controls.Add(DivLeft);
            //DivRow.Controls.Add(DivRight);
            DivOptions.Controls.Add(DivRow);
            BUTTONWINDOW.Controls.Add(DivOptions);

            Label Spacer = new Label();
            Spacer.Text = "<br />";
            BUTTONWINDOW.Controls.Add(Spacer);


            Label EndFlavor = new Label();
            EndFlavor.Text = "(Note: only some forms can be selected at the same time, all others will uncheck automatically for you)";
            EndFlavor.Font.Size = FontUnit.Smaller;
            EndFlavor.Font.Bold = false;
            EndFlavor.Font.Underline = false;
            BUTTONWINDOW.Controls.Add(EndFlavor);


            Funct_CheckBindings();


        }


        //WORKING
        public void EVENT_formButton(object sender, EventArgs e)
        {
            if (Session["SelGroupings"] != null)
            {
                SelectedGroupings = (string[])Session["SelGroupings"];
            }

            if (Session["ActGroupings"] != null)
            {
                ActiveInGroupings = (string[])Session["ActGroupings"];
            }

            FormSection BackToFirst;
            //Jumping to section FormSection, the first is not allowed to be hidden
            string NeedToUncheck = "";

            for (int R = 0; R < ApplySections.Items.Count; R++)
            {
                BackToFirst = SECTION.ReturnFirstSection().ReturnNextSection();

                while (true)
                {
                    if (BackToFirst.myName.Contains(ApplySections.Items[R].Text))
                    {
                        if (ApplySections.Items[R].Selected)
                        {
                            for (int X = 0; X < SelectedGroupings.Length; X++)
                            {
                                if (SelectedGroupings[X] == ApplySections.Items[R].Value)
                                {
                                    if (ActiveInGroupings[X] == ApplySections.Items[R].Text)
                                    {
                                        break;
                                    }
                                    else if (ActiveInGroupings[X] != ApplySections.Items[R].Text)
                                    {
                                        NeedToUncheck = ApplySections.Items[R].Text;
                                        ActiveInGroupings[X] = NeedToUncheck;

                                        for (int Z = 0; Z < ApplySections.Items.Count; Z++)
                                        {
                                            //EPSILON FOUND
                                            if (ApplySections.Items[Z].Value == SelectedGroupings[X] && NeedToUncheck != ApplySections.Items[Z].Text)
                                            {
                                                ApplySections.Items[Z].Selected = false;
                                                //Session[BackToFirst.myName] = ApplySections.Items[R].Selected;
                                            }
                                        }

                                        NeedToUncheck = "";
                                    }
                                }
                            }
                        }
                        break;
                    }

                    if (BackToFirst.ReturnNextSection() != null)
                    {
                        BackToFirst = BackToFirst.ReturnNextSection();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            bool SHOWECAPSID = false;

            for (int R = 0; R < ApplySections.Items.Count; R++)
            {
                BackToFirst = SECTION.ReturnFirstSection().ReturnNextSection();

                while (true)
                {
                    if (BackToFirst.myName.Contains(ApplySections.Items[R].Text))
                    {
                        BackToFirst.myTable.Visible = ApplySections.Items[R].Selected;
                        Session[BackToFirst.myName] = ApplySections.Items[R].Selected;
                        break;
                    }

                    if (BackToFirst.ReturnNextSection() != null)
                    {
                        BackToFirst = BackToFirst.ReturnNextSection();
                    }
                    else
                    {
                        break;
                    }
                }

                //CODE FOR THE ECAPS CONTROL TO SHOW AND HIDE
                if (ApplySections.Items[R].Text == FORM_HireForm.Replace(":", "")) //  || ApplySections.Items[R].Text == FORM_SepForm.Replace(":", ""))
                {
                    if (ApplySections.Items[R].Selected)
                    {
                        SHOWECAPSID = true;
                    }
                }
            }

            ECAPSWINDOW.Visible = SHOWECAPSID;

            Session["SelGroupings"] = SelectedGroupings;
            Session["ActGroupings"] = ActiveInGroupings;

            Funct_CheckBindings();
        }

        //THIS FUNCTION IS NOT YET WORKING - DOES NOT ACTIVATE CHILDREN
        private void Funct_CheckBindings()
        {
            FormSection BackToFirst = SECTION.ReturnFirstSection().ReturnNextSection(); ;
            //Jumping to section FormSection, the first is not allowed to be hidden

            string[] parents;
            bool escape;

            while (true)
            {
                parents = BackToFirst.Bind_ReturnAll();
                escape = false;
                if (parents != null)
                {
                    FormSection BackAgain = SECTION.ReturnFirstSection().ReturnNextSection(); ;

                    while (!escape)
                    {
                        for (int R = 0; R < parents.Length; R++)
                        {
                            if (BackAgain.myName == parents[R])
                            {
                                if ((bool)Session[BackAgain.myName])
                                {
                                    BackToFirst.myTable.Visible = true;
                                    Session[BackToFirst.myName] = true;
                                    escape = true;
                                    break;
                                }
                                else
                                {
                                    BackToFirst.myTable.Visible = false;
                                    Session[BackToFirst.myName] = false;
                                }
                            }

                        }


                        if (BackAgain.ReturnNextSection() != null)
                        {
                            BackAgain = BackAgain.ReturnNextSection();
                        }
                        else
                        {
                            BackToFirst.myTable.Visible = false;
                            Session[BackToFirst.myName] = false;
                            break;
                        }
                    }
                }

                if (BackToFirst.ReturnNextSection() != null)
                {
                    BackToFirst = BackToFirst.ReturnNextSection();
                }
                else
                {
                    break;
                }
            }
        }

        private HyperLink EDIT_addRegionsIMG()
        {
            Image ThisImg = new Image();
            ThisImg.ImageUrl = "/html/images/citymaphd.png";
            ThisImg.AlternateText = "Regional support map of the City of Sacramento";
            ThisImg.Height = 250;
            ThisImg.BorderStyle = BorderStyle.Ridge;
            ThisImg.BorderWidth = 1;
            ThisImg.BorderColor = System.Drawing.Color.SteelBlue;
            ThisImg.Width = 300;

            HyperLink LinkImg = new HyperLink();
            LinkImg.Controls.Add(ThisImg);
            LinkImg.NavigateUrl = "/html/images/citymaphd.png";
            LinkImg.Target = "_blank";

            return LinkImg;
        }

        private void ITEM_addNewSection(string Name, string SelectionGroup)
        {
            if (SECTION == null)
            {
                SECTION = new FormSection(Name, SelectionGroup);
            }
            else
            {
                SECTION.ITEM_addNewSection(Name, SelectionGroup);
                SECTION = SECTION.ReturnNextSection();
            }
        }

        private void ITEM_addNewSection(string Name, int BindMe)
        {
            SECTION.ITEM_addNewSection(Name, "", BindMe);
            SECTION = SECTION.ReturnNextSection();
        }

        private void FinishMainSection()
        {
            MAINWINDOW.Controls.Add(SECTION.myTable);
            CurTable++;
        }

        private void ITEM_endNewSection()
        {
            //Add Spacer
            SECTION.Insert_Spacer();

            // Check Session Variables
            if (Session[SECTION.myName] != null)
            {
                SECTION.myTable.Visible = (bool)Session[SECTION.myName];
            }
            else
            {
                Session[SECTION.myName] = SECTION.myTable.Visible;
            }

            HIDESWINDOW.Controls.Add(SECTION.myTable);
            CurTable++;
        }

        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% BUTTON FUNCTIONS %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% BUTTON FUNCTIONS %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% BUTTON FUNCTIONS %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

        private void FUNCT_AT_ToggleComments(bool OffOn)
        {
            AFTERTHOUGHTS.Visible = OffOn;
            MASTER.Visible = !OffOn;
        }

        public void FUNCT_AT_SendEmail()
        {
            if (txtAfterthoughts.Text.Replace(" ", "") != "")
            {
                MailMessage genForm = new MailMessage();
                SmtpClient sendClient = new SmtpClient("mail.cityofsacramento.org");

                genForm.IsBodyHtml = true;

                genForm.To.Add("jzanghi@cityofsacramento.org");
                genForm.To.Add("krexin@cityofsacramento.org");

                //Declarations from above
                //Session["FN"] = FirstName;
                //Session["LN"] = LastName;
                //Session["UE"] = UserEmail;

                if ((string)Session["FN"] != "")
                {
                    genForm.Subject = "FEEDBACK for IT FORMS from " + (string)Session["FN"] + " " + (string)Session["LN"] + "";
                    genForm.Body = (string)Session["FN"] + " " + (string)Session["LN"] + " (" + (string)Session["UE"] + ")<br><br><u>Sent the following:</u><br>" + txtAfterthoughts.Text;
                }
                else
                {
                    genForm.Subject = "FEEDBACK for IT FORMS";
                    genForm.Body = txtAfterthoughts.Text;
                }

                genForm.From = new MailAddress("noreply@CityOfSacramento.org");
                sendClient.Send(genForm);
            }
        }

        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% BUTTON CLICKS %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% BUTTON CLICKS %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% BUTTON CLICKS %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

        protected void btnATcancel_Click(object sender, EventArgs e)
        {
            FUNCT_AT_ToggleComments(false);
        }

        protected void btnATsubmit_Click(object sender, EventArgs e)
        {
            FUNCT_AT_SendEmail();
            FUNCT_AT_ToggleComments(false);
        }
    }


}



namespace Zanghi_ASPForms
{
    // Made by Jason Zanghi. This code is here to actuate the dynamic form pages, all of which will share these classes

    //=================================================================================================================================================
    //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ CODE TO MAKE DUMMY TABLE FOR DYNAMIC FORMS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    //=================================================================================================================================================


    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ MAKE TABLES CLASS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

    //public class eFormsManager
    //{
    //    emailForms[] Nevada;

    //    
    //}



    //\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\

    public class MakeTable
    {
        TableRow InsertRow = new TableRow();
        TableCell InsertCellA = new TableCell();
        TableCell InsertCellB = new TableCell();

        ImageButton HelpButton = new ImageButton();
        Label HelpDialog = new Label();

        public MakeTable(Table SupplyTable, string Header)
        {
            InsertCellA.ColumnSpan = 2;
            InsertCellA.Controls.Add(ReturnLabel_Header(Header));
            InsertCellA.Width = 900;
            InsertCellA.Height = 55;
            InsertRow.Cells.Add(InsertCellA);
            SupplyTable.Rows.Add(InsertRow);
        }

        public void TopAlign()
        {
            InsertCellA.VerticalAlign = VerticalAlign.Top;
        }

        private void BuildHelpIMG(string R)
        {
            HelpButton.ImageUrl = "/html/images/helpbutton.png";
            HelpButton.AlternateText = "help button";
            HelpButton.ImageAlign = ImageAlign.AbsMiddle;
            HelpButton.Height = 17;
            HelpButton.BorderStyle = BorderStyle.None;
            HelpButton.BorderWidth = 0;
            HelpButton.Width = 17;
            HelpButton.ToolTip = R;

            HelpDialog.Text = R;
            HelpDialog.Width = 180;
            HelpDialog.BorderColor = System.Drawing.Color.Black;
            HelpDialog.BorderWidth = 1;
            HelpDialog.BorderStyle = BorderStyle.Solid;
            HelpDialog.BackColor = System.Drawing.Color.LightGreen;
            HelpDialog.Font.Size = 8;
            HelpDialog.Visible = false;

            HelpButton.Click += new System.Web.UI.ImageClickEventHandler(this.HelpCommand);

            InsertCellA.Controls.Add(HelpButton);
            InsertCellA.Controls.Add(HelpDialog);
        }

        public void HelpCommand(object sender, EventArgs e)
        {
            HelpDialog.Visible = HelpDialog.Visible ? false : true;
        }

        public MakeTable(string Header)
        {

            if (Header.Contains("#H#"))
            {
                InsertCellA.Controls.Add(ReturnLabel_Strong(Header.Substring(0, Header.IndexOf("#H#")) + "&nbsp;&nbsp;"));

                try
                {
                    BuildHelpIMG(Header.Substring(Header.IndexOf("#H#") + 3));
                }
                catch
                {

                }
            }
            else
            {
                InsertCellA.Controls.Add(ReturnLabel_Strong(Header));
            }

            InsertCellA.Width = 200;
            InsertCellB.Width = 700;

            InsertCellA.VerticalAlign = VerticalAlign.Middle;

            InsertCellB.HorizontalAlign = HorizontalAlign.Justify;
            InsertCellB.VerticalAlign = VerticalAlign.Middle;
            InsertCellA.Height = 35;
        }

        public TableCell ReturnBodyCell()
        {
            return InsertCellB;
        }

        public void FinishTable(Table SupplyTable)
        {
            InsertRow.Cells.Add(InsertCellA);
            InsertRow.Cells.Add(InsertCellB);
            SupplyTable.Rows.Add(InsertRow);
        }

        //@@@@@@@@@@@@@@@@@@@@ RETURN FUNCTIONS @@@@@@@@@@@@@@@@@@@@@@@

        private Label ReturnLabel_Header(string SupplyText)
        {
            Label InsertText = new Label();
            InsertText.Text = "&nbsp;&nbsp;&nbsp;" + SupplyText + "&nbsp;&nbsp;&nbsp;";
            InsertText.Font.Bold = true;
            InsertText.BorderStyle = BorderStyle.Groove;
            InsertText.BackColor = System.Drawing.Color.WhiteSmoke;
            InsertText.BorderWidth = 1;
            InsertText.Font.Size = 10;
            InsertText.Style.Add("border-radius", "12px");

            return InsertText;
        }

        private Label ReturnLabel_Strong(string SupplyText)
        {
            Label InsertText = new Label();
            InsertText.Text = SupplyText;
            InsertText.Font.Bold = false;
            InsertText.Font.Size = 9;
            return InsertText;
        }

        private Label ReturnLabel(string SupplyText)
        {
            Label InsertText = new Label();
            InsertText.Text = SupplyText;
            InsertText.Font.Size = 10;
            return InsertText;
        }
    }

    //\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\

    public class BindUntoMe
    {
        public BindUntoMe Next;
        public string thisBind;

        public BindUntoMe(string giveBind)
        {
            thisBind = giveBind;
        }
    }

    //\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\

    public class emailForms
    {
        public string zHeader = "";
        public string[] zSections;
        public string zFooter = "";
        public string zEmail = "";
        public string zHighlight = "";
        public string zSubject = "";
        public string[] zCondition;
        public int[] zConditionType;
        public bool zConditionsMet = false;
    }
    //\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\

    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ FORM SECTION CLASS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

    public class FormSection
    {
        FormSection First;
        FormSection Next;
        FormSection Hidable;

        public string myName;
        public string mySelectionGroup;
        BindUntoMe myBind;
        BindUntoMe currentBind;

        public Table myTable;
        public Label myLabel;

        public FormNode FirstItem;
        public FormNode CurrentItem;

        private bool ClearNextItemOnSubmit = false;
        private MakeTable InsertHeader;
        private MakeTable NewTable;

        //################################################################################################################################################
        //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ The BELOW is for the 'physical' form to be sent via email @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        //################################################################################################################################################
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        emailForms[] Nevada;
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public void addEmailForm(string Email)
        {
            if (Nevada == null)
            {
                Nevada = new emailForms[1];
                Nevada[0] = new emailForms();
                Nevada[0].zEmail = Email;
            }
            else
            {
                emailForms[] Washington = new emailForms[Nevada.Length + 1];
                Nevada.CopyTo(Washington, 0);
                Nevada = Washington;
                Nevada[Nevada.Length - 1] = new emailForms();
                Nevada[Nevada.Length - 1].zEmail = Email;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public void addEmailHighlight(string Highlight, string SubjectHighlight)
        {
            if (Nevada != null)
            {
                Nevada[Nevada.Length - 1].zHighlight = Highlight;
                Nevada[Nevada.Length - 1].zSubject = SubjectHighlight;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public void addEmailHeader(string Header)
        {
            if (Nevada != null)
            {
                Nevada[Nevada.Length - 1].zHeader = Header;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public void addEmailSubject(string Subject)
        {
            if (Nevada != null)
            {
                Nevada[Nevada.Length - 1].zSubject = Subject;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public void addEmailFooter(string Footer)
        {
            if (Nevada != null)
            {
                Nevada[Nevada.Length - 1].zFooter = Footer;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public void addEmailSections(string[] Sections)
        {
            if (Nevada != null)
            {
                Nevada[Nevada.Length - 1].zSections = Sections;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public int getESectionsCount()
        {
            try
            {
                return Nevada.Length;
            }
            catch
            {
                return 0;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public string[] getESections(int R)
        {
            return Nevada[R].zSections;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public string getEmailHighlight(int R)
        {
            return Nevada[R].zHighlight;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public string getEmailHeader(int R)
        {
            return Nevada[R].zHeader;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public string getEmailFooter(int R)
        {
            return Nevada[R].zFooter;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public string getEmailSubject(int R)
        {
            return Nevada[R].zSubject;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public string getEmailAddress(int R)
        {
            return Nevada[R].zEmail;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        private void assignCondition(int R, string NameOfField, int ConditionID)
        {
            if (Nevada[R].zCondition == null)
            {
                Nevada[R].zCondition = new string[] { NameOfField };
                Nevada[R].zConditionType = new int[] { ConditionID };
            }
            else
            {
                string[] Washington = new string[Nevada[R].zCondition.Length + 1];
                Nevada[R].zCondition.CopyTo(Washington, 0);
                Washington[Nevada[R].zCondition.Length] = NameOfField;

                Nevada[R].zCondition = Washington;

                int[] Oklahoma = new int[Nevada[R].zCondition.Length + 1];
                Nevada[R].zConditionType.CopyTo(Oklahoma, 0);
                Oklahoma[Nevada[R].zCondition.Length] = ConditionID;

                Nevada[R].zConditionType = Oklahoma;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public void assignCondition_NotEmpty(string NameOfField)
        {
            assignCondition(Nevada.Length - 1, NameOfField, 0);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public void assignCondition_NotUnchecked(string NameOfField)
        {
            assignCondition(Nevada.Length - 1, NameOfField, 1);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public void assignCondition_NotThisValue(string NameOfField, string Value)
        {
            assignCondition(Nevada.Length - 1, NameOfField + "<$>" + Value, 2);
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        public bool getConditionStatus()
        {
            //cycle all items in this section to verify completion of conditions
            return true;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------|||||||
        //################################################################################################################################################
        //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ The ABOVE is for the 'physical' form to be sent via email @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        //################################################################################################################################################

        public FormSection(string SectionHeader, string SelectionGroup, FormSection IsFirst)
        {
            First = IsFirst;
            myName = SectionHeader;
            mySelectionGroup = SelectionGroup;
            Funct_BuildMyTable();
        }

        public FormSection(string SectionHeader, string SelectionGroup)
        {
            First = this;
            myName = SectionHeader;
            mySelectionGroup = SelectionGroup;
            Funct_BuildMyTable();
            myTable.Visible = true;
        }

        public void set_ClearOnSubmit_NextItem()
        {
            ClearNextItemOnSubmit = true;
        }

        public void Bind(string AddToBinds)
        {
            if (myBind == null)
            {
                myBind = new BindUntoMe(AddToBinds);
                currentBind = myBind;
            }
            else
            {
                currentBind.Next = new BindUntoMe(AddToBinds);
                currentBind = currentBind.Next;
            }
        }

        public bool Bind_AmIBound()
        {
            return (myBind == null ? false : true);
        }

        public string[] Bind_ReturnAll()
        {
            if (myBind == null)
                return null;
            else
            {
                BindUntoMe Shadow = myBind;

                int count = 1;
                while (true)
                {
                    if (Shadow.Next != null)
                    {
                        count++;
                        Shadow = Shadow.Next;
                    }
                    else
                    {
                        break;
                    }
                }

                string[] returning = new string[count];
                Shadow = myBind;

                for (int R = 0; R < count; R++)
                {
                    returning[R] = Shadow.thisBind;

                    if (R != count - 1)
                        Shadow = Shadow.Next;
                }

                return returning;
            }
        }

        private void Funct_BuildMyTable()
        {
            myTable = new Table();
            myTable.Width = 900;
            myTable.HorizontalAlign = HorizontalAlign.Center;

            InsertHeader = new MakeTable(myTable, myName);
            myTable.Visible = false;
        }

        public FormSection ReturnFirstSection()
        {
            if (First == null)
            {
                return this;
            }
            else
            {
                return First;
            }
        }

        public FormSection ReturnNextSection()
        {
            if (Next == null)
            {
                return null;
            }

            return Next;
        }

        public bool IsNextSection()
        {
            return Next != null ? true : false;
        }

        public void ITEM_addNewSection(string SectionHeader, string SelectionGroup)
        {
            Next = new FormSection(SectionHeader, SelectionGroup, First);
        }

        public void ITEM_addNewSection(string SectionHeader, string SelectionGroup, int BindMe)
        {
            Next = new FormSection(SectionHeader, SelectionGroup, First);
        }

        public void AddNewItem(string Header)
        {
            if (FirstItem == null)
            {
                FirstItem = new FormNode(Header);
                CurrentItem = FirstItem;
            }
            else
            {
                CurrentItem.Next = new FormNode(Header);
                CurrentItem = CurrentItem.Next;
            }
        }

        //@@@@@@@@@@@@@@@@@@@@ INSERT FUNCTIONS @@@@@@@@@@@@@@@@@@@@@@@
        //====================================================================================================BEGIN OPTIONS

        public void Insert_SingleText(string Header, bool MultiLine)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), "", 650, MultiLine, false);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;


            NewTable.FinishTable(myTable);
        }

        public void Insert_TrainWreck()
        {
            string Header = "";

            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Hidden(NewTable.ReturnBodyCell());
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;

            NewTable.FinishTable(myTable);
        }

        public void Insert_SingleText(string Header, bool MultiLine, string GhostText, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), "", 650, MultiLine, GhostText, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;

            NewTable.FinishTable(myTable);
        }

        public void Insert_SingleText(string Header, bool MultiLine, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), "", 650, MultiLine, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;

            NewTable.FinishTable(myTable);
        }

        public void Insert_EmailText(string Header, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Email_Box(NewTable.ReturnBodyCell(), "", 650, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;

            NewTable.FinishTable(myTable);
        }

        public void Insert_EmailWithSetDomains(string Header, string[] Domains, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Email_DropDomain(NewTable.ReturnBodyCell(), Domains, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;

            NewTable.FinishTable(myTable);
        }

        public void Insert_BiText(string Header, string Label1, string Label2, bool MultiLine)
        {
            NewTable = new MakeTable(Header);
            int Arranger = 584 - ((Label1.Length + Label2.Length) * 5);

            AddNewItem(Header);
            CurrentItem.Children = 1;
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label1 + " ", (Arranger / 2), false, false);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            AddNewItem(Header);
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label2 + " ", (Arranger / 2), false, false);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;

            NewTable.FinishTable(myTable);
        }

        public void Insert_BiText(string Header, string Label1, string Label2, bool MultiLine, bool Box1Required, bool Box2Required)
        {
            NewTable = new MakeTable(Header);
            int Arranger = 580 - ((Label1.Length + Label2.Length) * 5);

            AddNewItem(Header);
            CurrentItem.Children = 1;
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label1 + " ", (Arranger / 2), false, Box1Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);

            AddNewItem(Header);
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label2 + " ", (Arranger / 2), false, Box2Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;

            NewTable.FinishTable(myTable);
        }

        public void Insert_TriText(string Header, string Label1, string Label2, string Label3, bool MultiLine)
        {
            NewTable = new MakeTable(Header);
            int Arranger = 555 - ((Label1.Length + Label2.Length + Label3.Length) * 5);

            AddNewItem(Header);
            CurrentItem.Children = 2;
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label1 + " ", (Arranger / 3), false, false);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);

            AddNewItem(Header);
            CurrentItem.Children = 1;
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label2 + " ", (Arranger / 3), false, false);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);

            AddNewItem(Header);
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label3 + " ", (Arranger / 3), false, false);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;

            NewTable.FinishTable(myTable);
        }

        public void Insert_TriText(string Header, string Label1, string Label2, string Label3, bool MultiLine, bool Box1Required, bool Box2Required, bool Box3Required)
        {
            NewTable = new MakeTable(Header);
            int Arranger = 555 - ((Label1.Length + Label2.Length + Label3.Length) * 5);

            AddNewItem(Header);
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label1 + " ", (Arranger / 3), false, Box1Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            CurrentItem.Children = 2;
            AddNewItem(Header);
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label2 + " ", (Arranger / 3), false, Box2Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            CurrentItem.Children = 1;
            AddNewItem(Header);
            CurrentItem.Add_Label_Box(NewTable.ReturnBodyCell(), " " + Label3 + " ", (Arranger / 3), false, Box3Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;

            NewTable.FinishTable(myTable);
        }

        public void Insert_TimeBox(string Header, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);

            CurrentItem.Add_TimeLabel_Box(NewTable.ReturnBodyCell(), " ( HH:MM ) ", Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            //CurrentItem.Children = 1;


            //Table subTable = new Table();
            //TableRow subRow = new TableRow();
            //TableCell subCellA = new TableCell();
            //TableCell subCellB = new TableCell();

            //subCellA.Width = 200;
            //subCellB.Width = 400;

            //subCellA.HorizontalAlign = HorizontalAlign.Center;

            //CurrentItem.Add_Label_Box(subCellA, " ( HH:MM ) ", 100, false, false);
            //CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            //AddNewItem(Header);
            //CurrentItem.Add_Radio_Box_Small(subCellB, new string[] { "AM", "PM" }, Required);
            //CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            //ClearNextItemOnSubmit = false;


            //subRow.Controls.Add(subCellA);
            //subRow.Controls.Add(subCellB);
            //subTable.Controls.Add(subRow);

            //NewTable.ReturnBodyCell().Controls.Add(subTable);

            NewTable.FinishTable(myTable);
        }

        public void Insert_CheckField(string Header, string[] Options, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Check_Box(NewTable.ReturnBodyCell(), Options, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_CheckVerticalField(string Header, string[] Options, bool Skinny, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_VertCheck_Box(NewTable.ReturnBodyCell(), Options, Required, !Skinny);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        /// <summary>
        /// This function will clear on submit by default, and is (bool) Required by default
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="Options"></param>
        public void Insert_CheckCriteriaField(string Header, string[] Options)
        {
            ClearNextItemOnSubmit = true;
            NewTable = new MakeTable(Header);
            AddNewItem(Header);
            CurrentItem.Add_Check_CriteriaBox(NewTable.ReturnBodyCell(), Options);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_Information(string Header, string Info, System.Drawing.Color R)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Information(NewTable.ReturnBodyCell(), Info, R);
            NewTable.FinishTable(myTable);
        }

        public void Insert_Information(string Header, string Info, System.Drawing.Color R, string ConfirmBoxText)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Information(NewTable.ReturnBodyCell(), Info, R, ConfirmBoxText);
            NewTable.FinishTable(myTable);
        }

        public void Insert_RadioField(string Header, string[] Options)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Radio_Box(NewTable.ReturnBodyCell(), Options, false);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_RadioVerticalField(string Header, string[] Options)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_VertRadio_Box(NewTable.ReturnBodyCell(), Options, false, true);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_RadioVerticalField(string Header, string[] Options, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_VertRadio_Box(NewTable.ReturnBodyCell(), Options, Required, true);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_RadioTextOnSelect(string Header, string[] Options, string TextOnSelectText, int TOSfromOptions, bool FullWidth, bool Required)
        {
            NewTable = new MakeTable(Header);

            if (!FullWidth) NewTable.TopAlign();

            AddNewItem(Header);
            CurrentItem.Add_VertRadio_TextonSelectBox(NewTable.ReturnBodyCell(), Options, TextOnSelectText, new int[] { TOSfromOptions }, Required, FullWidth);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_RadioTextOnSelect(string Header, string[] Options, string TextOnSelectText, int[] TOSfromOptions, bool FullWidth, bool Required)
        {
            NewTable = new MakeTable(Header);

            if (!FullWidth) NewTable.TopAlign();

            AddNewItem(Header);
            CurrentItem.Add_VertRadio_TextonSelectBox(NewTable.ReturnBodyCell(), Options, TextOnSelectText, TOSfromOptions, Required, FullWidth);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_DropWithTextOnSelect(string Header, string[] Options, string TextOnSelectText, int TOSfromOptions, bool FullWidth, bool Required)
        {
            NewTable = new MakeTable(Header);

            if (!FullWidth) NewTable.TopAlign();

            AddNewItem(Header);
            CurrentItem.Add_Dropbox_TextonSelectBox(NewTable.ReturnBodyCell(), Options, TextOnSelectText, TOSfromOptions, Required, FullWidth);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_RadioField(string Header, string[] Options, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Radio_Box(NewTable.ReturnBodyCell(), Options, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_DropList(string Header, string[] Options)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Dropbox(NewTable.ReturnBodyCell(), Options, false, false);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_DropList(string Header, string[] Options, bool AutoSelectFirstOption, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Dropbox(NewTable.ReturnBodyCell(), Options, AutoSelectFirstOption, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_DropList_WithDisabler(string Header, string[] Options, bool[] Disables, bool AutoSelectFirstOption, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Dropbox(NewTable.ReturnBodyCell(), Options, AutoSelectFirstOption, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            CurrentItem.MakeDropboxDisabler(Options, Disables, this);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_DropList_WithGhostBox(string Header, string[] Options, bool[] CreatesGhostBox, string GhostBoxLabel, string PassGhostTextIfAny, bool TrueForLargeBox_FalseForSmallBox, bool AutoSelectFirstOption, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Dropbox(NewTable.ReturnBodyCell(), Options, AutoSelectFirstOption, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            CurrentItem.MakeDropboxGhostbox(NewTable.ReturnBodyCell(), Options, CreatesGhostBox, GhostBoxLabel, TrueForLargeBox_FalseForSmallBox);
            CurrentItem.Ghost_PassGenericToBox(PassGhostTextIfAny);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_eCapsField()
        {
            string Header = "Employee Status:";

            NewTable = new MakeTable(Header);
            AddNewItem(Header);
            CurrentItem.Add_EcapsITDropbox(NewTable.ReturnBodyCell());
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_DropList_WithRequiredTextOnSelect(string Header, string[] SelectOptions, bool[] OptionMakesBoxRequired, string RequiredBoxLabel, string PassDefaultRequiredTextIfAny, bool TrueForLargeBox_FalseForSmallBox, bool AutoSelectFirstOption, bool RequiredTextNumericOnly, int RequiredTextMinLength, int RequiredTextMaxLength)
        {
            //NewTable = new MakeTable(Header);

            //AddNewItem(Header);
            //CurrentItem.Add_Dropbox(NewTable.ReturnBodyCell(), Options, AutoSelectFirstOption, true);
            //CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            //CurrentItem.MakeDropboxGhostbox(NewTable.ReturnBodyCell(), Options, CreatesGhostBox, GhostBoxLabel, TrueForLargeBox_FalseForSmallBox);
            //CurrentItem.Ghost_PassGenericToBox(PassGhostTextIfAny);
            //ClearNextItemOnSubmit = false;
            //NewTable.FinishTable(myTable);
        }

        /// <summary>
        /// Calling this function returns a FormNode which needs to be passed on to the section that will be disabled. Two-part process
        /// </summary>
        public FormNode Insert_DropList_WithSectionDisablers(string Header, string[] Options, bool AutoSelectFirstOption, bool Required, string[] DisablerTexts)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Dropbox(NewTable.ReturnBodyCell(), Options, AutoSelectFirstOption, Required);
            //CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            CurrentItem.MakeDropboxDisabler_WholeSection(DisablerTexts);
            //ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);

            return CurrentItem;
        }

        public void Insert_CascadeDDL(string Header, string[] RawStringValues, char Separator)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Cascade_DropDownList(NewTable.ReturnBodyCell(), RawStringValues, Separator);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_ImgDropList(string Header, HyperLink SupplyImage, string[] Options, bool UseRadios, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_Picture_DropDownList(NewTable.ReturnBodyCell(), SupplyImage, Options, UseRadios, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_DataSourceDDL(string Header, SqlDataSource DataSourceObject, string DS_DataTextField, string DS_DataValueField, bool Required)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_DataSource_DropDownList(NewTable.ReturnBodyCell(), DataSourceObject, DS_DataTextField, DS_DataValueField, Required);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_CascadeDataSourceDDL(string Header, SqlDataSource DataSourceObject, string DS_DataTextField, string DS_DataValueField, char Separator, bool ThisIsAlpha)
        {
            NewTable = new MakeTable(Header);

            AddNewItem(Header);
            CurrentItem.Add_DataSource_CascadeDropDownList(NewTable.ReturnBodyCell(), DataSourceObject, DS_DataTextField, DS_DataValueField, Separator, ThisIsAlpha);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_CascadeDeptID(string Header, string[] DEPT_NAMES, DEPT_OBJ[] DEPTMATRIX, bool ThisIsAlpha)
        {
            NewTable = new MakeTable(Header);
            AddNewItem(Header);
            CurrentItem.Add_DataSource_CascadeDropDownList(NewTable.ReturnBodyCell(), DEPT_NAMES, DEPTMATRIX, ThisIsAlpha);
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        //public void Insert_CascadeDeptDDL(string Header, , string[] DEPT_NAMES, DEPT_ORJ[] DEPTMATRIX, bool ThisIsAlpha)
        //{
        //    NewTable = new MakeTable(Header);

        //    AddNewItem(Header);
        //    CurrentItem.Add_DataSource_CascadeDropDownList(NewTable.ReturnBodyCell(), DEPT_NAMES, DEPTMATRIX, ThisIsAlpha);

        //    CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
        //    ClearNextItemOnSubmit = false;
        //    NewTable.FinishTable(myTable);
        //}

        public void Insert_DateDDL(string Header)
        {
            NewTable = new MakeTable(Header);

            string[] ThisYear = PumpYear(DateTime.Now.Year);
            string[] NextYear = (PumpYear(DateTime.Now.Year + 1));

            string[] Forcast = new string[ThisYear.Length + NextYear.Length];
            int Loc = 0;

            foreach (string R in ThisYear)
            { Forcast[Loc] = R; Loc++; }

            foreach (string R in NextYear)
            { Forcast[Loc] = R; Loc++; }

            AddNewItem(Header);
            CurrentItem.Add_Cascade_DropDownList(NewTable.ReturnBodyCell(), Forcast, '-');
            CurrentItem.ClearOnSubmit(ClearNextItemOnSubmit);
            ClearNextItemOnSubmit = false;
            NewTable.FinishTable(myTable);
        }

        public void Insert_Spacer()
        {
            MakeTable NewTables = new MakeTable("");
            Panel AddSpace = new Panel();
            AddSpace.Height = 25;
            AddSpace.Width = 500;
            NewTables.ReturnBodyCell().Controls.Add(AddSpace);
            NewTables.FinishTable(myTable);
        }

        //====================================================================================================END OPTIONS

        private string[] PumpYear(int InYear)
        {
            bool Leap = (InYear % 4 == 0) ? true : false;
            string[] BuildDates = Leap ? new string[366] : new string[365];
            int Loc = 0;

            foreach (string R in PumpDates("(" + InYear.ToString() + ") January-", 1, 31))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") February-", 2, (Leap ? 29 : 28)))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") March-", 3, 31))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") April-", 4, 30))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") May-", 5, 31))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") June-", 6, 30))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") July-", 7, 31))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") August-", 8, 31))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") September-", 9, 30))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") October-", 10, 31))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") November-", 11, 30))
            { BuildDates[Loc] = R; Loc++; }

            foreach (string R in PumpDates("(" + InYear.ToString() + ") December-", 12, 31))
            { BuildDates[Loc] = R; Loc++; }

            return BuildDates;
        }

        private string[] PumpDates(string Month, int MonthNum, int DayCount)
        {
            string DateYear = Month.Substring(1, 4);
            string[] ReturnThis = new string[DayCount];
            for (int X = 0; X < DayCount; X++)
            {
                ReturnThis[X] = Month + (X + 1).ToString(); // +" [" + MonthNum.ToString() + "-" + (X + 1).ToString() + "-" + DateYear + "]";
            }
            return ReturnThis;
        }

        /// <summary>
        /// Builds a cascading dropdownlist that is required and cannot be left blank. The raw values will be parsed with the separator into two segments. The first segment will be placed in the first DDL and will dictate what is shown in the second DDL.
        /// </summary>
        /// <param name="SupplyTable"></param>
        /// <param name="Header"></param>
        /// <param name="RawStringValues"></param>
        /// <param name="Separator"></param>


        //@@@@@@@@@@@@@@@@@@@@ RETURN FUNCTIONS @@@@@@@@@@@@@@@@@@@@@@@

        private Label ReturnLabel_Strong(string SupplyText)
        {
            Label InsertText = new Label();
            InsertText.Text = SupplyText;
            InsertText.Font.Bold = true;
            return InsertText;
        }

        private Label ReturnLabel(string SupplyText)
        {
            Label InsertText = new Label();
            InsertText.Text = SupplyText;
            return InsertText;
        }

        private TextBox ReturnBox(string SupplyText)
        {
            TextBox InsertThis = new TextBox();
            InsertThis.Width = 150;
            return InsertThis;
        }

        public void YieldToAnotherSection(FormNode Master)
        {
            Master.PassDisablingNode(this);
        }

        public void DisableThis(bool YesNo)
        {
            this.myTable.Visible = !YesNo;
        }

        public void DisableAllOthers(FormNode ExceptMe, bool YesNo)
        {
            FormNode Transverse = FirstItem;
            if (Transverse != ExceptMe)
            {
                Transverse.AllEnabled(!YesNo);
            }

            while (Transverse.Next != null)
            {
                Transverse = Transverse.Next;
                if (Transverse != ExceptMe)
                {
                    Transverse.AllEnabled(!YesNo);
                }
            }
        }

        public string RecallFieldDataAsHTML()
        {
            FormNode Transverse = FirstItem;
            string BuildHTML = "";
            string SectionName = "";
            string RetData = "";

            while (Transverse != null)
            {
                SectionName = Transverse.RetrieveHeader().Replace(":", "");

                if (Transverse.Children > 0)
                {
                    //Insert the section name
                    BuildHTML += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + SectionName + ":: <br />";

                    //write all of the Children from X to 0
                    for (int X = Transverse.Children; X >= 0; X--)
                    {
                        RetData = Transverse.RetrieveData(true);
                        BuildHTML += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;*&nbsp;&nbsp;&nbsp;" + RetData.Replace("<br />", "").Replace(">>", "") + "<br />";
                        Transverse = Transverse.Next;
                    }

                    BuildHTML += "<br />";
                }
                else
                {
                    RetData = Transverse.RetrieveData(true);
                    BuildHTML += (RetData == "") ? "" : "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + SectionName + ":: " + RetData.Replace("<br />", "").Replace(">>", "") + "<br />";
                    Transverse = Transverse.Next;
                }
            }

            return BuildHTML;
        }

        public string RecallFieldDataAsRTF()
        {
            FormNode Transverse = FirstItem;
            string BuildHTML = "";
            string SectionName = "";

            int DataItems = 0;
            string RetData = "";

            while (Transverse != null)
            {
                SectionName = Transverse.RetrieveHeader().Replace(":", "");

                if (Transverse.Children > 0)
                {
                    //Adding this code to eliminate parents with no children (DECLARE)
                    bool pActive = true;

                    //write all of the Children from X to 0
                    for (int X = Transverse.Children; X >= 0; X--)
                    {
                        RetData = Transverse.RetrieveData(false);
                        if (RetData.Replace(" ", "") != "")
                        {
                            //Adding this code to eliminate parents with no children (CHECK and ACTIVATE)
                            if (pActive)
                            {
                                //Insert the section name
                                BuildHTML += "\r\n\t" + SectionName + ": \r\n";
                                pActive = false;
                            }
                            DataItems += 1;
                            BuildHTML += "\t* " + RetData.Replace("<br />", "").Replace(">>", "") + "\r\n";
                        }

                        Transverse = Transverse.Next;
                    }

                    BuildHTML += "\r\n";
                }
                else
                {
                    RetData = Transverse.RetrieveData(false).Trim();
                    DataItems += (RetData == "") ? 0 : 1;
                    BuildHTML += (RetData == "") ? "" : "\t" + SectionName + ": " + RetData + "\r\n";
                    Transverse = Transverse.Next;
                }
            }

            return (DataItems > 0) ? BuildHTML : "";
        }

        public string RecallFirstFieldDataAsRaw()
        {
            FormNode Transverse = FirstItem;
            return Transverse.RetrieveData(false);
        }

        public void FormWasSubmitted()
        {
            FormNode Transverse = FirstItem;
            Transverse.CheckAndClear_FormWasSubmitted();
            while (Transverse.Next != null)
            {
                Transverse = Transverse.Next;
                Transverse.CheckAndClear_FormWasSubmitted();
            }
        }
    }


    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ FORM NODE CLASS @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
    // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv


    public class FormNode
    {
        public FormNode Next;
        FormSection myParent;
        FormSection mySubmissive;

        //This array of Linked Objects forms the data for the Dept IDs shared among the program
        private DEPT_OBJ[] DEPTOBJMATRIX;
        private string[] DEPT_SHORTNAMES;

        System.Drawing.Color COLOR_CLEAR = System.Drawing.Color.Transparent;
        System.Drawing.Color COLOR_HILIGHT = System.Drawing.Color.Pink;

        Label IsRequired; bool BoolRequired = false;
        private string StoredHeader;

        int Type; //Node should be of only one type

        public int Children = 0;

        public bool BlankOnSubmit = false;

        private string DROPS_REENFORCE = "";

        //Type 1 consists of these
        Label Text;
        TextBox Box;

        //Type 2 consists of these
        RadioButtonList Rads;

        //Type 3 consists of these
        CheckBoxList Checks;

        //Type 4 consists of these
        DropDownList Drops;

        //Type 5 consists of these
        //Calendar Cal;
        //TextBox DateBox;

        //Type 6 consists of these
        DropDownList CascShort;
        DropDownList CascFull;
        string[] CascALL;
        char CascSeparator;

        //Type 7 / 8 consists of these
        SqlDataSource DB;

        //Type 9 (becomes 2 or 4) consists of these
        HyperLink ImageLink;

        public void AllEnabled(bool YesNo)
        {
            if (Box != null) { Box.Enabled = YesNo; }
            if (Rads != null) { Rads.Enabled = YesNo; }
            if (Checks != null) { Checks.Enabled = YesNo; }
            if (Drops != null) { Drops.Enabled = YesNo; }
            if (CascShort != null) { CascShort.Enabled = YesNo; }
            if (CascFull != null) { CascFull.Enabled = YesNo; }

            ClearErrors();
        }

        public void AddGhost(bool YesNo)
        {
            Text.Visible = YesNo;
            Box.Visible = YesNo;
        }

        public void CheckAndClear_FormWasSubmitted()
        {
            if (BlankOnSubmit)
            {
                if (Box != null) { Box.Text = ""; }
                if (Drops != null) { Drops.ClearSelection(); Drops.Items[0].Selected = true; };
                if (CascFull != null) { CascFull.ClearSelection(); CascFull.Items[0].Selected = true; }

                if (Rads != null)
                {
                    for (int R = 0; R < Rads.Items.Count; R++)
                    {
                        Rads.Items[R].Selected = false;
                    }
                }
                if (Checks != null)
                {
                    for (int R = 0; R < Checks.Items.Count; R++)
                    {
                        Checks.Items[R].Selected = false;
                    }
                }
            }
        }

        public FormNode(string Header)
        {
            StoredHeader = Header.Contains("#H#") ? Header.Substring(0, Header.IndexOf("#H#")) : Header;
        }

        public string RetrieveHeader()
        {
            return StoredHeader;
        }

        public void StoreHeader(string Header)
        {
            StoredHeader = Header.Contains("#H#") ? Header.Substring(0, Header.IndexOf("#H#")) : Header;
        }

        private void ClearErrors(int SpecialCase)
        {
            switch (SpecialCase)
            {
                case 20:
                    Drops.BackColor = COLOR_CLEAR;
                    Box.BackColor = COLOR_CLEAR;
                    break;

                default:
                    break;
            }

        }

        private void ClearErrors()
        {
            if (BoolRequired)
            {
                IsRequired.Visible = false;

                switch (Type)
                {
                    case 1:
                        Box.BackColor = COLOR_CLEAR;
                        break;
                    case 2:
                        Rads.BackColor = COLOR_CLEAR;
                        break;
                    case 3:
                        Checks.BackColor = COLOR_CLEAR;
                        break;
                    case 4:
                        Drops.BackColor = COLOR_CLEAR;
                        break;
                    case 5:
                        //return DateBox.Text;
                        break;
                    case 6:
                        CascFull.BackColor = COLOR_CLEAR;
                        break;
                    case 7:
                        Drops.BackColor = COLOR_CLEAR;
                        break;
                    case 11:
                        Box.BackColor = COLOR_CLEAR;
                        break;

                    default:
                        break;
                }
            }
        }

        public void isVisible(bool TrueFalse)
        {
            Box.Visible = TrueFalse;
        }

        public void CalCommand(object sender, EventArgs e)
        {
            //DateBox.Text = Cal.SelectedDate.ToShortDateString();
        }

        public void ThrowParent(FormSection Parent)
        {
            myParent = Parent;
        }

        public void ClearOnSubmit(bool YesNo)
        {
            BlankOnSubmit = YesNo;
        }

        public void Add_Hidden(TableCell SupplyCell)
        {
            Type = 1;

            Text = new Label();
            Box = new TextBox();

            Text.Text = "";
            isVisible(false);

            SupplyCell.Controls.Add(Text);
            SupplyCell.Controls.Add(Box);

            MakeRequired(SupplyCell);
        }

        public void Add_Label_Box(TableCell SupplyCell, string LabelText, int BoxWidth, bool BoxMultiLine, bool Required)
        {
            Type = 1;

            Text = new Label();
            Box = new TextBox();

            Text.Text = LabelText;
            Box.Width = BoxWidth;

            if (BoxMultiLine)
            {
                Box.TextMode = TextBoxMode.MultiLine;
                Box.Height = 75;
            }

            SupplyCell.Controls.Add(Text);
            SupplyCell.Controls.Add(Box);

            if (Required) { MakeRequired(SupplyCell); }

            if (Children > 0)
            {
                Label Spacer = new Label();
                Spacer.Text = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                SupplyCell.Controls.Add(Spacer);
            }
        }

        public void Add_Label_Box(TableCell SupplyCell, string LabelText, int BoxWidth, bool BoxMultiLine, string Ghost, bool Required)
        {
            Type = 1;

            Text = new Label();
            Box = new TextBox();

            Text.Text = LabelText;
            Box.Width = BoxWidth;

            if (BoxMultiLine)
            {
                Box.TextMode = TextBoxMode.MultiLine;
                Box.Height = 75;
            }


            Box.Attributes.Add("placeholder", Ghost);
            Box.Attributes["placeholder"] = Ghost;

            SupplyCell.Controls.Add(Text);
            SupplyCell.Controls.Add(Box);

            if (Required) { MakeRequired(SupplyCell); }

            if (Children > 0)
            {
                Label Spacer = new Label();
                Spacer.Text = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                SupplyCell.Controls.Add(Spacer);
            }
        }

        public void Add_TimeLabel_Box(TableCell SupplyCell, string LabelText, bool Required)
        {
            Type = 15;

            Table subTable = new Table();
            TableRow subRow = new TableRow();
            TableCell subCellA = new TableCell();
            TableCell subCellB = new TableCell();

            subCellA.Width = 200;
            subCellB.Width = 400;

            subCellA.HorizontalAlign = HorizontalAlign.Center;

            Text = new Label();
            Box = new TextBox();

            Text.Text = LabelText;
            Box.Width = 100;
            Rads = new RadioButtonList();

            Rads.Items.Add("AM");
            Rads.Items.Add("PM");
            Rads.RepeatColumns = 3;


            subCellA.Controls.Add(Text);
            subCellA.Controls.Add(Box);

            subCellB.Controls.Add(Rads);

            //SupplyCell.Controls.Add(Text);
            //SupplyCell.Controls.Add(Box);

            //SupplyCell.Controls.Add(Rads);
            subRow.Controls.Add(subCellA);
            subRow.Controls.Add(subCellB);
            subTable.Controls.Add(subRow);

            SupplyCell.Controls.Add(subTable);
            SupplyCell.VerticalAlign = VerticalAlign.Middle;
            SupplyCell.HorizontalAlign = HorizontalAlign.Center;



            if (Required)
            {
                MakeRequired(SupplyCell);
                IsRequired.Text = "Please enter a time.";
            }
        }

        public void Add_Email_DropDomain(TableCell SupplyCell, string[] Domains, bool Required)
        {
            Type = 12;

            Box = new TextBox();
            Drops = new DropDownList();
            Drops.Height = new System.Web.UI.WebControls.Unit((21));

            Box.Width = 150;
            Drops.Width = 200;


            foreach (string R in Domains)
            {
                string RR = R.ToLower();

                if (RR.Substring(0, 1) != "@")
                {
                    if (!RR.Contains("@"))
                    {
                        RR = "@" + R;
                    }
                    else
                    {
                        RR = R.Substring(R.IndexOf("@"));
                    }
                }

                Drops.Items.Add(RR);
            }

            SupplyCell.Controls.Add(Box);
            SupplyCell.Controls.Add(Drops);

            if (Required)
            {
                MakeRequired(SupplyCell);
            }

        }

        public void Add_Radio_Box_Small(TableCell SupplyCell, string[] Options, bool Required)
        {
            Type = 2;

            Rads = new RadioButtonList();

            foreach (string R in Options)
            {
                Rads.Items.Add(R);
            }

            Rads.RepeatColumns = 3;
            SupplyCell.Controls.Add(Rads);
            SupplyCell.VerticalAlign = VerticalAlign.Middle;
            SupplyCell.HorizontalAlign = HorizontalAlign.Center;

            if (Required)
            {
                MakeRequired(SupplyCell);
                IsRequired.Text = "Please select an option.";
            }
        }

        public void Add_Radio_Box(TableCell SupplyCell, string[] Options, bool Required)
        {
            Type = 2;

            Rads = new RadioButtonList();

            foreach (string R in Options)
            {
                Rads.Items.Add(R);
            }

            Rads.RepeatColumns = 3;
            Rads.Width = 650;
            Rads.BackColor = System.Drawing.Color.Beige;
            Rads.BorderColor = System.Drawing.Color.WhiteSmoke;
            Rads.BorderWidth = 1;
            Rads.BorderStyle = BorderStyle.Solid;
            SupplyCell.Controls.Add(Rads);
            SupplyCell.Height = new System.Web.UI.WebControls.Unit(((Options.Length / 3) + 1) * 20);
            SupplyCell.VerticalAlign = VerticalAlign.Middle;
            SupplyCell.HorizontalAlign = HorizontalAlign.Center;

            if (Required)
            {
                MakeRequired(SupplyCell);
                IsRequired.Text = "Please select an option.";
            }
        }

        public void Add_VertRadio_Box(TableCell SupplyCell, string[] Options, bool Required, bool FullWidth)
        {
            Type = 2;

            Rads = new RadioButtonList();

            foreach (string R in Options)
            {
                Rads.Items.Add(R);
            }

            Rads.RepeatColumns = 1;
            Rads.Width = FullWidth ? 650 : 325;
            Rads.BackColor = System.Drawing.Color.Beige;
            Rads.BorderColor = System.Drawing.Color.WhiteSmoke;
            Rads.BorderWidth = 1;
            Rads.BorderStyle = BorderStyle.Solid;


            if (FullWidth)
            {
                Label Spacer = new Label();
                Spacer.Text = "&nbsp;";
                SupplyCell.Controls.Add(Spacer);
                SupplyCell.Controls.Add(Rads);
                Spacer = new Label();
                Spacer.Text = "&nbsp;";
                SupplyCell.Controls.Add(Spacer);
            }
            else
            {
                SupplyCell.Controls.Add(Rads);
                SupplyCell.Height = new System.Web.UI.WebControls.Unit(250);
                SupplyCell.VerticalAlign = VerticalAlign.Middle;
                SupplyCell.HorizontalAlign = HorizontalAlign.Center;
            }

            if (Required)
            {
                MakeRequired(SupplyCell);
                IsRequired.Text = "Please select an option.";
            }
        }

        public void Add_VertRadio_TextonSelectBox(TableCell SupplyCell, string[] Options, string ToSLabel, int[] ToSOption, bool Required, bool FullWidth)
        {
            Type = 17;

            Rads = new RadioButtonList();
            Box = new TextBox();
            Text = new Label();

            foreach (string R in Options)
            {
                Rads.Items.Add(R);
            }

            Text.Text = ToSLabel;

            foreach (int X in ToSOption)
            {
                Rads.Items[X].Value = "TOS" + X.ToString();
            }

            Text.Visible = false;
            Text.Height = new System.Web.UI.WebControls.Unit(40);
            Box.Visible = false;
            Rads.RepeatColumns = 1;
            Rads.Width = FullWidth ? 650 : 325;
            Box.Width = FullWidth ? 600 : 250;
            Rads.BackColor = System.Drawing.Color.Beige;
            Rads.BorderColor = System.Drawing.Color.WhiteSmoke;
            Rads.BorderWidth = 1;
            Rads.BorderStyle = BorderStyle.Solid;

            Rads.SelectedIndexChanged += new System.EventHandler(this.TextOnSelectCommand);
            Rads.AutoPostBack = true;

            if (FullWidth)
            {
                Label Spacer = new Label();
                Spacer.Text = "&nbsp;";
                SupplyCell.Controls.Add(Spacer);
                SupplyCell.Controls.Add(Rads);
            }
            else
            {
                SupplyCell.Controls.Add(Rads);
                SupplyCell.Width = new System.Web.UI.WebControls.Unit(250);
                SupplyCell.VerticalAlign = VerticalAlign.Middle;
                SupplyCell.HorizontalAlign = HorizontalAlign.Center;
            }

            Label tSpacer = new Label();
            tSpacer.Text = "&nbsp;";
            SupplyCell.Controls.Add(tSpacer);
            SupplyCell.Controls.Add(Text);
            SupplyCell.Controls.Add(Box);

            MakeRequired(SupplyCell);
            IsRequired.Text = " Please select an option or enter required text. ";
            BoolRequired = Required;

        }

        public void TextOnSelectCommand(object sender, EventArgs e)
        {
            Text.Visible = (Rads.SelectedValue.ToString().Substring(0, 3) == "TOS");
            Box.Visible = (Rads.SelectedValue.ToString().Substring(0, 3) == "TOS");
        }

        public void Add_Dropbox_TextonSelectBox(TableCell SupplyCell, string[] Options, string ToSLabel, int ToSOption, bool Required, bool FullWidth)
        {
            //Type = 17;

            //Drops = new DropDownList();
            //Drops.Height = new System.Web.UI.WebControls.Unit((21));


            //Box = new TextBox();
            //Text = new Label();

            //foreach (string R in Options)
            //{
            //    Rads.Items.Add(R);
            //}

            //Text.Text = ToSLabel;
            //Rads.Items[ToSOption].Value = "TOS";
            //Text.Visible = false;
            //Text.Height = new System.Web.UI.WebControls.Unit(40);
            //Box.Visible = false;
            //Rads.RepeatColumns = 1;
            //Rads.Width = FullWidth ? 650 : 325;
            //Rads.BackColor = System.Drawing.Color.Beige;
            //Rads.BorderColor = System.Drawing.Color.WhiteSmoke;
            //Rads.BorderWidth = 1;
            //Rads.BorderStyle = BorderStyle.Solid;

            //Rads.SelectedIndexChanged += new System.EventHandler(this.TextOnSelectCommand);
            //Rads.AutoPostBack = true;

            //if (FullWidth)
            //{
            //    Label Spacer = new Label();
            //    Spacer.Text = "&nbsp;";
            //    SupplyCell.Controls.Add(Spacer);
            //    SupplyCell.Controls.Add(Rads);
            //}
            //else
            //{
            //    SupplyCell.Controls.Add(Rads);
            //    SupplyCell.Width = new System.Web.UI.WebControls.Unit(250);
            //    SupplyCell.VerticalAlign = VerticalAlign.Middle;
            //    SupplyCell.HorizontalAlign = HorizontalAlign.Center;
            //}

            //Label tSpacer = new Label();
            //tSpacer.Text = "&nbsp;";
            //SupplyCell.Controls.Add(tSpacer);
            //SupplyCell.Controls.Add(Text);
            //SupplyCell.Controls.Add(Box);

            //MakeRequired(SupplyCell);
            //IsRequired.Text = " Please select an option or enter required text. ";
            //BoolRequired = Required;

        }

        public void Add_VertCheck_Box(TableCell SupplyCell, string[] Options, bool Required, bool FullWidth)
        {
            Type = 3;

            Checks = new CheckBoxList();

            foreach (string R in Options)
            {
                Checks.Items.Add(R);
            }

            Checks.RepeatColumns = 1;
            Checks.Width = FullWidth ? 650 : 325;
            Checks.BackColor = System.Drawing.Color.Beige;
            Checks.BorderColor = System.Drawing.Color.WhiteSmoke;
            Checks.BorderWidth = 1;
            Checks.BorderStyle = BorderStyle.Solid;


            if (FullWidth)
            {
                Label Spacer = new Label();
                Spacer.Font.Size = 8;
                Spacer.Text = "&nbsp;";
                SupplyCell.Controls.Add(Spacer);
                SupplyCell.Controls.Add(Checks);
                Spacer = new Label();
                Spacer.Font.Size = 8;
                Spacer.Text = "&nbsp;";
                SupplyCell.Controls.Add(Spacer);
            }

            if (!FullWidth)
            {
                SupplyCell.Controls.Add(Checks);
                SupplyCell.Height = new System.Web.UI.WebControls.Unit(250);
                SupplyCell.VerticalAlign = VerticalAlign.Middle;
                SupplyCell.HorizontalAlign = HorizontalAlign.Center;
            }

            if (Required)
            {
                MakeRequired(SupplyCell);
                IsRequired.Text = "Please select at least one option.";
            }
        }

        public void Add_Check_CriteriaBox(TableCell SupplyCell, string[] Options)
        {
            Type = 14;

            Checks = new CheckBoxList();

            foreach (string R in Options)
            {
                Checks.Items.Add(R);
            }

            Checks.RepeatColumns = 1;
            Checks.Width = 650;
            Checks.BackColor = System.Drawing.Color.Beige;
            Checks.BorderColor = System.Drawing.Color.WhiteSmoke;
            Checks.BorderWidth = 1;
            Checks.BorderStyle = BorderStyle.Solid;

            Label Spacer = new Label();
            Spacer.Font.Size = 4;
            Spacer.Text = "&nbsp;";
            SupplyCell.Controls.Add(Spacer);
            SupplyCell.Controls.Add(Checks);
            Spacer = new Label();
            Spacer.Font.Size = 4;
            Spacer.Text = "&nbsp;";
            SupplyCell.Controls.Add(Spacer);

            MakeRequired(SupplyCell);
            IsRequired.Text = "Please check all criteria.";
        }

        public void Add_Check_Box(TableCell SupplyCell, string[] Options, bool Required)
        {
            Type = 3;

            Checks = new CheckBoxList();

            foreach (string R in Options)
            {
                Checks.Items.Add(R);
            }

            Checks.RepeatColumns = 3;
            Checks.Width = 630;
            Checks.BackColor = System.Drawing.Color.Beige;
            Checks.BorderColor = System.Drawing.Color.WhiteSmoke;
            Checks.BorderWidth = 1;
            Checks.BorderStyle = BorderStyle.Solid;
            SupplyCell.Controls.Add(Checks);
            SupplyCell.Height = new System.Web.UI.WebControls.Unit(((Options.Length / 3) + 1) * 30);
            SupplyCell.VerticalAlign = VerticalAlign.Middle;
            SupplyCell.HorizontalAlign = HorizontalAlign.Center;

            if (Required) { MakeRequired(SupplyCell); IsRequired.Text = "Please check one or more options."; }

        }

        public void Add_Dropbox(TableCell SupplyCell, string[] Options, bool AutoSelFirstOption, bool Required)
        {
            Type = 4;

            Drops = new DropDownList();

            if (!AutoSelFirstOption)
            {
                Drops.Items.Add("");
            }

            foreach (string R in Options)
            {
                Drops.Items.Add(R);
            }

            //if (AutoSelFirstOption)
            //{
            //    Drops.Items[0].Selected = true;
            //    Drops.SelectedIndex = 0;
            //    Drops.SelectedValue = Drops.Items[0].Value;
            //}

            Drops.Width = 650;
            SupplyCell.Controls.Add(Drops);

            if (Required && !AutoSelFirstOption)
            {
                MakeRequired(SupplyCell);
            }
        }

        public void MakeDropboxDisabler(string[] ItemText, bool[] Disables, FormSection Parent)
        {
            myParent = Parent;
            bool Seal = false;

            for (int Z = 0; Z < ItemText.Length; Z++)
            {
                try
                {
                    if (Disables[Z])
                    {
                        for (int R = 0; R < Drops.Items.Count; R++)
                        {
                            if (Drops.Items[R].Text == ItemText[Z])
                            {
                                //dont remove adding the R, values cant be the same as other values or weird things happen.
                                Drops.Items[R].Value = "disable" + R.ToString();
                                Seal = true;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    break;
                }
            }

            if (Seal)
            {
                Drops.SelectedIndexChanged += new System.EventHandler(this.DropsDisableOthersCommand);
                Drops.AutoPostBack = true;
            }
        }

        public void MakeDropboxGhostbox(TableCell SupplyCell, string[] ItemText, bool[] CreatesBox, string GhostBoxLabel, bool LargeBox)
        {
            bool Seal = false;

            for (int Z = 0; Z < ItemText.Length; Z++)
            {
                try
                {
                    if (CreatesBox[Z])
                    {
                        for (int R = 0; R < Drops.Items.Count; R++)
                        {
                            if (Drops.Items[R].Text == ItemText[Z])
                            {
                                //dont remove adding the R, values cant be the same as other values or weird things happen.
                                Drops.Items[R].Value = "createghost" + R.ToString();
                                Seal = true;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    break;
                }
            }

            if (Seal)
            {
                Drops.SelectedIndexChanged += new System.EventHandler(this.DropsCreateGhostCommand);

                Type = 19;

                Text = new Label();
                Box = new TextBox();

                Text.Visible = false;
                Box.Visible = false;

                if (LargeBox)
                {
                    Box.BackColor = System.Drawing.Color.AliceBlue;
                    Text.Font.Bold = true;
                    Box.TextMode = TextBoxMode.MultiLine;
                    Box.Height = 250;
                    Box.Width = 420;
                }

                Text.Text = GhostBoxLabel;

                Label tSpacer = new Label();
                tSpacer.Text = "&nbsp;<br>";
                SupplyCell.Controls.Add(tSpacer);
                SupplyCell.Controls.Add(Text);
                tSpacer = new Label();
                tSpacer.Text = "&nbsp;<br>";
                SupplyCell.Controls.Add(tSpacer);

                SupplyCell.Controls.Add(Box);

                Drops.AutoPostBack = true;

                MakeRequired(SupplyCell);
                IsRequired.Text = "<br>** Please select an option or enter required text. ";
                BoolRequired = true;
            }
        }

        public void Ghost_PassGenericToBox(string PassText)
        {
            Box.Text = PassText;
        }

        public void Add_EcapsITDropbox(TableCell SupplyCell)
        {
            Type = 20;

            Drops = new DropDownList();
            Text = new Label();
            Box = new TextBox();

            Text.Text = " eCAPs ID:";

            Drops.Items.Add("");
            Drops.Items[0].Value = "empty";
            Drops.Items.Add("Person is City employee");
            Drops.Items[1].Value = "required";
            Drops.Items.Add("Person is NOT a City employee");
            Drops.Items[2].Value = "okay";

            Drops.SelectedIndexChanged += new System.EventHandler(this.EcapsMakeRequiredCommand);
            Drops.AutoPostBack = true;


            Drops.Width = 250;
            Box.Width = 100;
            SupplyCell.Controls.Add(Drops);
            SupplyCell.Controls.Add(Text);
            SupplyCell.Controls.Add(Box);
            Label xSpacer = new Label();
            xSpacer.Text = "&nbsp;<br>";
            SupplyCell.Controls.Add(xSpacer);


            MakeRequired(SupplyCell);

            IsRequired.Text = "<br>** Please make a selection - eCaps ID required for City employees.";
        }

        public void EcapsMakeRequiredCommand(object sender, EventArgs e)
        {
            try
            {
                BoolRequired = (Drops.SelectedValue == "required" || Drops.SelectedValue == "empty");

                if (BoolRequired)
                {
                    Text.Text = " eCAPs ID:";
                }
                else
                {
                    Text.Text = " eCAPs ID: (Not Required) ";
                    ClearErrors();
                }
            }
            catch
            {
                BoolRequired = false;
            }
        }

        public void DropsCreateGhostCommand(object sender, EventArgs e)
        {
            try
            {
                if (Drops.SelectedValue.Length > 4 && Drops.SelectedValue.Substring(0, 11) == "createghost")
                {
                    Text.Visible = true;
                    Box.Visible = true;
                }
                else
                {
                    Text.Visible = false;
                    Box.Visible = false;
                    //tell our myParent to enable the rest
                }
            }

            catch
            {
                Text.Visible = false;
                Box.Visible = false;
                //tell our myParent to enable the rest
            }
        }


        public void DropsDisableOthersCommand(object sender, EventArgs e)
        {
            if (Drops.SelectedValue.Length > 6 && Drops.SelectedValue.Substring(0, 7) == "disable")
            {
                myParent.DisableAllOthers(this, true);
                //tell our parent to disable the rest
            }
            else
            {
                myParent.DisableAllOthers(this, false);
                //tell our myParent to enable the rest
            }
        }

        public void PassDisablingNode(FormSection Submissive)
        {
            mySubmissive = Submissive;
        }

        public void MakeDropboxDisabler_WholeSection(string[] ItemText)
        {
            bool Found = false;

            for (int R = 0; R < Drops.Items.Count; R++)
            {
                for (int Y = 0; Y < ItemText.Length; Y++)
                {
                    if (Drops.Items[R].Text == ItemText[Y])
                    {
                        Drops.Items[R].Value = "disablesect";
                        Found = true;
                    }
                }
            }

            if (Found)
            {
                Drops.SelectedIndexChanged += new System.EventHandler(this.DropsDisablesOtherSections_Command);
                Drops.AutoPostBack = true;
            }

        }

        public void DropsDisablesOtherSections_Command(object sender, EventArgs e)
        {
            if (mySubmissive != null)
            {
                if (Drops.SelectedValue == "disablesect")
                {
                    mySubmissive.DisableThis(true);
                    //tell our submissive to disable
                }
                else
                {
                    mySubmissive.DisableThis(false);
                }
            }
        }

        public void Add_NarrowDropbox(TableCell SupplyCell, string[] Options, bool Required)
        {
            Type = 4;

            Drops = new DropDownList();
            foreach (string R in Options)
            {
                Drops.Items.Add(R);
            }
            Drops.Width = 300;
            SupplyCell.Controls.Add(Drops);

            if (Required) { MakeRequired(SupplyCell); }
        }

        public void Add_Information(TableCell SupplyCell, string Info, System.Drawing.Color R)
        {
            Type = -1;

            Table myTable = new Table();
            TableRow myRow = new TableRow();
            TableCell myCellSpL = new TableCell();
            TableCell myCellA = new TableCell();
            TableCell myCellSpR = new TableCell();

            myTable.Width = 650;
            myCellA.Width = 550;
            myCellSpL.Width = 50;
            myCellSpR.Width = 50;
            myCellSpL.Text = "&nbsp;";
            myCellSpR.Text = "&nbsp;";

            Text = new Label();
            Text.Text = Info;

            myCellSpL.BackColor = R;
            myCellA.BackColor = R;
            myCellSpR.BackColor = R;
            Text.BackColor = R;
            Text.ForeColor = System.Drawing.Color.White;
            myCellA.Controls.Add(Text);
            myRow.Cells.Add(myCellSpL);
            myRow.Cells.Add(myCellA);
            myRow.Cells.Add(myCellSpR);
            myTable.Rows.Add(myRow);
            SupplyCell.Controls.Add(myTable);
        }

        public void Add_Information(TableCell SupplyCell, string Info, System.Drawing.Color R, string ConfirmText)
        {
            Type = 16;

            Table myTable = new Table();
            TableRow myRow = new TableRow();
            TableRow myConfirm = new TableRow();
            TableCell myCellSpL = new TableCell();
            TableCell myCellA = new TableCell();
            TableCell myCellSpR = new TableCell();
            TableCell myConfirmBox = new TableCell();

            myTable.Width = 650;
            myCellA.Width = 600;
            myCellSpL.Width = 25;
            myCellSpR.Width = 25;
            myCellSpL.Text = "&nbsp;";
            myCellSpR.Text = "&nbsp;";
            myConfirmBox.ColumnSpan = 3;
            myConfirmBox.HorizontalAlign = HorizontalAlign.Center;

            Text = new Label();
            Text.Text = Info;

            myCellSpL.BackColor = R;
            myCellA.BackColor = R;
            myCellSpR.BackColor = R;
            Text.BackColor = R;
            Text.ForeColor = System.Drawing.Color.White;
            myCellA.Controls.Add(Text);

            Checks = new CheckBoxList();
            Checks.Items.Add(" " + ConfirmText);
            Checks.RepeatColumns = 1;
            Checks.Width = 650;
            Checks.BackColor = System.Drawing.Color.Beige;
            Checks.BorderColor = System.Drawing.Color.WhiteSmoke;
            Checks.BorderWidth = 1;
            Checks.BorderStyle = BorderStyle.Solid;

            //Label Spacer = new Label();
            //Spacer.Font.Size = 4;
            //Spacer.Text = "&nbsp;";

            myConfirmBox.Controls.Add(Checks);
            //myConfirmBox.Controls.Add(Spacer);

            myRow.Cells.Add(myCellSpL);
            myRow.Cells.Add(myCellA);
            myRow.Cells.Add(myCellSpR);
            myConfirm.Cells.Add(myConfirmBox);
            myTable.Rows.Add(myRow);
            myTable.Rows.Add(myConfirm);
            SupplyCell.Controls.Add(myTable);

            MakeRequired(SupplyCell);
            IsRequired.Text = "Please confirm.";
        }

        public void Add_Picture_DropDownList(TableCell SupplyCell, HyperLink SupplyImageLink, string[] Options, bool UseRadiosInstead, bool Required)
        {
            ImageLink = SupplyImageLink;

            Table myTable = new Table();
            TableRow myRow = new TableRow();
            TableCell myCellA = new TableCell();
            TableCell myCellB = new TableCell();

            myCellA.Width = 350;
            myCellB.Width = 350;

            myCellA.HorizontalAlign = HorizontalAlign.Center;

            Label Test = new Label();
            Test.Text = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            myCellA.Controls.Add(Test);
            myCellA.Controls.Add(ImageLink);


            //primarily used for selecting the users Region from a drop down or radio button list
            Type = (UseRadiosInstead ? 2 : 4);  // "9"


            if (!UseRadiosInstead)
            {
                //create drop down list of type 4
                Add_NarrowDropbox(myCellB, Options, Required);
            }
            else
            {
                //Create Radio box of type 2
                Add_VertRadio_Box(myCellB, Options, Required, false);
            }

            myRow.Cells.Add(myCellA);
            myRow.Cells.Add(myCellB);
            myTable.Rows.Add(myRow);
            SupplyCell.Controls.Add(myTable);

        }

        public void Add_Cascade_DropDownList(TableCell SupplyCell, string[] SupplyRawValues, char Separator)
        {
            Type = 6;
            //will split the raw values into two segments. The first DDL will display the shortened segment, the second will display the entire raw value for 
            //each item that contains the shortened segment.
            CascALL = SupplyRawValues;
            CascShort = new DropDownList();
            CascFull = new DropDownList();
            CascSeparator = Separator;

            CascShort.Items.Add("");
            CascFull.Items.Add("");

            foreach (string R in CascALL)
            {
                try
                {
                    if (R != "" && !CascShort.Items.Contains(new ListItem(R.Substring(0, R.IndexOf(Separator)))))
                    {
                        CascShort.Items.Add(R.Substring(0, R.IndexOf(Separator)));
                    }
                }
                catch
                {

                }
            }

            CascShort.SelectedIndexChanged += new System.EventHandler(this.CascadeCommand);
            CascShort.AutoPostBack = true;

            CascShort.Width = 200;
            CascFull.Width = 425;


            try
            {
                //Select todays date if this is a datefilled object
                if (CascShort.Items[1].Text.Contains("(" + DateTime.Now.Year.ToString() + ")"))
                {
                    for (int R = 0; R < CascShort.Items.Count; R++)
                    {
                        if (CascShort.Items[R].Text.Contains(ReturnMonth(DateTime.Now.Month.ToString())))
                        {
                            CascShort.Items[R].Selected = true;
                            break;
                        }
                    }

                    CascFull.Items.Clear();
                    CascFull.Items.Add("");

                    foreach (string R in CascALL)
                    {
                        if (R.Contains(CascShort.SelectedItem.ToString() + CascSeparator.ToString()))
                        {
                            CascFull.Items.Add(R);
                        }
                    }

                    for (int R = 0; R < CascFull.Items.Count; R++)
                    {
                        if (CascFull.Items[R].Text.Contains(ReturnMonth(DateTime.Now.Month.ToString())) && CascFull.Items[R].Text.Contains(DateTime.Now.Day.ToString()))
                        {
                            CascFull.Items[R].Selected = true;
                            break;
                        }
                    }

                }
            }
            catch
            { }

            SupplyCell.Controls.Add(CascShort);
            Label temp = new Label();
            temp.Text = "&nbsp;~>&nbsp;";
            SupplyCell.Controls.Add(temp);
            SupplyCell.Controls.Add(CascFull);

            MakeRequired(SupplyCell);
        }

        public string ReturnMonth(string R)
        {
            switch (R)
            {
                case "1": return "January";
                case "2": return "February";
                case "3": return "March";
                case "4": return "April";
                case "5": return "May";
                case "6": return "June";
                case "7": return "July";
                case "8": return "August";
                case "9": return "September";
                case "10": return "October";
                case "11": return "November";
                case "12": return "December";
                default: return "";
            }
        }

        public void CascadeCommand(object sender, EventArgs e)
        {
            CascFull.Items.Clear();
            CascFull.Items.Add("");

            //foreach (string R in CascALL)
            //{
            //    if (R.Contains(CascShort.SelectedItem.ToString() + CascSeparator.ToString()))
            //    {
            //        CascFull.Items.Add(R);
            //    }
            //}

            for (int R = 0; R < CascALL.Length; R++)
            {
                try
                {
                    if (CascALL[R].Substring(0, CascALL[R].IndexOf(CascSeparator)) == CascShort.SelectedItem.ToString())
                    {
                        CascFull.Items.Add(CascALL[R]);
                    }
                }
                catch
                {

                }
            }

            //CascShort.SelectedItem.Text += " (" + (CascFull.Items.Count -1).ToString() + ")";

            if (CascFull.Items.Count == 2)
            {
                CascFull.Items[1].Selected = true;
            }
        }

        public void Add_DataSource_DropDownList(TableCell SupplyCell, SqlDataSource GiveDataSource, string DS_DataTextField, string DS_DataValueField, bool Required)
        {
            Type = 7;
            DB = GiveDataSource;
            Drops = new DropDownList();

            Drops.AppendDataBoundItems = true;
            Drops.Items.Add("");
            Drops.DataSource = DB;
            Drops.DataTextField = DS_DataTextField;
            Drops.DataValueField = DS_DataValueField;
            Drops.DataBind();

            Drops.Width = 650;


            SupplyCell.Controls.Add(Drops);

            if (Required) { MakeRequired(SupplyCell); }
        }

        public void Add_DataSource_CascadeDropDownList(TableCell SupplyCell, SqlDataSource GiveDataSource, string DS_DataTextField, string DS_DataValueField, char Separator, bool isAlpha)
        {
            Type = 8;
            DB = GiveDataSource;
            Drops = new DropDownList();

            Drops.DataSource = DB;
            Drops.DataTextField = DS_DataTextField;
            Drops.DataValueField = DS_DataValueField;
            Drops.DataBind();

            CascALL = new string[Drops.Items.Count];
            for (int X = 0; X < CascALL.Length; X++)
            {
                CascALL[X] = Drops.Items[X].Text;
            }

            Add_Cascade_DropDownList(SupplyCell, CascALL, Separator);
        }

        public void Add_DataSource_CascadeDropDownList(TableCell SupplyCell, string[] DEPT_NAMES, DEPT_OBJ[] DEPTMATRIX, bool isAlpha)
        {
            Type = 18;

            DEPTOBJMATRIX = DEPTMATRIX;
            DEPT_SHORTNAMES = DEPT_NAMES;

            CascShort = new DropDownList();
            CascFull = new DropDownList();

            for (int X = 0; X < DEPT_NAMES.Length; X++)
            {
                ListItem newItem = new ListItem();
                newItem.Text = DEPT_NAMES[X];
                newItem.Value = X.ToString();
                CascShort.Items.Add(newItem);
            }

            CascShort.SelectedIndexChanged += new System.EventHandler(this.CascadeCommand);
            CascShort.AutoPostBack = true;

            CascShort.Width = 200;
            CascFull.Width = 425;

        }

        public void CascadeCommand_deptID(object sender, EventArgs e)
        {
            CascFull.Items.Clear();
            CascFull.Items.Add("");

            int Marker = Convert.ToInt32(CascShort.SelectedValue.ToString());

            DEPT_OBJ START = DEPTOBJMATRIX[Marker];

            while (true)
            {
                ListItem NewItem = new ListItem();
                NewItem.Text = START.NAME;
                NewItem.Value = START.ID;
                CascFull.Items.Add(NewItem);

                if (START.NEXT == null)
                {
                    break;
                }
                else
                {
                    START = START.NEXT;
                }
            }

            CascShort.SelectedItem.Text += " (" + CascFull.Items.Count.ToString() + ")";

            if (CascFull.Items.Count == 2)
            {
                CascFull.Items[1].Selected = true;
            }
        }

        public void Add_Email_Box(TableCell SupplyCell, string LabelText, int BoxWidth, bool Required)
        {
            Type = 11;

            Text = new Label();
            Box = new TextBox();

            Text.Text = LabelText;
            Box.Width = BoxWidth;

            SupplyCell.Controls.Add(Text);
            SupplyCell.Controls.Add(Box);

            if (Children > 0)
            {
                Label Spacer = new Label();
                Spacer.Text = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                SupplyCell.Controls.Add(Spacer);
            }

            if (Required) { MakeRequired(SupplyCell); }
        }

        public void MakeRequired(TableCell SupplyCell)
        {
            try
            {
                IsRequired = new Label();
                IsRequired.Text = "**";
                IsRequired.ForeColor = System.Drawing.Color.Red;
                IsRequired.Font.Bold = true;
                IsRequired.Visible = false;
                SupplyCell.Controls.Add(IsRequired);
                BoolRequired = true;
            }
            catch
            {

            }
        }

        public void MakeRequired(TableCell SupplyCell, bool ForceNewLine)
        {
            try
            {
                IsRequired = new Label();
                IsRequired.Text = "<br>**";
                IsRequired.ForeColor = System.Drawing.Color.Red;
                IsRequired.Font.Bold = true;
                IsRequired.Visible = false;
                SupplyCell.Controls.Add(IsRequired);
                BoolRequired = true;
            }
            catch
            {

            }
        }

        public string RetrieveData(bool isHTML)
        {
            switch (Type)
            {
                case 1:
                    if (Box.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else if ((BoolRequired && Box.Text != "") || (!BoolRequired))
                    {
                        ClearErrors();
                        return (Box.Text.Replace(" ", "") != "") ? Text.Text + " " + Box.Text : "";
                    }
                    else
                    {   //TextBox
                        Box.BackColor = COLOR_HILIGHT;
                        IsRequired.Visible = true;
                        return "#Z#";
                    }
                case 2:
                    if (Rads.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else if (Rads.SelectedItem != null)
                    {
                        ClearErrors();
                        return (Rads.SelectedItem.Text == "AM" || Rads.SelectedItem.Text == "PM") ? "&nbsp;&nbsp;" + Rads.SelectedItem.Text : ((isHTML ? "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;>>&nbsp;&nbsp;&nbsp;" : "\r\n\t>>  ") + Rads.SelectedItem.Text);
                    }
                    else
                    {
                        if (Rads.Enabled == false)
                        {
                            return "";
                        }
                        else if (BoolRequired)
                        {
                            Rads.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#Z#";
                        }
                        else
                        {
                            return "";
                            //return (isHTML ? "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;>>&nbsp;&nbsp;&nbsp;" : "\r\n\t>>  ") + "None Selected";
                        }
                    }
                case 3:

                    if (Checks.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else
                    {
                        string SelectedOptions = (isHTML ? "<br />" : "\r\n");
                        int Selected = 0;

                        for (int X = 0; X < Checks.Items.Count; X++)
                        {
                            if (Checks.Items[X].Selected)
                            {
                                SelectedOptions += (isHTML ? "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;*&nbsp;&nbsp;&nbsp;" : "\t*   ") + Checks.Items[X].Text + (isHTML ? "<br />" : "\r\n");
                                Selected++;
                            }
                        }

                        if (Selected == 0 && BoolRequired)
                        {
                            Checks.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#ZCHECKS#";
                        }
                        else
                        {
                            ClearErrors();
                            if (Selected == 0)
                            {
                                return (isHTML ? "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;>>&nbsp;&nbsp;&nbsp;" : "\r\n\t>>  ") + "None Selected";
                            }
                            else
                            {
                                return SelectedOptions;
                            }
                        }
                    }
                case 4:
                    if (Drops.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else
                    {
                        if (BoolRequired)
                        {

                            if (Drops.SelectedItem.Text != "")
                            {
                                ClearErrors();
                                DROPS_REENFORCE = Drops.SelectedItem.Text;
                                return Drops.SelectedItem.Text;
                            }
                            else
                            {
                                if (DROPS_REENFORCE != "")
                                {
                                    return DROPS_REENFORCE;
                                }
                                else
                                {
                                    Drops.BackColor = COLOR_HILIGHT;
                                    IsRequired.Visible = true;
                                    return "#Z#";
                                }
                            }
                        }
                        else
                        {
                            return Drops.SelectedItem.Text;
                        }
                    }
                case 5:
                    //return DateBox.Text;
                    break;
                case 6:
                    if (CascFull.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else
                    {
                        if (CascFull.SelectedValue != "")
                        {
                            ClearErrors();
                            return CascFull.SelectedValue.ToString();
                        }
                        else
                        {
                            CascFull.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#Z#";
                        }
                    }
                case 7:
                    if (Drops.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else
                    {
                        if (BoolRequired)
                        {
                            if (Drops.SelectedValue != "")
                            {
                                ClearErrors();
                                return Drops.SelectedItem.Text;
                            }
                            else
                            {   //Droplist
                                Drops.BackColor = COLOR_HILIGHT;
                                IsRequired.Visible = true;
                                return "#Z#";
                            }
                        }
                        else
                        {
                            ClearErrors();
                            return Drops.SelectedItem.Text;
                        }
                    }
                case 11:
                    if (Box.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else
                    {
                        if ((BoolRequired && Box.Text != "" && Box.Text.Contains("@") && Box.Text.Contains(".") && (Box.Text.IndexOf("@") < (Box.Text.LastIndexOf(".") - 1)) && (Box.Text.LastIndexOf(".") != Box.Text.Length - 1)) || (!BoolRequired && Box.Text == ""))
                        {
                            string esegs = Box.Text.Substring(0, Box.Text.IndexOf('@'));
                            if (IsEmailValid(esegs))
                            {
                                ClearErrors();
                                return Text.Text + " " + Box.Text;
                            }
                            else
                            {
                                Box.BackColor = COLOR_HILIGHT;
                                IsRequired.Visible = true;
                                return "#ZEMAIL#";
                            }
                        }
                        else
                        {   //TextBox
                            Box.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#ZEMAIL#";
                        }
                    }
                case 12:
                    if (Box.Enabled == false || Drops.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else if ((BoolRequired && Box.Text != "") || (!BoolRequired))
                    {
                        if (Box.Text.Contains("@"))
                        {
                            Box.Text = Box.Text.Substring(0, Box.Text.IndexOf("@"));
                        }
                        
                        string esegs = Box.Text;
                        
                        if (esegs.Length > 0 && IsEmailValid(esegs))
                        {
                            ClearErrors();
                            return (Box.Text.Replace(" ", "") != "") ? Box.Text + Drops.SelectedItem.Text : "";
                        }
                        else
                        {
                            Box.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#ZEMAIL#";
                        }
                    }
                    else
                    {   //TextBox
                        Box.BackColor = COLOR_HILIGHT;
                        IsRequired.Visible = true;
                        return "#ZEMAIL#";
                    }
                    //else if ((BoolRequired && Box.Text != "") || (!BoolRequired))
                    //{
                    //    ClearErrors();
                    //    if (Box.Text.Contains("@"))
                    //    {
                    //        Box.Text = Box.Text.Substring(0, Box.Text.IndexOf("@"));
                    //    }

                    //    if (Regex.Replace(Box.Text, "[^0-9a-zA-Z]+", "#").Contains("#"))
                    //    {
                    //        return "#ZEMAIL#";
                    //    }

                    //    return (Box.Text.Replace(" ", "") != "") ? Box.Text + Drops.SelectedItem.Text : "";
                    //}
                    //else
                    //{   //TextBox
                    //    Box.BackColor = COLOR_HILIGHT;
                    //    IsRequired.Visible = true;
                    //    return "#ZEMAIL#";
                    //}
                case 14:
                    string SelectedCriteria = (isHTML ? "<br />" : "\r\n");
                    int Selects = 0;

                    for (int X = 0; X < Checks.Items.Count; X++)
                    {
                        if (Checks.Items[X].Selected)
                        {
                            SelectedCriteria += (isHTML ? "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" : "\t") + " (Reaffirmed) " + Checks.Items[X].Text + (isHTML ? "<br />" : "\r\n");
                            Selects++;
                        }
                    }

                    if (Selects < Checks.Items.Count)
                    {
                        Checks.BackColor = COLOR_HILIGHT;
                        IsRequired.Visible = true;
                        return "#ZFIRM#";
                    }
                    else
                    {
                        ClearErrors();
                        return SelectedCriteria;
                    }
                case 15:
                    if (!BoolRequired)
                    {
                        if (Box.Text.Replace(" ", "") != "")
                        {
                            return Box.Text + (Rads.SelectedItem != null ? Rads.SelectedItem.Text : "");
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else
                    {
                        if (Box.Text.Replace(" ", "") != "" && Rads.SelectedItem != null)
                        {
                            return Box.Text + " " + Rads.SelectedItem.Text;
                        }
                        else
                        {
                            Box.BackColor = COLOR_HILIGHT;
                            Rads.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#Z#";
                        }
                    }
                case 16:
                    string SelectedConfirm = (isHTML ? "<br />" : "\r\n");
                    int Confs = 0;

                    for (int X = 0; X < Checks.Items.Count; X++)
                    {
                        if (Checks.Items[X].Selected)
                        {
                            SelectedConfirm += (isHTML ? "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" : " \r\n\t") + " (CHECKED) " + Checks.Items[X].Text + (isHTML ? "<br />" : "\r\n") + " (" + Text.Text + ")" + (isHTML ? "<br /><br />" : "\r\n \r\n");
                            Confs++;
                        }
                    }

                    if (Confs < Checks.Items.Count)
                    {
                        Checks.BackColor = COLOR_HILIGHT;
                        IsRequired.Visible = true;
                        return "#ZFIRM#";
                    }
                    else
                    {
                        ClearErrors();
                        return SelectedConfirm;
                    }
                case 17:
                    if (Rads.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else if (Rads.SelectedItem != null && (Box.Visible == false || (Box.Visible == true && Box.Text != "")))
                    {
                        ClearErrors();
                        if (isHTML)
                        {
                            return "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + Rads.SelectedItem.Text + (Box.Visible ? " and " + Text.Text + " is " + Box.Text + "<br />" : "<br />");
                        }
                        else
                        {
                            return "\r\n\t" + Rads.SelectedItem.Text + (Box.Visible ? " and " + Text.Text + " is " + Box.Text + "\r\n" : "\r\n");
                        }
                    }
                    else
                    {
                        if (BoolRequired || Box.Visible)
                        {
                            Rads.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#Z#";
                        }
                        else
                        {
                            return "";
                            //return (isHTML ? "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;>>&nbsp;&nbsp;&nbsp;" : "\r\n\t>>  ") + "None Selected";
                        }
                    }
                case 18:
                    if (CascFull.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else
                    {
                        if (CascFull.SelectedValue != "")
                        {
                            ClearErrors();
                            return CascShort.SelectedItem.Text.ToString() + "-" + CascFull.SelectedItem.Text.ToString();
                        }
                        else
                        {
                            CascFull.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#Z#";
                        }
                    }
                case 19:
                    if (Drops.Enabled == false)
                    {
                        ClearErrors();
                        return "";
                    }
                    else
                    {
                        if (Drops.SelectedItem.Text != "" || !BoolRequired)
                        {
                            if (Box.Visible)
                            {
                                if (Box.Text.Trim() != "")
                                {
                                    return Drops.SelectedItem.Text + (isHTML ? "<br />" : "\r\n") + Text.Text + ": " + (isHTML ? "<br />" : "\r\n") + Box.Text;
                                }

                                else
                                {   //TextBox
                                    Box.BackColor = COLOR_HILIGHT;
                                    IsRequired.Visible = true;
                                    return "#Z#";
                                }
                            }
                            else
                            {
                                ClearErrors();
                                return Drops.SelectedItem.Text;
                            }

                        }
                        else
                        {   //Droplist
                            Drops.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#Z#";
                        }

                    }
                    break;
                case 20:
                    if (Drops.Enabled == false)
                    {
                        ClearErrors(20);
                        return "";
                    }
                    else
                    {
                        if (Drops.SelectedItem.Text != "")
                        {
                            if (BoolRequired)
                            {
                                if (Box.Text != "")
                                {
                                    if (IsEcapsValidated(Box.Text))
                                    {
                                        ClearErrors(20);
                                        return Drops.SelectedItem.Text + (isHTML ? "<br />" : "\r\n") + Text.Text + ": " + (isHTML ? "<br />" : "\r\n") + Text.Text + ": " + Box.Text.Trim();
                                    }

                                    else
                                    {   //TextBox
                                        Box.BackColor = COLOR_HILIGHT;
                                        //Box.Text = "IT3";
                                        IsRequired.Visible = true;
                                        return "#Z#";
                                    }
                                }
                                else
                                {   //Droplist
                                    if (BoolRequired)
                                    {
                                        //Box.Text = "IT1";
                                        Box.BackColor = COLOR_HILIGHT;
                                        IsRequired.Visible = true;
                                        return "#Z#";
                                    }
                                }
                            }
                            else
                            {
                                ClearErrors(20);
                                return Drops.SelectedItem.Text + (isHTML ? "<br />" : "\r\n") + "No eCaps ID was given.";
                            }
                        }
                        else
                        {   //Droplist
                            //Box.Text = "IT2";
                            Drops.BackColor = COLOR_HILIGHT;
                            IsRequired.Visible = true;
                            return "#Z#";
                        }

                    }
                    break;

                default:
                    break;
            }

            return "";
        }

        private bool IsEmailValid(string emailheader)
        {
            string[] allowedchars = { "-" };

            foreach (string R in allowedchars)
            {
                if (emailheader.Contains(R))
                {
                    emailheader = emailheader.Replace(R, "");
                }
            }
            
            string RegFind = Regex.Replace(emailheader, "[^0-9a-zA-Z]+", "#");
            if (RegFind.Contains('#'))
            {
                return false;
            }

            return true;
        }

        private bool IsEcapsValidated(string boxtext)
        {
            if (boxtext.Length < 5)
            {
                IsRequired.Text = "** Too short. eCaps ID consists of the employee's first and last initial plus their employee number, which is either four or five digits.";
                return false;
            }
            else if (boxtext.Length == 5)
            {
                if (!RunNumberValidator(boxtext))
                {
                    IsRequired.Text = "** Invalid characters or too short. eCaps ID consists of the employee's first and last initial plus their employee number, which is either four or five digits.";
                    return false;
                }
                else
                {
                    ClearErrors(20);
                    return true;
                }

            }
            else if (boxtext.Length > 7)
            {
                IsRequired.Text = "** Too long. eCaps ID consists of the employee's first and last initial plus their employee number, which is either four or five digits.";
                return false;
            }
            else
            {

                if (boxtext.Length > 5)
                {
                    if (!RunTextValidator(boxtext.Substring(0, 2)))
                    {
                        IsRequired.Text = "** Invalid characters or too long. eCaps ID consists of the employee's first and last initial plus their employee number, which is either four or five digits.";
                        return false;
                    }
                    else
                    {
                        if (!RunNumberValidator(boxtext.Substring(2)))
                        {
                            IsRequired.Text = "** Invalid numbers or too many letters. eCaps ID consists of the employee's first and last initial plus their employee number, which is either four or five digits.";
                            return false;
                        }
                        else
                        {
                            ClearErrors(20);
                            return true;
                        }
                    }
                }


                return true;
            }
        }

        private bool RunNumberValidator(string checktext)
        {
            if (checktext != "")
            {
                string R = checktext.Substring(0, 1);

                if (!(R == "1" || R == "2" || R == "3" || R == "4" || R == "5" || R == "6" || R == "7" || R == "8" || R == "9" || R == "0"))
                {
                    return false;
                }
            }

            if (checktext.Length > 1)
            {
                return RunNumberValidator(checktext.Substring(1));
            }
            else
            {
                return true;
            }

            return true;
        }

        private bool RunTextValidator(string checktext)
        {
            if (checktext != "")
            {
                string R = checktext.Substring(0, 1).ToLower();

                if (!(R == "a" || R == "b" || R == "c" || R == "d" || R == "e" || R == "f" || R == "g" || R == "h" || R == "i" || R == "j" || R == "k" || R == "l" || R == "m" || R == "n" || R == "o" || R == "p" || R == "q" || R == "r" || R == "s" || R == "t" || R == "u" || R == "v" || R == "w" || R == "x" || R == "y" || R == "z"))
                {
                    return false;
                }

            }

            if (checktext.Length > 1)
            {
                return RunTextValidator(checktext.Substring(1));
            }
            else
            {
                return true;
            }
        }
    }

    //=============================================================================================================================================================
    // TextDan makes sure the SQL inserts do not contain any injection code and is formatted correctly
    //=============================================================================================================================================================
    public class TextDan
    {
        public int Count;
        public string Text;
        public bool Flagged = false;

        public TextDan(string SupplyText, DropDownList SupplyDDL)
        {
            Count = 20 + SupplyDDL.Items.Count;

            Text = Regex.Replace(SupplyText, @"[\[\]\\\^\$\.\|\?\*\+\(\)\{\}%,;><!@#&\+]", "").Replace("--", "");

            foreach (ListItem R in SupplyDDL.Items)
            {
                if (Regex.Replace(Text.ToLower().Replace(" ", ""), @"[\[\]\\\^\$\.\|\?\*\+\(\)\{\}%,;><!@#&\-\+]", "") == Regex.Replace(R.Text.ToLower().Replace(" ", ""), @"[\[\]\\\^\$\.\|\?\*\+\(\)\{\}%,;><!@#&\-\+]", "") || Text.ToLower().Contains("delete") || Text.ToLower().Contains("drop"))
                {
                    Flagged = true;
                }
            }
        }

        public TextDan(string SupplyText)
        {
            try
            {
                Text = SupplyText.Replace("&", "and");
                Text = Regex.Replace(Text, @"[\[\]\\\^\$\.\|\?\*\+\(\)\{\}%,;><!@#&\+]", "").Replace("--", "");
            }
            catch
            {
                Text = "";
                Flagged = true;
            }
        }
    }

    public class DEPT_OBJ
    {
        public DEPT_OBJ NEXT;

        public string NAME;
        public string ID;

        public DEPT_OBJ(string MyName, string MyID)
        {
            NAME = MyName;
            ID = MyID;
        }
    }
}