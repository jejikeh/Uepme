using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UEScript.CLI.Commands.Build;
using UEScript.CLI.Services;
using UEScript.Utils.Extensions;
using static System.CommandLine.Help.HelpBuilder;
using System.Xml.Linq;
using UEScript.CLI.Commands.Engine.Add;
using System.IO;
using UEScript.CLI.Commands.Engine.Install;
using UEScript.CLI.Services.Impl;
using UEScript.Utils.Results;

namespace UEScript.CLI.Commands.Archive.Extract;


public static class ConfigureExtractCommand
{
    public static void AddExtractCommand(this CliCommand rootCommand)
    {
        var extractCommand = new CliCommand("extract", "Exctracts archive to the destination path")
    {

        new CliArgument<FileInfo>("file")
        {
            Description = "Archive to extract",
        },
        new CliArgument<DirectoryInfo>("destination")
        {
            Description = "Destination path",
        },
    };

        extractCommand.Action = CommandHandler.Create<FileInfo, DirectoryInfo, IHost>((file, destination, host) =>
        {
            var serviceProvider = host.Services;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ExtractCommand");

            var archiveExtractor = serviceProvider.GetRequiredService<IArchiveExtractor>();
            var result = Task.Run(async () =>
                await ExtractCommand.ExecuteAsync(file, destination, archiveExtractor, logger)).Result;
            logger.LogResult(result);

        });

        rootCommand.Add(extractCommand);
    }
}