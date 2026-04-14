using Microsoft.EntityFrameworkCore;
using SiGen.Data.Common;
using SiGen.Data.Entities;
using System.Xml.Linq;

var options = new DbContextOptionsBuilder<SiGenDbContext>()
    .UseSqlite("Data Source=SiGenDatabase.db")
    .Options;

using var db = new SiGenDbContext(options);
db.Database.EnsureCreated(); // Creates DB and tables if they don't exist

// 2. Load XML
var doc = XDocument.Load("StringDB.xml");

// 3. Parse All Individual Strings into a Dictionary for quick lookup
Console.WriteLine("Parsing String Library...");
var stringLibrary = doc.Descendants("StringData")
    .Select(x => new StringSpec
    {
        Name = x.Attribute("ID")?.Value ?? "",
        Brand = "D'Addario",
        // Your logic for Material Enum:
        MaterialType = Enum.Parse<StringMaterialType>(x.Attribute("Material")?.Value ?? "SteelPlain"),
        Gauge = double.Parse(x.Attribute("OD")?.Value ?? "0"),
        //CoreDiameter = double.Parse(x.Attribute("CD")?.Value ?? "0"),
        UnitWeight = double.Parse(x.Attribute("UW")?.Value ?? "0"),
    })
    .ToDictionary(s => s.Name);

foreach (var str in stringLibrary.Values)
{
    db.StringSpecs.Add(str);

    if (str.MaterialType.ToString().Contains("Plain", StringComparison.InvariantCultureIgnoreCase))
        str.CoreDiameter = str.Gauge;
    else if (str.MaterialType.ToString().Contains("Wound", StringComparison.InvariantCultureIgnoreCase))
    {
        if (str.MaterialType == StringMaterialType.NickelWound)
            str.CoreDiameter = str.Gauge * 0.38;
        else if (str.MaterialType == StringMaterialType.BronzeWound)
            str.CoreDiameter = str.Gauge * 0.34;
        else
            str.CoreDiameter = str.Gauge * 0.35;
    }

}

// 4. Parse String Sets
Console.WriteLine("Parsing Sets and linking strings...");
var setsElements = doc.Descendants("StringSet");

foreach (var setXml in setsElements)
{
    var set = new StringSet
    {
        Name = setXml.Attribute("Name")?.Value ?? "Unknown Set",
        Brand = "D'Addario", // Hardcoded per your request
        NumberOfStrings = int.Parse(setXml.Attribute("StringCount")?.Value ?? "0"),
        // Map the Instrument string to your Flags Enum
        InstrumentType = setXml.Attribute("Instrument")?.Value ?? string.Empty
    };

    
    var stringElements = setXml.Elements("String");
    int stringIndex = stringElements.Count(); //strings are from treble to bass but we want bass to treble (bass is index 0)
    // Link the strings from our library
    foreach (var sXml in stringElements)
    {
        var id = sXml.Attribute("ID")?.Value;
        if (id != null && stringLibrary.TryGetValue(id, out var spec))
        {
            set.Strings.Add(new SetItem
            {
                String = spec,
                SortOrder = stringIndex--,
                //TargetNote = sXml.Attribute("Note")?.Value
            });
        }
        else
        {
            Console.WriteLine($"Warning: String ID '{id}' not found in library for set '{set.Name}'.");
        }
    }

    db.StringSets.Add(set);
}

await db.SaveChangesAsync();
Console.WriteLine("Migration Complete! SiGenDatabase.db is ready.");