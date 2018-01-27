using MonoTorrent.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentCleaner
{
    class Program
    {
        private const string TorrentParameterName = "-torrent";
        private const string DestinationPathParameterName = "-destination";
        private const string BackupPathParameterName = "-backup";

        /// <summary>
        /// Usage: TorrentCleaner -torrent [.torrent file location] -destination [destination torrent directory] -backup [backup directory name]
        /// </summary>
        /// <param name="args"></param>
        static int Main(string[] args)
        {
            // Parse arguments
            var torrentFilePath = GetCommandLineArgument(args, TorrentParameterName);
            var desinationPath = GetCommandLineArgument(args, DestinationPathParameterName);
            var backupPath = GetCommandLineArgument(args, BackupPathParameterName);

            if (new[] { torrentFilePath, desinationPath, backupPath }.Any(x => x == null))
            {
                Console.Error.WriteLine("Invalid arguments!");
                Console.WriteLine("Usage: -torrent [.torrent file location] -destination [destination torrent directory] -backup [backup directory name]");
                return 1;
            }

            Console.WriteLine("Arguments:");
            Console.WriteLine("==========");
            Console.WriteLine("{0} = {1}", TorrentParameterName, torrentFilePath);
            Console.WriteLine("{0} = {1}", DestinationPathParameterName, desinationPath);
            Console.WriteLine("{0} = {1}", BackupPathParameterName, backupPath);

            // Validate and process arguments
            Torrent torrent;
            if (!Torrent.TryLoad(torrentFilePath, out torrent))
            {
                Console.Error.WriteLine("Invalid torrent file {0}", torrentFilePath);
                return 1;
            }

            if (!Directory.Exists(desinationPath))
            {
                Console.Error.WriteLine("Destination path '{0}' does not represent a valid directory.", desinationPath);
                return 1;
            }

            var destinationDirectoryInfo = new DirectoryInfo(desinationPath);

            if (!Directory.Exists(backupPath))
            {
                Console.Error.WriteLine("Backup path '{0}' does not represent a valid directory.", backupPath);
                return 1;
            }

            // Find the files defined by the torrent.
            var filesDefinedByTorrent = torrent.Files.Select(x => new FileInfo(Path.Combine(destinationDirectoryInfo.FullName, x.FullPath))).ToList();

            if (filesDefinedByTorrent.Any(x => !x.Exists))
            {
                Console.Error.WriteLine("Torrent '{0}' contains files that destination directory '{1}' does not. TorrentCleaner only supports reconciling completed torrents.", torrentFilePath, desinationPath);
                return 1;
            }

            // List the files that shouldn't be there.
            var filesInDestinationDirectory = destinationDirectoryInfo.EnumerateFiles("*", SearchOption.AllDirectories);

            var unwantedFiles = filesInDestinationDirectory.Except(filesDefinedByTorrent, FileSystemInfoFullNameComparer.Instance).Cast<FileInfo>().ToList();

            foreach (var file in unwantedFiles)
            {
                Console.WriteLine(file.FullName);
            }

            Console.WriteLine("{0} unwanted files, totalling {1} bytes", unwantedFiles.Count, unwantedFiles.Sum(x => x.Length));

            if (unwantedFiles.Any())
            {
                Console.WriteLine("Type 'YES' to move these files to the backup directory '{0}'", backupPath);
                var response = Console.ReadLine();

                if (!"yes".Equals(response, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Operation aborted.");
                    return 0;
                }

                foreach (var file in unwantedFiles)
                {
                    var destinationFileName = new FileInfo(file.FullName.Replace(desinationPath, backupPath));

                    Console.Write("Moving file '{0}' to '{1}'...", file.FullName, destinationFileName.FullName);

                    if (!destinationFileName.Directory.Exists)
                    {
                        destinationFileName.Directory.Create();
                    }

                    file.MoveTo(destinationFileName.FullName);

                    Console.WriteLine("Done.");
                }

                Console.WriteLine("Operation complete.");
            }

            var directoriesInTorrent = filesDefinedByTorrent.Select(x => x.Directory).Distinct(FileSystemInfoFullNameComparer.Instance);
            IEnumerable<DirectoryInfo> directoriesInDestination = destinationDirectoryInfo.EnumerateDirectories("*", SearchOption.AllDirectories).Where(x => !x.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).Any()).ToList();

            var unwantedDirectories = directoriesInDestination.Except(directoriesInTorrent, FileSystemInfoFullNameComparer.Instance).Cast<DirectoryInfo>().ToList();

            foreach (var directory in unwantedDirectories)
            {
                Console.WriteLine(directory.FullName);
            }

            Console.WriteLine("{0} unwanted directories", unwantedDirectories.Count);

            if (unwantedDirectories.Any())
            {
                Console.WriteLine("Type 'YES' to delete these directories.");
                var response = Console.ReadLine();

                if (!"yes".Equals(response, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Operation aborted.");
                    return 0;
                }

                foreach (var directory in unwantedDirectories)
                {
                    if (directory.GetFileSystemInfos("*", SearchOption.AllDirectories).Any())
                    {
                        throw new ApplicationException(string.Format("Unexpected files in directory '{0}' that was marked for deletion", directory.FullName));
                    }

                    Console.Write("Deleting directory '{0}'...", directory.FullName);

                    directory.Delete();

                    Console.WriteLine("Done.");
                }

                Console.WriteLine("Operation complete");
            }

            return 0;
        }

        /// <summary>
        /// Extracts the given command line argument by searching for argumentName and then returning the argument immediately following it.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="argumentName"></param>
        /// <returns></returns>
        private static string GetCommandLineArgument(string[] args, string argumentName)
        {
            var argumentValue = args.SkipWhile(arg => !argumentName.Equals(arg, StringComparison.OrdinalIgnoreCase)).Skip(1).FirstOrDefault();
            return argumentValue;
        }
    }
}
