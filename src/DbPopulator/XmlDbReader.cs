using Microsoft.EntityFrameworkCore;
using SiGen.Data.Common;
using SiGen.Data.Entities;
using System.Globalization;
using System.Xml.Linq;

namespace DbPopulator
{
    internal sealed class XmlDbReader
    {
        private readonly SiGenDbContext _dbContext;

        public XmlDbReader(SiGenDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ImportResult> ImportAsync(string filePath)
        {
            var doc = XDocument.Load(filePath);
            var result = new ImportResult();

            var specsByName = await _dbContext.StringSpecs
                .ToDictionaryAsync(s => s.Name, StringComparer.OrdinalIgnoreCase);

            var specElements = doc.Root?
                .Element("StringSpecs")?
                .Elements("StringSpec")
                ?? Enumerable.Empty<XElement>();

            foreach (var element in specElements)
            {
                var name = (string?)element.Attribute("Name");
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var hasBrand = element.Attribute("Brand") != null;
                var brand = ParseBrand((string?)element.Attribute("Brand"));

                if (!TryParseMaterialType((string?)element.Attribute("MaterialType"), out var materialType))
                {
                    Console.WriteLine($"Skipping StringSpec '{name}': invalid MaterialType.");
                    continue;
                }

                if (!TryParseDouble((string?)element.Attribute("Gauge"), out var gauge))
                {
                    Console.WriteLine($"Skipping StringSpec '{name}': invalid Gauge.");
                    continue;
                }

                TryParseNullableDouble((string?)element.Attribute("CD"), out var coreDiameter);
                TryParseNullableDouble((string?)element.Attribute("UW"), out var unitWeight);

                if (specsByName.TryGetValue(name, out var existing))
                {
                    existing.MaterialType = materialType;
                    existing.Gauge = gauge;
                    existing.CoreDiameter = coreDiameter;
                    existing.UnitWeight = unitWeight;
                    if (hasBrand || string.IsNullOrWhiteSpace(existing.Brand))
                        existing.Brand = brand;

                    result.UpdatedSpecs++;
                }
                else
                {
                    var newSpec = new StringSpec
                    {
                        Name = name,
                        Brand = brand,
                        MaterialType = materialType,
                        Gauge = gauge,
                        CoreDiameter = coreDiameter,
                        UnitWeight = unitWeight
                    };

                    _dbContext.StringSpecs.Add(newSpec);
                    specsByName[name] = newSpec;
                    result.AddedSpecs++;
                }
            }

            await _dbContext.SaveChangesAsync();

            var existingSets = await _dbContext.StringSets
                .Include(s => s.Strings)
                .ToListAsync();

            var setsByKey = existingSets.ToDictionary(
                s => BuildSetKey(s.Name, s.InstrumentType, s.NumberOfStrings),
                StringComparer.OrdinalIgnoreCase);

            var setElements = doc.Root?
                .Element("StringSets")?
                .Elements("StringSet")
                ?? Enumerable.Empty<XElement>();

            foreach (var setElement in setElements)
            {
                var setName = ((string?)setElement.Attribute("Name"))?.Trim();
                var setHasBrand = setElement.Attribute("Brand") != null;
                var setBrand = ParseBrand((string?)setElement.Attribute("Brand"));
                var instrumentType = ((string?)setElement.Attribute("InstrumentType"))?.Trim();
                var numberOfStrings = ParseInt((string?)setElement.Attribute("NumberOfStrings"));

                if (string.IsNullOrWhiteSpace(setName) || string.IsNullOrWhiteSpace(instrumentType) || numberOfStrings <= 0)
                    continue;

                var key = BuildSetKey(setName, instrumentType, numberOfStrings);

                if (!setsByKey.TryGetValue(key, out var set))
                {
                    set = new StringSet
                    {
                        Name = setName,
                        Brand = setBrand,
                        InstrumentType = instrumentType,
                        NumberOfStrings = numberOfStrings
                    };

                    _dbContext.StringSets.Add(set);
                    setsByKey[key] = set;
                    result.AddedSets++;
                }
                else
                {
                    set.Name = setName;
                    set.InstrumentType = instrumentType;
                    set.NumberOfStrings = numberOfStrings;
                    if (setHasBrand || string.IsNullOrWhiteSpace(set.Brand))
                        set.Brand = setBrand;
                    result.UpdatedSets++;
                }

                var itemsByOrder = set.Strings.ToDictionary(i => i.SortOrder);

                foreach (var stringElement in setElement.Elements("String"))
                {
                    var specName = ((string?)stringElement.Attribute("Name"))?.Trim();
                    var order = ParseInt((string?)stringElement.Attribute("Order"));

                    if (string.IsNullOrWhiteSpace(specName) || order <= 0)
                        continue;

                    if (!specsByName.TryGetValue(specName, out var spec))
                    {
                        Console.WriteLine($"Skipping set item '{setName}'/{order}: StringSpec '{specName}' not found.");
                        continue;
                    }

                    if (itemsByOrder.TryGetValue(order, out var existingItem))
                    {
                        if (existingItem.StringSpecId != spec.Id)
                        {
                            existingItem.StringSpecId = spec.Id;
                            result.UpdatedSetItems++;
                        }
                    }
                    else
                    {
                        var newItem = new SetItem
                        {
                            StringSet = set,
                            String = spec,
                            SortOrder = order
                        };
                        set.Strings.Add(newItem);
                        itemsByOrder[order] = newItem;
                        result.AddedSetItems++;
                    }
                }

                set.SyncNumberOfStrings();
            }

            await _dbContext.SaveChangesAsync();
            return result;
        }

        private static string BuildSetKey(string name, string instrumentType, int numberOfStrings)
            => $"{name}|{instrumentType}|{numberOfStrings}";

        private static string ParseBrand(string? raw)
            => string.IsNullOrWhiteSpace(raw) ? "D'Addario" : raw.Trim();

        private static bool TryParseMaterialType(string? raw, out StringMaterialType value)
            => Enum.TryParse(raw, ignoreCase: true, out value);

        private static bool TryParseDouble(string? raw, out double value)
            => double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        private static bool TryParseNullableDouble(string? raw, out double? value)
        {
            if (TryParseDouble(raw, out double parsed))
            {
                value = parsed;
                return true;
            }

            value = null;
            return false;
        }

        private static int ParseInt(string? raw)
            => int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    internal sealed class ImportResult
    {
        public int AddedSpecs { get; set; }
        public int UpdatedSpecs { get; set; }
        public int AddedSets { get; set; }
        public int UpdatedSets { get; set; }
        public int AddedSetItems { get; set; }
        public int UpdatedSetItems { get; set; }
    }
}
