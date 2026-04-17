using Microsoft.EntityFrameworkCore;
using SiGen.Data.Common;
using SiGen.Data.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SiGen.Services
{
    public interface IDatabaseInitializationService
    {
        Task InitializeAsync(bool mergeXml);
    }

    public sealed class DatabaseInitializationService : IDatabaseInitializationService
    {
        private static readonly SemaphoreSlim InitLock = new(1, 1);

        public async Task InitializeAsync(bool mergeXml)
        {
            await InitLock.WaitAsync();
            try
            {
                using var db = new SiGenDbContext();
                await db.Database.MigrateAsync();
                //var hasMigrations = db.Database.GetMigrations().Any();
                //if (hasMigrations)
                //    await db.Database.MigrateAsync();
                //else
                //    await db.Database.EnsureCreatedAsync();

                if (!mergeXml) return;
                var xmlPath = Path.Combine(AppContext.BaseDirectory, "StringDB.xml");
                if (!File.Exists(xmlPath))
                    return;

                await MergeFromXmlAsync(db, xmlPath);
            }
            finally
            {
                InitLock.Release();
            }
        }

        private static async Task MergeFromXmlAsync(SiGenDbContext db, string filePath)
        {
            var doc = XDocument.Load(filePath);

            var specsByName = await db.StringSpecs
                .ToDictionaryAsync(s => s.Name, StringComparer.OrdinalIgnoreCase);

            var specElements = doc.Root?
                .Element("StringSpecs")?
                .Elements("StringSpec")
                ?? Enumerable.Empty<XElement>();

            foreach (var element in specElements)
            {
                var name = ((string?)element.Attribute("Name"))?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var hasBrand = element.Attribute("Brand") != null;
                var brand = ParseBrand((string?)element.Attribute("Brand"));

                if (!Enum.TryParse((string?)element.Attribute("MaterialType"), true, out StringMaterialType materialType))
                    continue;

                if (!TryParseDouble((string?)element.Attribute("Gauge"), out var gauge))
                    continue;

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

                    db.StringSpecs.Add(newSpec);
                    specsByName[name] = newSpec;
                }
            }

            await db.SaveChangesAsync();

            var existingSets = await db.StringSets
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
                    db.StringSets.Add(set);
                    setsByKey[key] = set;
                }
                else
                {
                    set.Name = setName;
                    set.InstrumentType = instrumentType;
                    set.NumberOfStrings = numberOfStrings;
                    if (setHasBrand || string.IsNullOrWhiteSpace(set.Brand))
                        set.Brand = setBrand;
                }

                var itemsByOrder = set.Strings.ToDictionary(i => i.SortOrder);

                foreach (var stringElement in setElement.Elements("String"))
                {
                    var specName = ((string?)stringElement.Attribute("Name"))?.Trim();
                    var order = ParseInt((string?)stringElement.Attribute("Order"));

                    if (string.IsNullOrWhiteSpace(specName) || order <= 0)
                        continue;

                    if (!specsByName.TryGetValue(specName, out var spec))
                        continue;

                    if (itemsByOrder.TryGetValue(order, out var existingItem))
                    {
                        if (existingItem.StringSpecId != spec.Id)
                            existingItem.StringSpecId = spec.Id;
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
                    }
                }

                set.SyncNumberOfStrings();
            }

            await db.SaveChangesAsync();
        }

        private static string ParseBrand(string? raw)
            => string.IsNullOrWhiteSpace(raw) ? "None" : raw.Trim();

        private static string BuildSetKey(string name, string instrumentType, int numberOfStrings)
            => $"{name}|{instrumentType}|{numberOfStrings}";

        private static bool TryParseDouble(string? raw, out double value)
            => double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        private static bool TryParseNullableDouble(string? raw, out double? value)
        {
            if (TryParseDouble(raw, out var parsed))
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
}
