using Microsoft.Extensions.Logging;
using UEScript.Utils.Results;
using UEScript.CLI.Commands;
using SharpCompress.Archives.Zip;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Archives.Rar;
using SharpCompress.Common.Tar;
using SharpCompress.Common.Zip;
using SharpCompress.Common.Rar;
using SharpCompress.Common.SevenZip;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.GZip;
using SharpCompress.Common.GZip;
using SharpCompress;
using Spectre.Console;
using UEScript.CLI.Services.Enums;

namespace UEScript.CLI.Services.Impl;


internal class ArchiveExtractor(ILogger<ArchiveExtractor> logger) : IArchiveExtractor
{
    static Result<string, CommandError> ProcessArchive<T, TEntry, TVolume>(object arch, string destinationPath, Action<double>? progressBarAction)
        where T : AbstractArchive<TEntry, TVolume> 
        where TEntry : IArchiveEntry
        where TVolume : IVolume
    {
        var archive = (T)arch;
        try
        {
            int lastPerc = 0;
            long totalRead = 0;
            if (progressBarAction is not null)
                progressBarAction(0);

            if (archive is SevenZipArchive)
                archive.CompressedBytesRead += (sender, e) =>
                {
                    double progress = (double)e.CurrentFilePartCompressedBytesRead / archive.TotalSize * 100;
                    if (progressBarAction is not null)
                        progressBarAction(progress);
                };
            else if (archive is not GZipArchive)
                archive.EntryExtractionEnd += (sender, e) =>
                {
                    totalRead += e.Item.CompressedSize;
                    double progress = (double)totalRead / archive.TotalSize * 100;
                    if (progressBarAction is not null)
                        progressBarAction(progress);
                };

            archive.Entries
                .Where(e => !e.IsDirectory)
                .ForEach(e => e.WriteToDirectory(destinationPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true }));

            if (progressBarAction is not null)
                progressBarAction(100);

            return Result<string, CommandError>.Ok("Archive was successfully extracted to " + destinationPath);
        }
        catch (Exception e)
        {
            return Result<string, CommandError>.Error(new CommandError($"Error: " + e.Message));
        }
    }

    delegate object ArchiveFileExtractor(FileInfo info);
    delegate object ArchiveStreamExtractor(Stream stream);
    delegate Result<string, CommandError> ArchiveExtractionProcessor(object archive, string destinationPath, Action<double>? progressBarAction);
    
    // окей, эти генерики просто нельзя сделать более адекватным образом

    Dictionary<SupportedArchiveExtractionTypes, (ArchiveExtractionProcessor processor, ArchiveFileExtractor fileExtractor, ArchiveStreamExtractor streamExtractor)> extractors = new()
    {
        { SupportedArchiveExtractionTypes.zip, (ProcessArchive<ZipArchive, ZipArchiveEntry, ZipVolume>, (FileInfo info) => ZipArchive.Open(info), (Stream stream) => ZipArchive.Open(stream)) },
        { SupportedArchiveExtractionTypes.rar, (ProcessArchive<RarArchive, RarArchiveEntry, RarVolume>, (FileInfo info) => RarArchive.Open(info), (Stream stream) => RarArchive.Open(stream)) },
        { SupportedArchiveExtractionTypes.sevenz, (ProcessArchive<SevenZipArchive, SevenZipArchiveEntry, SevenZipVolume>, (FileInfo info) => SevenZipArchive.Open(info), (Stream stream) => SevenZipArchive.Open(stream)) },
        { SupportedArchiveExtractionTypes.tar, (ProcessArchive<TarArchive, TarArchiveEntry, TarVolume>, (FileInfo info) => TarArchive.Open(info), (Stream stream) => TarArchive.Open(stream)) },
        { SupportedArchiveExtractionTypes.gz, (ProcessArchive<GZipArchive, GZipArchiveEntry, GZipVolume>, (FileInfo info) => GZipArchive.Open(info), (Stream stream) => GZipArchive.Open(stream)) },
    };

    public Result<string, CommandError> Extract(FileInfo file, string destinationPath, Action<double>? progressBarAction = null)
    {
        var type = Archives.GetArchiveExtractionType(file.Extension);
        if (type == SupportedArchiveExtractionTypes.unknown)
            return Result<string, CommandError>.Error(new CommandError("Unknown archive type"));

        var extractor = extractors[type];
        return extractor.processor(extractor.fileExtractor(file!), destinationPath, progressBarAction);
    }
    public Result<string, CommandError> Extract(Stream stream, SupportedArchiveExtractionTypes type, string destinationPath, Action<double>? progressBarAction = null)
    {
        if (type == SupportedArchiveExtractionTypes.unknown)
            return Result<string, CommandError>.Error(new CommandError("Unknown archive type"));

        var extractor = extractors[type];
        return extractor.processor(extractor.streamExtractor(stream!), destinationPath, progressBarAction);
    }
}
