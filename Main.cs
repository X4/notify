namespace notify
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
			if (args.Length == 1) {
				// Display the proper way to call the program.
				Console.WriteLine ("Usage: Notify.exe <Watched Path>");
				return;
			}

			// Create a new FileSystemWatcher and set its properties.
			FileSystemWatcher watcher = new FileSystemWatcher ();

			watcher.Path = args [1];
			/* Watch for changes in LastAccess and LastWrite times, and
			   the renaming of files or directories. */
			watcher.NotifyFilter =
				NotifyFilters.LastAccess | NotifyFilters.LastWrite |
				NotifyFilters.FileName | NotifyFilters.DirectoryName;

			// Only watch text files.
			watcher.Filter = "*.pdf";

			// Add event handlers.
			watcher.Changed += new FileSystemEventHandler (OnCreated);
			watcher.Created += new FileSystemEventHandler (OnChanged);
			watcher.Deleted += new FileSystemEventHandler (OnDeleted);
			watcher.Renamed += new RenamedEventHandler(OnRenamed);

			// Watch subfolders.
			watcher.IncludeSubdirectories = true;
			// Begin watching.
			watcher.EnableRaisingEvents = true;

			// Wait for the user to quit the program.
			Console.WriteLine("Press \'q\' to quit the app.");
			while(Console.Read()!='q');
		}

		private static void OnCreated(object source, FileSystemEventArgs e)
		{
		    if (e.ChangeType == WatcherChangeTypes.Created)
		    {
		        if (Directory.Exists(e.FullPath))
		        {
		            foreach (string file in Directory.GetFiles(e.FullPath))
		            {
		                var eventArgs = new FileSystemEventArgs(
		                    WatcherChangeTypes.Created,
		                    Path.GetDirectoryName(file),
		                    Path.GetFileName(file));
		                OnCreated(source, eventArgs);
		            }
		        }
		        else
		        {
		            Console.WriteLine("{0} created.",e.FullPath);
		        }
		    }
		}

		private static void OnChanged (object source, FileSystemEventArgs e)
		{
			// Notify, when a file is changed, or created.
			Console.WriteLine ("File changed: " + e.FullPath + " " + e.ChangeType);

			// Create process
			System.Diagnostics.Process command = new System.Diagnostics.Process ();

			// Contains path and file name of command to run
			command.StartInfo.FileName = "pdf2htmlEX";

			command.StartInfo.Arguments = String.Format(" --hdpi 96 --vdpi 96 --printing 1 --optimize-text 1 --process-outline 0 --bg-format jpg \"{0}\"", e.FullPath);
			command.StartInfo.UseShellExecute = false;

			// Set output of program to be written to process output stream
			command.StartInfo.RedirectStandardOutput = true;

			// Pass working directory
			string workingDir = Path.GetDirectoryName(e.FullPath);
			command.StartInfo.WorkingDirectory = workingDir;

			// Start the process
			command.Start ();

			// Get program output
			command.StandardOutput.ReadToEnd ();

			// Wait for process to finish
			command.WaitForExit ();
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
			string OldFileName = Path.GetDirectoryName(e.FullPath) + "/" + Path.GetFileNameWithoutExtension(e.FullPath) + ".html";

			// Try to delete the old file
			Console.WriteLine ("File: {0} deleted", OldFileName);
			File.Delete(OldFileName);
		}
	}
}