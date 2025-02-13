using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UEScript.CLI.Commands;
using UEScript.Utils.Results;

namespace UEScript.CLI.Services
{
    public interface IArchiveService
    {
        public Result<string, CommandError> Archive(string sourcePath, FileInfo destination);
    }

}
