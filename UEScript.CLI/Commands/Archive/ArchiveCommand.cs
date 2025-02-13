using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using UEScript.CLI.Common;
using UEScript.CLI.Services;
using UEScript.Utils.Extensions;
using UEScript.Utils.Results;
using System.IO;

namespace UEScript.CLI.Commands.Archive;

public static class ArchiveCommand
{
    public static Result<string, CommandError> Execute(DirectoryInfo source, FileInfo destination, IArchiveService archiver, ILogger logger)
    {
        logger.LogTrace("Starting archive command execution...");

        if (source is null)
            return new CommandError("Source is null");
        if (!source.Exists)
            return CommandError.PathIsNotDirectory(source.FullName);

        if (destination is null)
            return new CommandError("Destionation is null");

        var result = archiver.Archive(source.FullName, destination);

        if (result is null || !result.IsSuccess)
        {
            return Result<string, CommandError>.Error(result ?? new CommandError("Failed to archive"));
        }

        return result;
    }
}
