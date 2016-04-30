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
using System.Security.Cryptography;

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
            string dataFileCheckSum = this.GetMd5Hash(dataFileAsString);

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
                string attachmentSizeXpath = "//uibizheader:ACAUIBusinessHeader/air70:ACATransmitterManifestReqDtl/irscommon:AttachmentByteSizeNum";

                XPathNavigator checkSumNav = navigator.SelectSingleNode(checksumXpath, manager);
                checkSumNav.SetValue(dataFileCheckSum);

                XPathNavigator uniqueTransIdNav = navigator.SelectSingleNode(uniqueTransIdXpath, manager);
                string uniqueTransId = uniqueTransIdNav.Value;
                int firstColon = uniqueTransId.IndexOf(':');
                //
                // Example of Unique Transmission ID
                //    5e34ed8e-f92f-42f6-ac65-8cd1eddabf23:SYS12:BB0KF::T
                //
                uniqueTransIdNav.SetValue(Guid.NewGuid().ToString() + uniqueTransId.Substring(firstColon, uniqueTransId.Length - firstColon));

                XPathNavigator attachmentSizeNav = navigator.SelectSingleNode(attachmentSizeXpath, manager);
                attachmentSizeNav.SetValue(dataFileAsString.Length.ToString());

                document.Save(manifestFile);

                MessageBox.Show(
                    String.Format("Manifest file '{0}': successfully updated attachment check sum, attachment size, and unique transmission ID.", manifestFile),
                    "File Processing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.btnProcess.Enabled = false;
                this.txtFolderPath.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Error processing manifest file : '{0}'.", ex.Message),
                    "File Processing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                this.btnProcess.Enabled = false;
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

        // From IRS-ACA Documentation
        // 3.4.4 Computing Checksum
        // There are many algorithms readily available that can be used to calculate an MD5 Checksum:
        // a. The algorithm that a transmitter uses will differ depending on their technology stack. For
        // example, if the transmitter is building the interface on Windows/.NET Framework, they will
        // need to obtain a MD5 Checksum algorithm that runs on the Windows platform. If the
        // transmitter is building their interface on Linux/Java, they will need to obtain a MD5
        // Checksum that runs on the Linux platform.

        // The MD5 Checksum computed on the Form Data File attached to the transmission

        //<xsd:simpleType name="MD5Type">
        //    <xsd:annotation>
        //        <xsd:documentation>
        //            <Component>
        //                <DictionaryEntryNm>MD5 Checksum Type</DictionaryEntryNm>
        //                <MajorVersionNum>1</MajorVersionNum>
        //                <MinorVersionNum>0</MinorVersionNum>
        //                <VersionEffectiveBeginDt>2013-05-01</VersionEffectiveBeginDt>
        //                <VersionDescriptionTxt>Initial Version</VersionDescriptionTxt>
        //                <DescriptionTxt>The MD5 Checksum Type (32 character hex value).</DescriptionTxt>
        //            </Component>
        //        </xsd:documentation>
        //    </xsd:annotation>
        //    <xsd:restriction base="xsd:string">
        //        <xsd:pattern value="[0-9A-Fa-f]{32}"/>
        //    </xsd:restriction>
        //</xsd:simpleType>

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }
}
