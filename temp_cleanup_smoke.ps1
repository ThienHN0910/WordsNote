$ErrorActionPreference = 'Stop'

$projectPath = 'e:/workspace/srcPrj/WordsNote/src/backend/FeatureFusion/FeatureFusion.csproj'
$secrets = dotnet user-secrets list --project $projectPath
$connLine = $secrets | Where-Object { $_ -like 'MongoDB:ConnectionString*' } | Select-Object -First 1
$dbLine = $secrets | Where-Object { $_ -like 'MongoDB:DatabaseName*' } | Select-Object -First 1
if (-not $connLine -or -not $dbLine) { throw 'MongoDB secrets not found.' }
$conn = ($connLine -split '=', 2)[1].Trim()
$dbName = ($dbLine -split '=', 2)[1].Trim()

$tempRoot = 'e:/workspace/srcPrj/WordsNote/.tmp'
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null
$tempDir = Join-Path $tempRoot ('wordsnote-smoke-clean-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempDir | Out-Null

$csproj = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MongoDB.Driver" Version="2.30.0" />
  </ItemGroup>
</Project>
'@
Set-Content -Path (Join-Path $tempDir 'Cleanup.csproj') -Value $csproj -Encoding UTF8

$program = @'
using MongoDB.Bson;
using MongoDB.Driver;

var connectionString = args[0];
var databaseName = args[1];
var client = new MongoClient(connectionString);
var database = client.GetDatabase(databaseName);

var desks = database.GetCollection<BsonDocument>("wordsnote_desks");
var cards = database.GetCollection<BsonDocument>("wordsnote_cards");

var smokeTitleFilter = Builders<BsonDocument>.Filter.Regex("Title", new BsonRegularExpression("smoke", "i"));
var deskDocs = desks.Find(smokeTitleFilter).ToList();

if (deskDocs.Count == 0)
{
    Console.WriteLine("No smoke collections found.");
    return;
}

var deskIds = deskDocs
    .Select(d =>
    {
        if (d.Contains("Id")) return d["Id"].ToString();
        if (d.Contains("_id")) return d["_id"].ToString();
        return string.Empty;
    })
    .Where(id => !string.IsNullOrWhiteSpace(id))
    .Distinct()
    .ToList();

var deletedCards = 0L;
if (deskIds.Count > 0)
{
    var cardFilter = Builders<BsonDocument>.Filter.Or(
        Builders<BsonDocument>.Filter.In("DeskId", deskIds),
        Builders<BsonDocument>.Filter.In("deskId", deskIds),
        Builders<BsonDocument>.Filter.In("CollectionId", deskIds),
        Builders<BsonDocument>.Filter.In("collectionId", deskIds)
    );
    deletedCards = cards.DeleteMany(cardFilter).DeletedCount;
}

var deletedDesks = desks.DeleteMany(smokeTitleFilter).DeletedCount;

Console.WriteLine($"Deleted smoke collections: {deletedDesks}");
Console.WriteLine($"Deleted related cards: {deletedCards}");
Console.WriteLine("Collection titles removed:");
foreach (var d in deskDocs)
{
    var title = d.GetValue("Title", "(no-title)").ToString();
    var id = d.Contains("Id") ? d.GetValue("Id").ToString() : d.GetValue("_id", "(no-id)").ToString();
    Console.WriteLine($"- {title} ({id})");
}
'@
Set-Content -Path (Join-Path $tempDir 'Program.cs') -Value $program -Encoding UTF8

Push-Location $tempDir
dotnet run --project Cleanup.csproj -- "$conn" "$dbName"
$exitCode = $LASTEXITCODE
Pop-Location

Remove-Item -Path $tempDir -Recurse -Force
exit $exitCode
