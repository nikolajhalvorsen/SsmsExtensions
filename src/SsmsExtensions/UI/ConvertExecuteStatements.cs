using System;
using System.Linq;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SsmsExtensions.Services;

#pragma warning disable VSTHRD002

namespace SsmsExtensions.UI;

internal static class ConvertExecuteStatements
{
	public static void Execute(DTE2 dte)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		try
		{
			var text = dte.GetSelectionOrDocumentText(out var isSelection);

			if (string.IsNullOrWhiteSpace(text))
			{
				dte.OutputString($"The {(isSelection ? "selection" : "document")} is empty.");

				return;
			}

			var (errors, modified, modifiedText) = ConvertExecuteStatementsService.Execute(text);

			if (errors.Any())
			{
				dte.OutputString($"The {(isSelection ? "selection" : "document")} contains errors.");

				return;
			}

			if (modified)
			{
				dte.SetActiveDocumentSelectionOrText(modifiedText, out _);
				dte.OutputString($"The {(isSelection ? "selection" : "document")} was modified.");
			}
			else
			{
				dte.OutputString($"The {(isSelection ? "selection" : "document")} was not modified.");
			}
		}
		catch (NoActiveDocumentException)
		{
			dte.OutputString("An active document is required.");
		}
		catch (Exception ex)
		{
			dte.OutputString($"Unhandled exception: {ex.Message}");
		}
	}
}