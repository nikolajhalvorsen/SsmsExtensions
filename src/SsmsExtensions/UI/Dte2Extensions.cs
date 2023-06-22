using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace SsmsExtensions.UI;

internal static class Dte2Extensions
{
	public static string GetActiveDocumentText(this DTE2 dte)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		var textDocument = (TextDocument)dte.ActiveDocument.Object("TextDocument");
		var editPoint = textDocument.StartPoint.CreateEditPoint();

		return editPoint.GetText(textDocument.EndPoint);
	}

	public static string GetSelectionOrDocumentText(this DTE2 dte, out bool isSelection)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		var activeDocument = dte.ActiveDocument;

		if (activeDocument == null)
		{
			throw new NoActiveDocumentException();
		}

		var ts = (TextSelection)activeDocument.Selection;

		isSelection = ts.Text.Length > 0;

		var text = isSelection ? ts.Text : dte.GetActiveDocumentText();

		return text;
	}

	public static void OutputString(this DTE2 dte, string text)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		dte.Windows.Item(Constants.vsWindowKindOutput).Activate();

		dte.ToolWindows.OutputWindow.ActivePane.OutputString($"[{DateTime.Now:O}] SSMS Extensions: {text}");
	}

	public static void SetActiveDocumentSelectionOrText(this DTE2 dte, string text, out bool isSelection)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		var activeDocument = dte.ActiveDocument;

		if (activeDocument == null)
		{
			throw new NoActiveDocumentException();
		}

		var ts = (TextSelection)activeDocument.Selection;

		isSelection = ts.Text.Length > 0;

		if (isSelection)
		{
			ts.Text = text;
		}
		else
		{
			dte.SetActiveDocumentText(text);
		}
	}

	public static void SetActiveDocumentText(this DTE2 dte, string text)
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		var textDocument = (TextDocument)dte.ActiveDocument.Object("TextDocument");
		var editPoint = textDocument.StartPoint.CreateEditPoint();

		editPoint.ReplaceText(textDocument.EndPoint, text, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
	}
}