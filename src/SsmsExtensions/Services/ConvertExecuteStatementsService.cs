using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SsmsExtensions.Services;

internal static class ConvertExecuteStatementsService
{
	public static (IList<ParseError> Errors, bool IsModified, string OutputText) Execute(string text)
	{
		using var reader = new StringReader(text);
		var parser = new TSql160Parser(true, SqlEngineType.All);
		var script = (TSqlScript)parser.Parse(reader, out var errors);

		if (errors.Any())
		{
			return (errors, false, text);
		}

		static bool IsSpExecuteSql(TSqlFragment executeStatement)
		{
			for (var i = executeStatement.FirstTokenIndex; i <= executeStatement.LastTokenIndex; i++)
			{
				if (string.Compare(executeStatement.ScriptTokenStream[i].Text, "sp_executesql", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return true;
				}
			}

			return false;
		}

		var executeStatements = script.Batches.SelectMany(x => x.Statements).Where(x => x is ExecuteStatement executeStatement && IsSpExecuteSql(executeStatement)).Cast<ExecuteStatement>().ToList();

		if (!executeStatements.Any())
		{
			return (Array.Empty<ParseError>(), false, text);
		}

		executeStatements.Reverse();

		foreach (var executeStatement in executeStatements)
		{
			var sb = new StringBuilder();
			var parameters = executeStatement.ExecuteSpecification.ExecutableEntity.Parameters;
			var statementString = ((Literal)parameters[0].ParameterValue).Value.Trim();
			var parametersStrings = new List<string>();

			if (parameters.Count > 1)
			{
				parametersStrings.AddRange(((Literal)parameters[1].ParameterValue).Value.Split(',').Select(x => x.Trim()));
			}

			var parameterValueStrings = new List<string>();
			if (parameters.Count > 2)
			{
				for (var i = 2; i < parameters.Count; i++)
				{
					switch (parameters[i].ParameterValue)
					{
						case StringLiteral stringLiteral:
							parameterValueStrings.Add($"'{stringLiteral.Value.Replace("'", "''")}'");
							break;
						case Literal literal:
							parameterValueStrings.Add(literal.Value);
							break;
						default:
							throw new InvalidOperationException($"Unsupported parameter value: {parameters[i].ParameterValue.GetType()}");
					}
				}
			}

			for (var i = 0; i < parametersStrings.Count; i++)
			{
				sb.Append($"declare {parametersStrings[i]}");
				if (i < parameterValueStrings.Count)
				{
					sb.Append($" = {parameterValueStrings[i]}");
				}
				sb.AppendLine();
			}
			sb.AppendLine(statementString);

			text = text.Remove(executeStatement.StartOffset, executeStatement.FragmentLength);
			text = text.Insert(executeStatement.StartOffset, sb.ToString());
		}

		return (Array.Empty<ParseError>(), true, text);
	}
}