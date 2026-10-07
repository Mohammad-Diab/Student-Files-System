# Student Files System

A small system that keeps **students and their files**: an ASP.NET Web API stores the students and the files uploaded for each one, and an ASP.NET MVC site lists the students, shows a student with their files, and lets you add, edit, upload, download and delete.

A training project for my first job, written in August 2018 right after the interview. The code is kept as it was then (names such as `studant`, `Studants` and `bairthday` included).

| Folder | What it is |
|---|---|
| `studant api` | ASP.NET Web API 2 on .NET Framework 4.5. Students and files in SQL Server (ADO.NET), the file contents on disk under `App_Data\Files`. Help page at `/Help`. |
| `studant client` | ASP.NET MVC 5 on .NET Framework 4.5. Calls the API from the server with `HttpClient`. |
| `schema.sql` | Creates the `students` database with four made-up demo students. |

## The API

All routes are under `/api/values/`:

| Verb | Route | What it does |
|---|---|---|
| GET | `status` | Number of students and files |
| GET | `students`, `students/page{n}` | Students, two per page |
| GET | `student/{id}` | A student and their files |
| POST | `add student`, `edit student/{id}` | Add or edit a student |
| DELETE | `delete student/{id}` | Delete a student |
| POST | `add file` | Upload a file for a student (multipart) |
| GET | `download/{file_id}` | Download a file |
| DELETE | `delete file/{file_id}` | Delete a file |

## Run the release

The release runs on Windows 10/11 with the .NET Framework that comes with Windows.

1. Install **SQL Server Express LocalDB**: run the [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) installer, choose **Download Media**, then **LocalDB**, and run `SqlLocalDB.msi`.
2. Install **IIS Express** if you don't have Visual Studio: [IIS Express 10](https://www.microsoft.com/download/details.aspx?id=48264).
3. Extract `Student-Files-System-v1.0.0-win-x64.7z` from the [Releases](https://github.com/Mohammad-Diab/Student-Files-System/releases) page with [7-Zip](https://www.7-zip.org).
4. In the extracted folder, run `Setup-Database.cmd`. It creates the `students` database on LocalDB.
5. Run `Start.cmd`. It starts the API on `http://localhost:51550` and the site on `http://localhost:64952`, and opens the site.

The client looks for the API at `http://localhost:51550`, so keep that port.

## Run from source

1. Install SQL Server Express LocalDB and run `sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i schema.sql`. The API connects to `(localdb)\MSSQLLocalDB` with your Windows account (the connection string is in `ValuesController.cs`).
2. Open `studant api\studant api.sln` and `studant client\studant client.sln` in Visual Studio, restore the NuGet packages, and run the API first (port 51550), then the client (port 64952).

The projects target .NET Framework 4.5. Current Visual Studio versions don't install the 4.5 targeting pack; instead of retargeting, build with the reference-assemblies package:

```
nuget install Microsoft.NETFramework.ReferenceAssemblies.net45 -OutputDirectory refasm
msbuild "studant api\studant api.sln" -t:restore -p:RestorePackagesConfig=true
msbuild "studant api\studant api\studant api.csproj" -p:TargetFrameworkRootPath=<full path to refasm>\Microsoft.NETFramework.ReferenceAssemblies.net45.1.0.3\build
```

and the same for `studant client`. If the restore fails with "Could not find a part of the path", the folder path is too long; build from a shorter path.

## Known limits

- The SQL is built with `string.Format`, so a `'` in any field breaks the query (and it is open to SQL injection). Dates follow the server's culture.
- The API returns no errors to the client: a failed add or edit (for example, an empty field) just does nothing.
- A downloaded file is always named `file name here.dat`; the API returns only the bytes.
- Deleting a student leaves their files.
- There are no error pages when the API is not running.

## History

The first commit is the 2018 code. The commits after it add `.gitignore`, the database script, and this README and license.

## License

[MIT](LICENSE)
