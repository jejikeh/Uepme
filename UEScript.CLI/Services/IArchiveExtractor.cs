using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UEScript.CLI.Commands;
using UEScript.CLI.Models;
using UEScript.CLI.Services.Enums;
using UEScript.Utils.Results;

namespace UEScript.CLI.Services
{
    public interface IArchiveExtractor
    {
        public Result<string, CommandError> Extract(FileInfo file, string destinationPath, Action<double>? progressBarAction);
        public Result<string, CommandError> Extract(Stream stream, SupportedArchiveExtractionTypes type, string destinationPath, Action<double>? progressBarAction);
    }
}
