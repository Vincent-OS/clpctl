using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading;

/// <summary>
/// Create the root command and subcommands. Also create the necessary arguments for the specific subcommands.
/// </summary>
var rootCommand = new RootCommand("clpctl -  CLI Interface for Core LivePatch for Vincent OS.");
var listCommand = new Command("list", "List all installed CLP patches.");
var updateCommand = new Command("update", "Update the CLP database and apply new patches.");
var installCommand = new Command("install", "Install a CLP patch.");
var installArgument = new Argument<string>("patch")
{
    Description = "The patch to install (in .clp format)."
};
installCommand.Arguments.Add(installArgument);
var uninstallCommand = new Command("uninstall", "Uninstall a CLP patch.");
var uninstallArgument = new Argument<string>("patch")
{
    Description = "The patch to uninstall."
};
uninstallCommand.Arguments.Add(uninstallArgument);

/// <summary>
/// Assign the handler for commands.
/// </summary>
installCommand.SetAction((ParseResult parse, CancellationToken token) =>
{
    string file = parse.GetValue(installArgument);
    var command = new CLP.CLI.InstallCommand();
    command.InstallPatch(file);
    return Task.FromResult(0);
});
listCommand.SetAction((ParseResult parse) =>
{
    var command = new CLP.CLI.ListCommand();
    command.ListInstalledPatches();
});
uninstallCommand.SetAction((ParseResult parse, CancellationToken token) =>
{
    string patch = parse.GetValue(uninstallArgument);

    var command = new CLP.CLI.UninstallCommand();
    command.UninstallPatch(patch);

    return Task.FromResult(0);
});
updateCommand.SetAction(async (ParseResult parse, CancellationToken token) =>
{
    var command = new CLP.CLI.UpdateCommand();
    await command.UpdateDatabase();
});

/// <summary>
/// Add the commands to the root command.
/// </summary>
rootCommand.Subcommands.Add(listCommand);
rootCommand.Subcommands.Add(installCommand);
rootCommand.Subcommands.Add(uninstallCommand);
rootCommand.Subcommands.Add(updateCommand);

return await rootCommand.Parse(args).InvokeAsync();