using System.CommandLine;

/// <summary>
/// Create the root command and subcommands.
/// Also create the necessary arguments for the specific subcommands.
/// </summary>
var rootCommand = new RootCommand("klpctl -  CLI Interface for Kernel LivePatch for Vincent OS.");
var listCommand = new Command("list", "List all installed KLP patches.");
var updateCommand = new Command("update", "Update the KLP database and apply new patches.");
var installCommand = new Command("install", "Install a KLP patch.")
{
    new Argument<string>("patch", "The patch to install (in .klp format).")
};
var uninstallCommand = new Command("uninstall", "Uninstall a KLP patch.")
{
    new Argument<string>("patch", "The patch to uninstall.")
};

/// <summary>
/// Assign the handler for commands.
/// </summary>
installCommand.SetHandler(async (string file) =>
{
    KLP.CLI.InstallCommand command = new KLP.CLI.InstallCommand();
    command.InstallPatch(file);
    await Task.CompletedTask;
}, (System.CommandLine.Binding.IValueDescriptor<string>)installCommand.Arguments[0]);
listCommand.SetHandler(async () =>
{
    KLP.CLI.ListCommand command = new KLP.CLI.ListCommand();
    command.ListInstalledPatches();
    await Task.CompletedTask;
});
uninstallCommand.SetHandler(async (string patch) =>
{
    KLP.CLI.UninstallCommand command = new KLP.CLI.UninstallCommand();
    command.UninstallPatch(patch);
    await Task.CompletedTask;
}, (System.CommandLine.Binding.IValueDescriptor<string>)uninstallCommand.Arguments[0]);
updateCommand.SetHandler(async () =>
{
    KLP.CLI.UpdateCommand command = new KLP.CLI.UpdateCommand();
    command.UpdateDatabase();
    await Task.CompletedTask;
});

/// <summary>
/// Add the commands to the root command.
/// </summary>
rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(installCommand);
rootCommand.AddCommand(uninstallCommand);
rootCommand.AddCommand(updateCommand);

return await rootCommand.InvokeAsync(args);