using System;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SsmsExtensions.UI;
using Task = System.Threading.Tasks.Task;

#pragma warning disable VSTHRD002

namespace SsmsExtensions.Commands;

/// <summary>
///     Command handler
/// </summary>
internal sealed class ConvertExecuteStatementsCommand
{
	/// <summary>
	///     Command ID.
	/// </summary>
	public const int CommandId = 0x0100;

	/// <summary>
	///     Command menu group (command set GUID).
	/// </summary>
	public static readonly Guid CommandSet = new("bda77397-cf79-4ba0-a073-6738a0059eb7");

	/// <summary>
	///     VS Package that provides this command, not null.
	/// </summary>
	private readonly AsyncPackage _package;

	/// <summary>
	///     Initializes a new instance of the <see cref="ConvertExecuteStatementsCommand" /> class.
	///     Adds our command handlers for menu (commands must exist in the command table file)
	/// </summary>
	/// <param name="package">Owner package, not null.</param>
	/// <param name="commandService">Command service to add command to, not null.</param>
	private ConvertExecuteStatementsCommand(AsyncPackage package, OleMenuCommandService commandService)
	{
		_package = package ?? throw new ArgumentNullException(nameof(package));
		commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

		var menuCommandId = new CommandID(CommandSet, CommandId);
		var menuItem = new MenuCommand(Execute, menuCommandId);
		commandService.AddCommand(menuItem);
	}

	/// <summary>
	///     Gets the instance of the command.
	/// </summary>
	public static ConvertExecuteStatementsCommand Instance { get; private set; }

	/// <summary>
	///     Gets the service provider from the owner package.
	/// </summary>
	private IAsyncServiceProvider ServiceProvider => _package;

	/// <summary>
	///     Initializes the singleton instance of the command.
	/// </summary>
	/// <param name="package">Owner package, not null.</param>
	public static async Task InitializeAsync(AsyncPackage package)
	{
		// Switch to the main thread - the call to AddCommand in ConvertExecuteStatementsCommand's constructor requires
		// the UI thread.
		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

		var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
		Instance = new ConvertExecuteStatementsCommand(package, commandService);
	}

	/// <summary>
	///     This function is the callback used to execute the command when the menu item is clicked.
	///     See the constructor to see how the menu item is associated with this function using
	///     OleMenuCommandService service and MenuCommand class.
	/// </summary>
	/// <param name="sender">Event sender.</param>
	/// <param name="e">Event args.</param>
	private void Execute(object sender, EventArgs e)
	{
		var dte = (DTE2)ServiceProvider.GetServiceAsync(typeof(DTE)).GetAwaiter().GetResult();

		ConvertExecuteStatements.Execute(dte);
	}
}