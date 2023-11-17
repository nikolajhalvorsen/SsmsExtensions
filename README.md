# SsmsExtensions

Extensions to Microsoft SQL Server Management Studio

## Features

### Convert Execute Statements

Created specifically to convert parameterized queries from the SQL Server Profiler trace to plain SQL but can of course be used to convert any execute statement.

```
exec sp_executesql N'select @Id, @Name', N'@Id int, @Name nvarchar(max)', @Id = 1, @Name = 'Dummy'
```

=>

```
declare @Id int = 1
declare @Name nvarchar(max) = 'Dummy'
select @Id, @Name
```

### Format

Formats the query. It is currently not possible to configure the formatting.

- The keyword casing is lowercase.
- Comments are not preserved.

## Installation

SsmsExtension supports SSMS version 18 and 19.

Open the solution with VS2022 version 17.6.4 or later running as __administrator__.

The project is configured for SSMS version 19.

Build the project and the extension will be installed in SSMS version 19 (tested with 19.1.56.0).

Change _Debug => Start external program_ to debug with another version of SSMS.

Change _VSIX => Copy VSIX content to the following location_ to install into another version of SSMS.

## Additional Information

For information on how to create extensions for SMSS go [here](https://www.codeproject.com/Articles/1377559/How-to-Create-SQL-Server-Management-Studio-18-SSMS).