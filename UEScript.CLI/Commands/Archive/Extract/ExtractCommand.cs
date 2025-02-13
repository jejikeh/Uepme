using Microsoft.Extensions.Logging;
using SharpCompress.Common;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UEScript.CLI.Common;
using UEScript.CLI.Services;
using UEScript.CLI.Services.Impl;
using UEScript.Utils.Extensions;
using UEScript.Utils.Results;

namespace UEScript.CLI.Commands.Archive.Extract;

public static class ExtractCommand
{

    public static async Task<Result<string, CommandError>> ExecuteAsync(FileInfo file, DirectoryInfo destinationPath, IArchiveExtractor archiveExtractor, ILogger logger)
    {
        logger.LogTrace("Starting extract command execution...");

        if (file is null)
            return new CommandError("File is null");
        if (!file.Exists)
            return CommandError.FileNotFound(file);

        if (destinationPath is null)
            return new CommandError("Destination path is null");
        if (!destinationPath.Exists)
            return CommandError.PathIsNotDirectory(destinationPath.FullName);

        var result = await Extract(file, destinationPath.FullName, archiveExtractor);

        if (result is null || !result.IsSuccess)
            return Result<string, CommandError>.Error(result ?? new CommandError("Failed to extract archive"));

        if (file.Name.EndsWith(".tar.gz"))
        {
            logger.LogResult(result);
            logger.LogInformation("Trying to extract .tar archive from .tar.gz archive");

            result = await Extract(new FileInfo(destinationPath.FullName + "/" + file.Name.Substring(0, file.Name.Length - 3)), destinationPath.FullName, archiveExtractor);
            
            if (result is null || !result.IsSuccess)
                return Result<string, CommandError>.Error(result ?? new CommandError("Failed to extract .tar archive from .tar.gz archive"));
        }

        return result;
    }

    private static async Task<Result<string, CommandError>?> Extract(FileInfo file, string dir, IArchiveExtractor archiveExtractor)
    {
        var extractResult = default(Result<string, CommandError>);

        await AnsiConsoleUtils.WrapTaskAroundProgressBar("Extracting: ", async (ctx) =>
        {
            extractResult = archiveExtractor.Extract(file, dir, (value) => ctx.Value(value));
        });

        return extractResult;
    }
}
