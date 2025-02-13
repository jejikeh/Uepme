using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UEScript.CLI.Commands.Archive.Extract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UEScript.CLI.Services;
using UEScript.Utils.Extensions;

namespace UEScript.CLI.Commands.Archive;

public static class ConfigureArchiveCommand
{
    public static void AddArchiveCommand(this CliRootCommand rootCommand)
    {
        var archiveCommand = new CliCommand("archive", "Archives the source directory to the destination path with archive name by it's extension")
        {
            new CliArgument<DirectoryInfo>("source")
            {
                Description = "Source directory to archive (archives the whole directory, not just a single file)",
            },
            new CliArgument<FileInfo>("destination")
            {
                Description = "Destination path with archive name and extension\nSupported: .zip, .tar, .gz",
            },
        };

        archiveCommand.AddExtractCommand();

        archiveCommand.Action = CommandHandler.Create<DirectoryInfo, FileInfo, IHost>((source, destination, host) =>
        {
            var serviceProvider = host.Services;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ArchiveCommand");

            var archiver = serviceProvider.GetRequiredService<IArchiveService>();
            var result = ArchiveCommand.Execute(source, destination, archiver, logger);
            logger.LogResult(result);
        });

        rootCommand.Add(archiveCommand);
    }
}