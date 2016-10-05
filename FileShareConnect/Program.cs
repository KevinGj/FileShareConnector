using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure; // Namespace for Azure Configuration Manager
using Microsoft.WindowsAzure.Storage; // Namespace for Storage Client Library
// using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage
using Microsoft.WindowsAzure.Storage.File; // Namespace for File storage
using System.Runtime.InteropServices;

namespace FileShareConnect
{
    class Program
    {
       


        [DllImport("Mpr.dll",
            EntryPoint = "WNetAddConnection2",
            CallingConvention = CallingConvention.Winapi)]
        private static extern int WNetAddConnection2(NETRESOURCE lpNetResource,
                                             string lpPassword,
                                             string lpUsername,
                                             System.UInt32 dwFlags);

        [DllImport("Mpr.dll",
                   EntryPoint = "WNetCancelConnection2",
                   CallingConvention = CallingConvention.Winapi)]
        private static extern int WNetCancelConnection2(string lpName,
                                                        System.UInt32 dwFlags,
                                                        System.Boolean fForce);

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public int dwScope;
            public ResourceType dwType;
            public int dwDisplayType;
            public int dwUsage;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        };

        public enum ResourceType
        {
            RESOURCETYPE_DISK = 1,
        };


        static void Main(string[] args)
        {

         string shareName = "\\\\unipertest.file.core.windows.net\\SecretDocuments";
         string driveLetterAndColon = "z:";
         string username = "unipertest";
         string password = "aM9MW1mxInlwS9ILsorAMOrm9JPhFis9Ax7GbA7SvY0kBBK3+6Q/gcdgCABw/AMLgyPRi2RjNbvIgtiM61GkJg==";


        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create a CloudFileClient object for credentialed access to File storage.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            // Get a reference to the file share we created previously.
            CloudFileShare share = fileClient.GetShareReference("secretdocuments");

            // Ensure that the share exists.
            if (share.Exists())
            {
                // Get a reference to the root directory for the share.
                CloudFileDirectory rootDir = share.GetRootDirectoryReference();

                // Get a reference to the directory we created previously.
                CloudFileDirectory sampleDir = rootDir.GetDirectoryReference("CustomLogs");

                // Ensure that the directory exists.
                if (sampleDir.Exists())
                {
                    // Get a reference to the file we created previously.
                    CloudFile file = sampleDir.GetFileReference("Log1.txt");

                    // Ensure that the file exists.
                    if (file.Exists())
                    {
                        // Write the contents of the file to the console window.
                        Console.WriteLine(file.DownloadTextAsync().Result);

                        Console.WriteLine("Successfully connected to file share");

                        System.Threading.Thread.Sleep(300);
                    }
                }

                if (!String.IsNullOrEmpty(driveLetterAndColon))
                {
                    // Make sure we aren't using this driveLetter for another mapping
                    WNetCancelConnection2(driveLetterAndColon, 0, true);
                }

                NETRESOURCE nr = new NETRESOURCE();
                nr.dwType = ResourceType.RESOURCETYPE_DISK;
                nr.lpRemoteName = shareName;
                nr.lpLocalName = driveLetterAndColon;

                int result = WNetAddConnection2(nr, password, username, 0);

                if (result != 0)
                {
                    throw new Exception("WNetAddConnection2 failed with error " + result);
                }
            }
        }
            
    }
}
