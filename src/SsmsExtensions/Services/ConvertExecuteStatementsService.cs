using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SsmsExtensions.Services;

internal static class ConvertExecuteStatementsService
{
    public static (IList<ParseError> Errors, bool IsModified, string Text) Execute(string text)
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
                parametersStrings.AddRange(ParseParameters(((Literal)parameters[1].ParameterValue).Value));
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

    private static IEnumerable<string> ParseParameters(string parametersLiteral)
    {
        using var stringReader = new StringReader(parametersLiteral);
        var sqlParser = new TSql160Parser(true, SqlEngineType.All);
        var sqlFragment = sqlParser.Parse(stringReader, out var errors);
        var state = ParseParametersState.Search;
        var parameter = string.Empty;

        for (var i = sqlFragment.FirstTokenIndex; i <= sqlFragment.LastTokenIndex; i++)
        {
            var token = sqlFragment.ScriptTokenStream[i];
            switch (token.TokenType)
            {
                case TSqlTokenType.Variable:
                    switch (state)
                    {
                        case ParseParametersState.Search:
                            state = ParseParametersState.Variable;
                            parameter = token.Text;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case TSqlTokenType.Comma:
                    switch (state)
                    {
                        case ParseParametersState.Search:
                            break;
                        case ParseParametersState.Variable:
                            state = ParseParametersState.Search;
                            yield return parameter;
                            break;
                        case ParseParametersState.VariableArguments:
                            parameter += token.Text;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case TSqlTokenType.LeftParenthesis:
                    switch (state)
                    {
                        case ParseParametersState.Variable:
                            state = ParseParametersState.VariableArguments;
                            parameter += token.Text;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case TSqlTokenType.RightParenthesis:
                    switch (state)
                    {
                        case ParseParametersState.VariableArguments:
                            state = ParseParametersState.Search;
                            parameter += token.Text;
                            yield return parameter;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case TSqlTokenType.EndOfFile:
                    if (state != ParseParametersState.Search)
                    {
                        yield return parameter;
                    }
                    break;
                default:
                    if (state != ParseParametersState.Search)
                    {
                        parameter += token.Text;
                    }
                    break;
            }
        }
    }

    private enum ParseParametersState
    {
        Search,
        Variable,
        VariableArguments
    }
}