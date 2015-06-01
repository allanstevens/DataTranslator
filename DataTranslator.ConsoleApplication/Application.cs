using System;
using System.Text;
using System.Web;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Xml;
using AllanStevens.DataTranslator.Factory;



namespace AllanStevens.DataTranslator.ConsoleApplication
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Application
	{
        enum ExitCode : int {
          CompletedWithSuccess = 0,
          CompletedWithErrors = 1,
          DidNotRun = 2,
          InvalidSourceDirectory = 3,
          InvalidTargetDirectory = 4,
          InvalidArchiveDirectory = 5,
          InvalidLogFile = 6,
          UnknownError = 10
        }

        static bool RUN_IN_DEBUG = false;

        //static string logFilename;
		//static bool flgLog;
		static int processID;
        static string sourceDirectory;
        static string targetDirectory;
        static string filter;
        static string archiveDirectory;
        static string logFilename;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            // Run in debug mode
            if (RUN_IN_DEBUG)
            {
                args = new string[]{
                    "C:\\Development\\DataTranslator",
                    "C:\\Development\\DataTranslator"
                    
                };
                //Log("XMLConverter " + sRelease + " started using debug mode.");
            }

            // Spacer
            Echo();

            //get process id, to log in log file
            processID = System.Diagnostics.Process.GetCurrentProcess().Id;

            // Set default variables
            sourceDirectory = "";
            targetDirectory = "";
            filter = "*.xml";
            archiveDirectory = "~\\archive\\";
            logFilename = "";

            // No args to return /h message
            if (args.Length == 0)
            {
                Echo("No command line parameters found use /h for more help.");
                ExitApplication(ExitCode.DidNotRun);
            }
            else
            {
                // Check for /l and /h command line switch first, so other switches are logged in file
                foreach (string arg in args)
                {
                    if (arg.ToLower().StartsWith("/l"))
                    {
                        // Set file name
                        if (arg.Length > 3)
                            logFilename = arg.Substring(3);
                        else
                            ExitApplication(ExitCode.InvalidLogFile);
                    }

                    if (arg.ToLower().StartsWith("/?") || arg.ToLower().StartsWith("/h"))
                    {
                        Echo("TRANS source target [/F[[:]filter]] [/A[[:]folder]] [/L[[:]logfile]");
                        Echo();
                        Echo("  source     Specifies the folder file(s) are to be read.");
                        Echo("  target     Specifies the folder file(s) will be created.");
                        Echo("  /F         Set a filter on source folder.");
                        Echo("  filter     If not set the default will be *.xml");
                        Echo("  /A         Specifiy archive folder to use.");
                        Echo("  folder     If not set will use source/archive folder.");
                        Echo("  /L         Generates Log file.");
                        Echo("  logfile    Filename and folder of log file.");
                        Echo();

                        ExitApplication(ExitCode.DidNotRun);
                    }
                }
            }

            // Log application title and version
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string applicationRelease =
                version.Major.ToString()
                + "." + version.Minor.ToString()
                + "." + version.Build.ToString();
            Echo("DataTranslator " + applicationRelease + " started using line parameters - " + args.Flatten(" "));
            Echo();

            // Loop throught the rest of of the commandline switches
            foreach (string arg in args)
            {
                string trimedArg = arg.ToLower();

                if (trimedArg.Length > 2) trimedArg = trimedArg.Substring(0, 2);

                switch (trimedArg)
                {
                    case "/l":
                        // Log file has already been applied above
                        break;

                    case "/f":
                        // Set filter
                        if (arg.Length > 3)
                            filter = arg.Substring(3);
                        break;

                    case "/a":
                        // Archive folder                            
                        if (arg.Length > 3)
                            archiveDirectory = arg.Substring(3);
                        if (!Directory.Exists(archiveDirectory))
                        {
                            Echo("Archive directory is not valid.");
                            ExitApplication(ExitCode.InvalidArchiveDirectory);
                        }
                        break;

                    default:
                        if (arg.Substring(0, 1) == "/")
                        {
                            Echo("Command line parameter '" + arg + "' has not been recognised use /H for more help.");
                            ExitApplication(ExitCode.DidNotRun);
                        }
                        else if (sourceDirectory == "")
                        {
                            sourceDirectory = arg;
                            // check the location exists
                            if (!Directory.Exists(sourceDirectory))
                            {
                                Echo("Source directory is not valid.");
                                ExitApplication(ExitCode.InvalidSourceDirectory);
                            }
                        }
                        else if (targetDirectory == "")
                        {
                            targetDirectory = arg;
                            // check the location exists
                            if (!Directory.Exists(targetDirectory))
                            {
                                Echo("Target directory is not valid.");
                                ExitApplication(ExitCode.InvalidTargetDirectory);
                            }
                        }
                        else
                        {
                            Echo("Parameter '" + arg + "' has not been recognised use /H for more help.");
                            ExitApplication(ExitCode.DidNotRun);
                        }
                        break;
                }
            }

            Echo("File process started at "+DateTime.Now.ToString());

            // Set the application exit code
            ExitCode exitCode = ExitCode.CompletedWithSuccess;
            
            // File process count
            int count = 0;

            // Process the list of files found in the source directory 
            foreach (string fileName in Directory.GetFiles(sourceDirectory, filter))
            {
                Echo();
                Echo("Process file: " + fileName);
                Thread.Sleep(1000);
                if (!ProcessFile(fileName))
                {
                    exitCode = ExitCode.CompletedWithErrors;
                }
                count++;
            }

            Echo();
            Echo("Processed " + count.ToString() + " file(s).");

            // Close the applicaiton with correct exit code
            ExitApplication(exitCode);
            
        }

        static bool ProcessFile(string fileToOpen)
        {
            string translatorFile = "";

            try
            {
                // Find out file type file of file
                XmlDocument docType = new XmlDocument();
                docType.Load(fileToOpen);

                // Example of how xpath could look at source file and decide the xml translator file
                //
                // if (docType.DocumentElement.SelectSingleNode(
                //    "/XMLRoot/Data/XYZ") != null)
                // {
                //    translatorFile = "TranslatorXYZ.xml";
                // }
                // else if (docType.DocumentElement.SelectSingleNode(
                //    "/XMLRoot/Data/ABC/@type").Value == "somevalue")
                // {
                //    translatorFile = "TranslatorABC.xml";
                // }
                // else
                // {
                //    Echo("Document type is unrecognised, convert aborted");
                //    return false;
                // }

                translatorFile = "Translator.xml";
            }
            catch
            {
                Echo("Document type is unrecognised, convert aborted");
                return false;
            }

            // Check file is not being used by another process
            int iCount = 0;
            int iSpin = 0;
            string sSpiner = "-\\|/";

            while (IsFileLocked(fileToOpen))
            {
                // Calculate the elapsed time and stop if the maximum retry        
                // period has been reached.   

                Console.Write("\rFile locked by another process, attempting retry " + sSpiner[iSpin] + " CTRL+C to abort.");

                Thread.Sleep(500);
                iCount++;

                //File is still locked after 30 mins
                if (iCount == 1800000)
                {
                    Echo("File locked by another process. Convert aborted.");
                    return false;
                }

                //animate spinner
                iSpin++;
                if (iSpin == 4)
                    iSpin = 0;
            }

            try
            {
                using(Translator dataTranslator = new Translator())
                {
                    dataTranslator.SourceFilename = fileToOpen;
                    dataTranslator.TargetFilename = targetDirectory + fileToOpen.Substring(fileToOpen.LastIndexOf("\\"));
                    dataTranslator.TranslatorFilename = translatorFile;
                    dataTranslator.Initialize();
                    dataTranslator.RunTranslator();
                }
                //dataTranslator = null;

                Echo("File converted");
            }
            catch (Exception ex)
            {
                Echo("Exception thrown. Convert aborted.");
                Echo("Message: " + ex.Message);
                Echo("Stack: " + ex.StackTrace);
                return false;
            }

            try
            {
                // If using default archive then replace tilda with source
                archiveDirectory = archiveDirectory.Replace("~\\", sourceDirectory + "\\");

                // Create folder if it does not exist
                if (!Directory.Exists(archiveDirectory))
                {
                    Directory.CreateDirectory(archiveDirectory);
                }

                // Move the file to archive
                File.Move(
                    fileToOpen,
                    archiveDirectory + fileToOpen.Substring(fileToOpen.LastIndexOf("\\")));
                Echo("File moved to archive");
                // Check to ensure the file has moved
                if (File.Exists(fileToOpen))
                {
                    Echo("Moved file still exists in source location, which is unexpected.");
                    return false; //will flag as an issue and exit with 'Completed with errors'
                }
            }
            catch (Exception ex)
            {
                Echo("Exception thrown. Convert aborted.");
                Echo("Message: " + ex.Message);
                Echo("Stack: " + ex.StackTrace);
                // Although the file has been created successfully, it has been deleted as the source file could not be moved to the archive
                File.Delete(targetDirectory + fileToOpen.Substring(fileToOpen.LastIndexOf("\\")));
                Echo("Target file removed.");
                return false;
            }
            
            return true;
        }

        static void ExitApplication(ExitCode exitCode)
        {
            Echo();
            Echo("Exit Code: " + (int)exitCode + " [" + exitCode.ToString() + "]");

            if (RUN_IN_DEBUG)
            {
                Console.ReadLine();
            }

            Environment.Exit((int)exitCode);
        }

		static void Log(string s)
		{
			//create log file
			using (StreamWriter sw=new StreamWriter(logFilename,true))
			{
				sw.WriteLine(DateTime.Now.ToString()+","+processID+","+s);
			}
		}

		static void Echo()
		{
			Console.WriteLine();
		}
		static void Echo(string s)
		{
			Console.WriteLine(s);
			
			if(!logFilename.Length.Equals(0))
			{
				Log(s);
			}
		
		}


		/// <summary>
		/// Check if the file is locked by another process.
		/// </summary>
		/// <param name="filename">The name of file to check.</param>
		/// <returns>
		/// Returns true if the specified file is being accessed 
		/// by another process.
		/// </returns>
		private static bool IsFileLocked(string filename)
		{    
			// If the file can be opened for exclusive access it means that the file    
			// is no longer locked by another process.    
			try    
			{        
				using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
				{            
					return false;        
				}    
			}    
			catch (IOException)    
			{        
				return true;    
			}
		}
		
	}
}
