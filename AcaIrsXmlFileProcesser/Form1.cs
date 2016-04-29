using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security;
using System.Xml;
using System.Xml.XPath;

namespace AcaIrsXmlFileProcesser
{
    public partial class Form1 : Form
    {
        private static string MANIFEST_FILE_PATTERN = "*_Manifest.xml";
        private static string DATA_FILE_PATTERN     = "1094C_Request_*.xml";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.btnProcess.Enabled = true;

                //string folderPath = @"C:\Users\slchampeau\Downloads\_ACA-IRS";
                string folderPath = folderBrowserDialog1.SelectedPath;

                if (
                       (Directory.GetDirectories(folderPath).Count() > 0)
                    || (Directory.GetFiles(folderPath).Count() != 2)
                    || (Directory.GetFiles(folderPath, "*.xml").Count() != 2)
                    )
                {
                    MessageBox.Show(
                        "Selected folder must contain 2 XML files, and nothing else.",
                        "Folder Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                if (this.GetManifestFile(folderPath) == null)
                {
                    return;
                }

                if (this.GetDataFile(folderPath) == null)
                {
                    return;
                }

                this.btnProcess.Enabled = true;
                this.txtFolderPath.Text = folderPath;
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            string manifestFile = GetManifestFile(this.txtFolderPath.Text);
            string dataFile = GetDataFile(this.txtFolderPath.Text);

            if (manifestFile == null || dataFile == null)
            {
                this.btnProcess.Enabled = false;
                return;
            }

            FileInfo manifestFileInfo = new FileInfo(manifestFile);
            if (manifestFileInfo.IsReadOnly)
            {
                MessageBox.Show(
                    String.Format("Manifest file cannot be read-only: '{0}'.", manifestFile),
                    "File Processing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                this.btnProcess.Enabled = false;
                return;
            }

            string dataFileAsString = File.ReadAllText(dataFile);
            string dataFileCheckSum = Guid.NewGuid().ToString();

            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(manifestFile);
                XPathNavigator navigator = document.CreateNavigator();

                XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);

                manager.AddNamespace("uibizheader", "urn:us:gov:treasury:irs:msg:acauibusinessheader");
                manager.AddNamespace("bizheader", "urn:us:gov:treasury:irs:msg:acabusinessheader");
                manager.AddNamespace("air70", "urn:us:gov:treasury:irs:ext:aca:air:7.0");
                manager.AddNamespace("irscommon", "urn:us:gov:treasury:irs:common");

                string checksumXpath = "//uibizheader:ACAUIBusinessHeader/air70:ACATransmitterManifestReqDtl/irscommon:ChecksumAugmentationNum";
                string uniqueTransIdXpath = "//uibizheader:ACAUIBusinessHeader/bizheader:ACABusinessHeader/air70:UniqueTransmissionId";

                XPathNavigator checkSumNav = navigator.SelectSingleNode(checksumXpath, manager);
                checkSumNav.SetValue(dataFileCheckSum);

                XPathNavigator uniqueTransIdNav = navigator.SelectSingleNode(uniqueTransIdXpath, manager);
                string uniqueTransId = uniqueTransIdNav.Value;
                uniqueTransIdNav.SetValue(uniqueTransId + "SLC");

                document.Save(manifestFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Error processing manifest file : '{0}'.", ex.Message),
                    "File Processing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                this.btnProcess.Enabled = false;
                return;
            }
        }

        private string GetManifestFile(string folderPath)
        {
            string[] file = Directory.GetFiles(folderPath, MANIFEST_FILE_PATTERN);

            if (file.Count() != 1)
            {
                MessageBox.Show(
                    String.Format("Selected folder must contain manifest XML file that matches pattern: '{0}'.", MANIFEST_FILE_PATTERN),
                    "Folder Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return null;
            }
            return file[0];
        }

        private string GetDataFile(string folderPath)
        {
            string[] file = Directory.GetFiles(folderPath, DATA_FILE_PATTERN);

            if (file.Count() != 1)
            {
                MessageBox.Show(
                    String.Format("Selected folder must contain data XML file that matches pattern: '{0}'.", DATA_FILE_PATTERN),
                    "Folder Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return null;
            }
            return file[0];
        }
    }
}
