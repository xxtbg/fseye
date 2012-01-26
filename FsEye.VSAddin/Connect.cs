using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
namespace FsEye.VSAddin {
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2 {
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect() {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom) {
            try {
                _applicationObject = (DTE2)application;
                _addInInstance = (AddIn)addInInst;

                // ctlProgID - the ProgID for your user control.
                var ctlProgID = "Swensen.FsEye.Forms.WatchPanel";

                // asmPath - the path to your user control DLL.
                // Replace the <Path to VS Project> with the path to
                // the folder where you created the WindowsCotrolLibrary.
                // Remove the line returns from the path before 
                // running the add-in.
                var asmPath = @"C:\Users\Stephen\Documents\Visual Studio 2010\Projects\FsEye\code\FsEye\bin\Release\FsEye.dll";

                // guidStr - a unique GUID for the user control.
                var guidStr = "{cd187e5b-abd9-4b4b-83a3-6e99e74e0597}";

                var toolWins = (Windows2)_applicationObject.Windows;
                // Create the new tool window, adding your user control.
                object objTemp = null;
                var toolWin = toolWins.CreateToolWindow2(_addInInstance, asmPath, ctlProgID, "FsEye", guidStr, ref objTemp);

                // The tool window must be visible before you do anything 
                // with it, or you will get an error.
                if (toolWin != null)
                    toolWin.Visible = true;

                //// Set the new tool window's height and width, 
                //// and then close it.
                //System.Windows.Forms.MessageBox.Show("Setting the height 
                //to 500 and width to 400...");
                //toolWin.Height = 500;
                //toolWin.Width = 400;
                //System.Windows.Forms.MessageBox.Show
                //  ("Closing the tool window...");
                //toolWin.Close(vsSaveChanges.vsSaveChangesNo);
            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show("Exception: "
                  + ex.Message);
            }

        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom) {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom) {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom) {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom) {
        }

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
    }
}