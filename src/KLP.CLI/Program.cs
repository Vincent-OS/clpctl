using System.CommandLine;

var rootCommand = new RootCommand("klpctl -  CLI Interface for Kernel LivePatch ");
var installCommand = new Command("install", "Install a KLP patch.")
{
    new Argument<string>("patch", "The patch to install (in .klp format).")
}
installCommand.SetHandler((string file) =>
{
    InstallPatch(file);
}, installCommand.Arguments[0]);
var uninstallCommand = new Command("uninstall", "Uninstall a KLP patch.")
{
    new Argument<string>("patch", "The patch to uninstall.")
} 
uninstallCommand.SetHandler((string patch) =>
{
    UninstallPatch(patch);
}, uninstallCommand.Arguments[0]);

rootCommand.AddCommand(new Command("list", "List all installed KLP patches."));
rootCommand.AddCommand(installCommand);
rootCommand.AddCommand(uninstallCommand);
rootCommand.AddCommand(new Command("update", "Update the KLP database and apply new patches."));

return await rootCommand.InvokeAsync(args);