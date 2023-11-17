using Shouldly;
using SsmsExtensions.Services;

namespace SsmsExtensions.Tests.Services;

public class FormatServiceTests
{
    [Fact]
    public void Error()
    {
        const string text = @"select";

        var (errors, isModified, outputText) = ConvertExecuteStatementsService.Execute(text);

        errors.Single().Message.ShouldBe("Incorrect syntax near select.");
        isModified.ShouldBeFalse();
        outputText.ShouldBe(text);
    }

    [Fact]
    public void Script()
    {
        const string text = @"SELECT A.Id, COUNT(*) FROM (SELECT B.Id, B.Name FROM [Table] B (NOLOCK)) A WHERE A.Id = 0 AND B.Name = 'dummy' GROUP BY A.Id ORDER BY A.Id";

        var (errors, isModified, outputText) = FormatService.Execute(text);

        errors.ShouldBe(null);
        isModified.ShouldBe(true);
        outputText.ShouldBe(@"select   A.Id,
         count(*)
from     (select B.Id,
                 B.Name
          from   [Table] as B with (nolock)) as A
where    A.Id = 0
         and B.Name = 'dummy'
group by A.Id
order by A.Id;

");
    }
}