using System.CommandLine;

/// <summary>
/// Create the root command and subcommands.
/// Also create the necessary arguments for the specific subcommands.
/// </summary>
var rootCommand = new RootCommand("clpctl -  CLI Interface for Core LivePatch for Vincent OS.");
var listCommand = new Command("list", "List all installed CLP patches.");
var updateCommand = new Command("update", "Update the CLP database and apply new patches.");
var createCommand = new Command("create", "Create a new CLP patch")
{
    new Argument<string>("path", "The path to the directory to create the patch from."),
    new Argument<string>("name", "The name file for the patch.")
};
var installCommand = new Command("install", "Install a CLP patch.")
{
    new Argument<string>("patch", "The patch to install (in .clp format).")
};
var uninstallCommand = new Command("uninstall", "Uninstall a CLP patch.")
{
    new Argument<string>("patch", "The patch to uninstall.")
};

/// <summary>
/// Assign the handler for commands.
/// </summary>
createCommand.SetHandler(async (string path, string name) =>
{
    CLP.CLI.CreateCommand command = new CLP.CLI.CreateCommand();
    command.CreatePatch(path, name);
    await Task.CompletedTask;
}, (System.CommandLine.Binding.IValueDescriptor<string>)createCommand.Arguments[0], (System.CommandLine.Binding.IValueDescriptor<string>)createCommand.Arguments[1]);
installCommand.SetHandler(async (string file) =>
{
    CLP.CLI.InstallCommand command = new CLP.CLI.InstallCommand();
    command.InstallPatch(file);
    await Task.CompletedTask;
}, (System.CommandLine.Binding.IValueDescriptor<string>)installCommand.Arguments[0]);
listCommand.SetHandler(async () =>
{
    CLP.CLI.ListCommand command = new CLP.CLI.ListCommand();
    command.ListInstalledPatches();
    await Task.CompletedTask;
});
uninstallCommand.SetHandler(async (string patch) =>
{
    CLP.CLI.UninstallCommand command = new CLP.CLI.UninstallCommand();
    command.UninstallPatch(patch);
    await Task.CompletedTask;
}, (System.CommandLine.Binding.IValueDescriptor<string>)uninstallCommand.Arguments[0]);
updateCommand.SetHandler(async () =>
{
    CLP.CLI.UpdateCommand command = new CLP.CLI.UpdateCommand();
    command.UpdateDatabase();
    await Task.CompletedTask;
});

/// <summary>
/// Add the commands to the root command.
/// </summary>
rootCommand.AddCommand(createCommand);
rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(installCommand);
rootCommand.AddCommand(uninstallCommand);
rootCommand.AddCommand(updateCommand);

return await rootCommand.InvokeAsync(args);