using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SsmsExtensions.Services;

internal static class FormatService
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

        var generator = new Sql160ScriptGenerator(new SqlScriptGeneratorOptions
        {
            KeywordCasing = KeywordCasing.Lowercase
        });

        generator.GenerateScript(script, out var formattedText);

        formattedText = Regex.Replace(formattedText, @"\(nolock\)", "(nolock)", RegexOptions.IgnoreCase);
        formattedText = Regex.Replace(formattedText, @"count\(\*\)", "count(*)", RegexOptions.IgnoreCase);

        return (null, true, formattedText);
    }
}