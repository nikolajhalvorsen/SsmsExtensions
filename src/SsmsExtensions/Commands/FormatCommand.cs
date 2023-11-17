using System;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SsmsExtensions.Services;
using SsmsExtensions.UI;
using Task = System.Threading.Tasks.Task;

#pragma warning disable VSTHRD002

namespace SsmsExtensions.Commands;

internal sealed class FormatCommand
{
    private readonly AsyncPackage _package;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FormatCommand" /> class.
    ///     Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    private FormatCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

        var menuCommandId = new CommandID(new Guid("bda77397-cf79-4ba0-a073-6738a0059eb7"), 0x0200);
        var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
        commandService.AddCommand(menuItem);
    }

    public static FormatCommand Instance { get; private set; }
    private IAsyncServiceProvider ServiceProvider => _package;

    public static async Task InitializeAsync(AsyncPackage package)
    {
        // Switch to the main thread - the call to AddCommand in FormatCommand's constructor requires
        // the UI thread.
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

        var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
        Instance = new FormatCommand(package, commandService);
    }

    private void MenuItemCallback(object sender, EventArgs e)
    {
        var dte = (DTE2)ServiceProvider.GetServiceAsync(typeof(DTE)).GetAwaiter().GetResult();

        Excecuter.Execute(dte, text =>
        {
            var (_, _, formattedText) = FormatService.Execute(text);

            return FormatService.Execute(formattedText);
        });
    }
}