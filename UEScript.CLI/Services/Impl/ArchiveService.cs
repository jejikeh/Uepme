using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using UEScript.CLI.Commands;
using UEScript.Utils.Results;
using UEScript.CLI.Services.Enums;

namespace UEScript.CLI.Services.Impl
{
    public class ArchiveService(ILogger<ArchiveService> logger) : IArchiveService
    {
        Result<string, CommandError> ProcessArchive<T>(T archive, string sourcePath, string destinationPath)
            where T : IWritableArchive
        {
            try
            {
                archive.AddAllFromDirectory(sourcePath);
                archive.SaveTo(destinationPath, new WriterOptions((archive is TarArchive) ? CompressionType.None : CompressionType.Deflate));

                return Result<string, CommandError>.Ok("Directory was successfully archived to " + destinationPath);
            }
            catch (Exception e)
            {
                return Result<string, CommandError>.Error(new CommandError($"Error: " + e.Message));
            }
        }

        delegate IWritableArchive ArchiveCreator();
        static Dictionary<SupportedArchivePackTypes, ArchiveCreator> archives = new() {
            { SupportedArchivePackTypes.zip, ZipArchive.Create },
            { SupportedArchivePackTypes.tar, TarArchive.Create },
            { SupportedArchivePackTypes.gz, GZipArchive.Create },
        };

        public Result<string, CommandError> Archive(string sourcePath, FileInfo destination)
        {
            var type = Archives.GetArchivePackType(destination.Extension);

            if (type == SupportedArchivePackTypes.unknown)
                return Result<string, CommandError>.Error(new CommandError("Unknown archive type"));

            return ProcessArchive(archives[type](), sourcePath, destination.FullName);
        }
    }
}
