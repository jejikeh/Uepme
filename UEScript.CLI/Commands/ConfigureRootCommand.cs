using System.CommandLine;
using UEScript.CLI.Commands.Build;
using UEScript.CLI.Commands.Engine;
using UEScript.CLI.Commands.Archive;

namespace UEScript.CLI.Commands;

public static class ConfigureRootCommand
{
    public static CliRootCommand AddRootCommand()
    {
        var rootCommand = new CliRootCommand("Unreal Engine Command Line Interface")
        {
            new CliOption<bool>("--debug-log")
            {
                Description = "Enable debug logging",
                Hidden = true
            }
        };

        rootCommand.AddBuildCommand();
        rootCommand.AddEngineCommand();
        rootCommand.AddArchiveCommand();
        
        return rootCommand;
    }
}