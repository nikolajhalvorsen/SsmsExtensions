using Shouldly;
using SsmsExtensions.Services;

namespace SsmsExtensions.Tests.Services;

public class ConvertExecuteStatementsServiceTests
{
	[Fact]
	public void Error()
	{
		const string text = @"sp_executesql '";

		var (errors, isModified, outputText) = ConvertExecuteStatementsService.Execute(text);

		errors.Single().Message.ShouldBe("Expected but did not find a closing quotation mark after the character string '.");
		isModified.ShouldBeFalse();
		outputText.ShouldBe(text);
	}

	[Fact]
	public void Script()
	{
		const string text =
			@"dummy
go

SELECT 1

exec sp_executesql @Statement=N'select @Id, @Name', @params= N'@Id int, @Name nvarchar(max)', @Id = 1, @Name = 'Dum''my'

SELECT 2
go

sp_executesql @Statement=N'select @Id, @Name', @params= N'@Id int, @Name nvarchar(max)', @Id = 1, @Name = 'Dum''my'
go

SELECT 3
go

exec sp_executesql N'select @Id, @Name', N'@Id int, @Name nvarchar(max)', @Id = 1, @Name = 'Dum''my'

SELECT 4
go

sp_executesql N'select @Id, @Name', N'@Id int, @Name nvarchar(max)', @Id = 1, @Name = 'Dum''my'
go";

		var (errors, isModified, outputText) = ConvertExecuteStatementsService.Execute(text);

		errors.Count.ShouldBe(0);
		isModified.ShouldBeTrue();
		outputText.ShouldBe(@"dummy
go

SELECT 1

declare @Id int = 1
declare @Name nvarchar(max) = 'Dum''my'
select @Id, @Name


SELECT 2
go

declare @Id int = 1
declare @Name nvarchar(max) = 'Dum''my'
select @Id, @Name

go

SELECT 3
go

declare @Id int = 1
declare @Name nvarchar(max) = 'Dum''my'
select @Id, @Name


SELECT 4
go

declare @Id int = 1
declare @Name nvarchar(max) = 'Dum''my'
select @Id, @Name

go");
	}

	[Fact]
	public void Statement_dummy()
	{
		const string text = @"dummy";

		var (errors, isModified, outputText) = ConvertExecuteStatementsService.Execute(text);

		errors.Count.ShouldBe(0);
		isModified.ShouldBeFalse();
		outputText.ShouldBe(text);
	}

	[Theory]
	[InlineData("exec sp_executesql N'select @Id, @Name', N'@Id int, @Name nvarchar(max)', @Id = 1, @Name = 'Dum''my'")]
	[InlineData("sp_executesql N'select @Id, @Name', N'@Id int, @Name nvarchar(max)', @Id = 1, @Name = 'Dum''my'")]
	public void Statement_exec_sp_executesql(string text)
	{
		var (errors, isModified, outputText) = ConvertExecuteStatementsService.Execute(text);

		errors.Count.ShouldBe(0);
		isModified.ShouldBeTrue();
		outputText.ShouldBe("declare @Id int = 1\r\ndeclare @Name nvarchar(max) = 'Dum''my'\r\nselect @Id, @Name\r\n");
	}

	[Theory]
	[InlineData("exec sp_executesql @Statement=N' select 1, ''Dum''''my'''")]
	[InlineData("sp_executesql @Statement=N' select 1, ''Dum''''my'''")]
	public void Statement_exec_sp_executesql_param(string text)
	{
		var (errors, isModified, outputText) = ConvertExecuteStatementsService.Execute(text);

		errors.Count.ShouldBe(0);
		isModified.ShouldBeTrue();
		outputText.ShouldBe("select 1, 'Dum''my'\r\n");
	}

	[Theory]
	[InlineData("exec sp_executesql @Statement=N'select @Id, @Name', @params= N'@Id int, @Name nvarchar(max)', @Id = 1, @Name = 'Dum''my'")]
	[InlineData("sp_executesql @Statement=N'select @Id, @Name', @params= N'@Id int, @Name nvarchar(max)', @Id = 1, @Name = 'Dum''my'")]
	public void Statement_exec_sp_executesql_params(string text)
	{
		var (errors, isModified, outputText) = ConvertExecuteStatementsService.Execute(text);

		errors.Count.ShouldBe(0);
		isModified.ShouldBeTrue();
		outputText.ShouldBe("declare @Id int = 1\r\ndeclare @Name nvarchar(max) = 'Dum''my'\r\nselect @Id, @Name\r\n");
	}
}