using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE80;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.Shell;

#pragma warning disable VSTHRD002

namespace SsmsExtensions.UI;

internal static class Excecuter
{
    public static void Execute(DTE2 dte, Func<string, (IList<ParseError> Errors, bool IsModified, string Text)> func)
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

            var (errors, modified, modifiedText) = func(text);

            if (errors != null && errors.Any())
            {
                dte.OutputString($"The {(isSelection ? "selection" : "document")} contains errors.");

                foreach (var error in errors)
                {
                    dte.OutputString(error.Message);
                }

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