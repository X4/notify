namespace fifnotify
{
	using System;
	using System.IO;
	using System.Security.Permissions;
	using System.Linq;

	public class Watcher
	{

		public static void Main()
		{
			Run();
		}

		[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
		public static void Run ()
		{
			string[] args = System.Environment.GetCommandLineArgs ();

			// If a directory is not specified, exit program.
			if (args.Length != 2) {
				// Display the proper way to call the program.
				Console.WriteLine ("Usage: Watcher.exe WorkingDirectory Path Command (Options)");
				return;
			}

			// Create a new FileSystemWatcher and set its properties.
			FileSystemWatcher watcher = new FileSystemWatcher ();

			watcher.Path = args [2];
			/* Watch for changes in LastAccess and LastWrite times, and
			   the renaming of files or directories. */
			watcher.NotifyFilter =
				NotifyFilters.LastAccess | NotifyFilters.LastWrite |
				NotifyFilters.FileName | NotifyFilters.DirectoryName;

			// Only watch text files.
			watcher.Filter = "*.pdf";

			// Add event handlers.
			watcher.Changed += new FileSystemEventHandler (OnChanged);
			watcher.Created += new FileSystemEventHandler (OnChanged);
			watcher.Deleted += new FileSystemEventHandler (OnDeleted);
			watcher.Renamed += new RenamedEventHandler(OnRenamed);

			// Begin watching.
			watcher.EnableRaisingEvents = true;

			// Wait for the user to quit the program.
			Console.WriteLine("Press \'q\' to quit the app.");
			while(Console.Read()!='q');
		}

		// Define event handlers.
		private static void OnChanged (object source, FileSystemEventArgs e)
		{
			// Notify, when a file is changed, or created.
			Console.WriteLine ("File changed: " + e.FullPath + " " + e.ChangeType);

			// Create process
			System.Diagnostics.Process command = new System.Diagnostics.Process ();

			// Get command line arguments
			string[] commandArgs = System.Environment.GetCommandLineArgs ();
			string commandArgsExtra = "";

			// commandArgs contains path and file name of command to run
			command.StartInfo.FileName = commandArgs [3];

			// Additionally commandArgs contains the parameters to pass to program
			//command.StartInfo.Arguments = string.Join (" ", commandArgs.Skip (2));
			for(int i=3; i < commandArgs.Length; i++) {
				commandArgsExtra += commandArgs[i];
			}

			command.StartInfo.Arguments = commandArgsExtra;

			command.StartInfo.UseShellExecute = false;

			// Set output of program to be written to process output stream
			command.StartInfo.RedirectStandardOutput = true;

			// Optionally pass working directory
			if (string.IsNullOrEmpty(commandArgs [1]) ) {
				command.StartInfo.WorkingDirectory = commandArgs[1];
			}

			// Start the process
			command.Start ();

			// Get program output
			string strOutput = command.StandardOutput.ReadToEnd ();

			// Wait for process to finish
			command.WaitForExit ();

			// Show output for processed files
			Console.WriteLine ("Log: {0}", strOutput);
		}

		private static void OnRenamed(object source, RenamedEventArgs e)
		{
			// Do nothing, when a file is renamed.
			Console.WriteLine("File: {0} renamed to {1}, no need to recompile", e.OldFullPath, e.FullPath);
		}

		private static void OnDeleted (object source, FileSystemEventArgs e)
		{
			// Delete corresonding *.html, when a file is deleted.
			Console.WriteLine ("File: {0} deleted", e.FullPath);

			// Swap filename with that of the target format
			string newExt = "html";
			string OldFileName = Path.GetFileNameWithoutExtension(e.FullPath) + newExt;

			// Try to delete the old file
			File.Delete(OldFileName);
		}
	}
}


