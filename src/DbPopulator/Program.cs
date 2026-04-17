using DbPopulator;
using Microsoft.EntityFrameworkCore;
using SiGen.Data.Entities;

var dbPath = SiGenDatabasePath.GetDatabaseFilePath();
Console.WriteLine($"Database path: {dbPath}");

var options = new DbContextOptionsBuilder<SiGenDbContext>()
    .UseSqlite(SiGenDatabasePath.GetConnectionString())
    .Options;

using var db = new SiGenDbContext(options);
db.Database.EnsureCreated();

var xmlPath = "StringDB.xml";

if (args.Any(a => string.Equals(a, "--export", StringComparison.OrdinalIgnoreCase)))
{
    var xmlDbWriter = new XmlDbWriter(db);
    await xmlDbWriter.Save(xmlPath);
    Console.WriteLine($"Exported database to '{xmlPath}'.");
}
else
{
    var reader = new XmlDbReader(db);
    var result = await reader.ImportAsync(xmlPath);
    Console.WriteLine($"Import complete. Added specs: {result.AddedSpecs}, Updated specs: {result.UpdatedSpecs}, Added sets: {result.AddedSets}, Updated sets: {result.UpdatedSets}, Added set items: {result.AddedSetItems}, Updated set items: {result.UpdatedSetItems}.");
}

