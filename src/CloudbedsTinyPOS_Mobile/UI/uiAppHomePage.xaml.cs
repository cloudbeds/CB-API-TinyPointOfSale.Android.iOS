using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Xml;

namespace CloudbedsTinyPOS_Mobile
{

    /// <summary>
    /// This UI is in the HOME page of the application
    /// </summary>
    public partial class uiAppHomePage : StackLayout
    {
        /// <summary>
        /// Delegate and Events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal delegate void AppStartModeEventHandler(object sender, EventArgs e);
        internal event AppStartModeEventHandler StartAppWithFakeData;
        internal event AppStartModeEventHandler StartAppWithRealData;

        /// <summary>        
        /// Constructor
        /// </summary>
        public uiAppHomePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// If there are listeners, fire the event
        /// </summary>
        private void FireEvent_StartAppWithFakeData()
        {
            var evt = StartAppWithFakeData;
            if(evt != null)
            {
                evt(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// If there are listeners, fire the event
        /// </summary>
        private void FireEvent_StartAppWithRealData()
        {
            var evt = StartAppWithRealData;
            if (evt != null)
            {
                evt(this, EventArgs.Empty);
            }
        }

        private void btnStartWithTestData_Clicked(object sender, EventArgs e)
        {
            FireEvent_StartAppWithFakeData();
        }

        private void btnStartAndConnectToCloudbeds_Clicked(object sender, EventArgs e)
        {
            FireEvent_StartAppWithRealData();
        }


        /// <summary>
        /// Write a test file....
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestFileSystem_Clicked(object sender, EventArgs e)
        {
            //We need to use the directory we have permission to access
            string testFileName = System.IO.Path.Combine(
                FileIOHelper.MobileDevice_WorkingDirectoryForApp,
                "ivoTestFile.txt");

            var localFile =  System.IO.File.CreateText(testFileName);
            using(localFile)
            {
                localFile.WriteLine("A long strange trip.");
                localFile.Close();
            }
        }

        /// <summary>
        /// Import the App Config XML from text pasted into the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImportAppConfig_Clicked(object sender, EventArgs e)
        {
            string xmlCandidateText = txtTransferToFrom.Text;
            var statusLogs = TaskStatusLogsSingleton.Singleton;

            //==========================================================
            //Try to load and parse this XML 
            //==========================================================
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlCandidateText);

                //Parse it into the app config
                var appConfig = CloudbedsAppConfig.FromXmlDocument(xmlDocument);
            }
            catch(Exception ex)
            {
                string errorMessage_parseXml = "Error parsing XML as AppConfig: " + ex.Message;
                txtStatusUpdatesHere.Text = errorMessage_parseXml;
                statusLogs.AddError("1029-615: " + errorMessage_parseXml);
                return;
            }

            //==========================================================
            //Try to save XML to the mobile device file system
            //==========================================================
            try
            {
                var savePath = AppSettings.LoadPreference_PathAppSecretsConfig();
                System.IO.File.WriteAllText(savePath, xmlCandidateText);
            }
            catch (Exception exFileSave)
            {
                string errMessage_fileSave = "XML is valid, but an error occured saving it: " + exFileSave.Message;
                txtStatusUpdatesHere.Text = errMessage_fileSave;
                statusLogs.AddError("1029-631: " + errMessage_fileSave);
                return;
            }

            txtStatusUpdatesHere.Text = "Success importing/saving app config XML.";
        }

        /// <summary>
        /// Import data for an Authentication session that we need to use...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImportAuthSessionSecrets_Clicked(object sender, EventArgs e)
        {
            string xmlCandidateText = txtTransferToFrom.Text;
            var statusLogs = TaskStatusLogsSingleton.Singleton;

            //==========================================================
            //Try to load and parse this XML 
            //==========================================================
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlCandidateText);

                //Parse it into the app config
                var accessSecrets = CloudbedsAuthSession_OAuth.FromXml(xmlDocument, statusLogs);
            }
            catch (Exception ex)
            {
                string errorMessage_parseXml = "Error parsing XML as AUTH SESSION: " + ex.Message;
                txtStatusUpdatesHere.Text = errorMessage_parseXml;
                statusLogs.AddError("1029-644: " + errorMessage_parseXml);
                return;
            }

            //==========================================================
            //Try to save XML to the mobile device file system
            //==========================================================
            try
            {
                var savePath = AppSettings.LoadPreference_PathUserAccessTokens();
                System.IO.File.WriteAllText(savePath, xmlCandidateText);
            }
            catch (Exception exFileSave)
            {
                string errMessage_fileSave = "XML is valid, but an error occured saving it: " + exFileSave.Message;
                txtStatusUpdatesHere.Text = errMessage_fileSave;
                statusLogs.AddError("1029-642: " + errMessage_fileSave);
                return;
            }

            txtStatusUpdatesHere.Text = "Success importing/saving app config XML.";
        }

        private void btnClearTransferText_Clicked(object sender, EventArgs e)
        {
            txtTransferToFrom.Text = "";
        }
    }
}
