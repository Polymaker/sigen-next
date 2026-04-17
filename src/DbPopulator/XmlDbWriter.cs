using SiGen.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace DbPopulator
{
    internal class XmlDbWriter
    {
        public SiGenDbContext DbContext { get; }

        public XmlDbWriter(SiGenDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task Save(string filepath)
        {
            var strings = await DbContext.StringSpecs.ToListAsync();

            var xmlDbDocument = new XDocument();

            xmlDbDocument.Add(new XElement("StringsDB"));

            var stringSpecsElement = new XElement("StringSpecs");

            xmlDbDocument.Root?.Add(stringSpecsElement);

            foreach (var str in strings)
            {
                var element = new XElement("StringSpec",
                    new XAttribute("Name", str.Name),
                    new XAttribute("Brand", str.Brand),
                    new XAttribute("MaterialType", str.MaterialType.ToString()),
                    new XAttribute("Gauge", str.Gauge)
                );
                if (str.CoreDiameter.HasValue)
                    element.Add(new XAttribute("CD", str.CoreDiameter.Value));

                if (str.UnitWeight.HasValue)
                    element.Add(new XAttribute("UW", str.UnitWeight.Value));

                stringSpecsElement.Add(element);
            }

            var stringSetsElement = new XElement("StringSets");
            xmlDbDocument.Root?.Add(stringSetsElement);

            foreach (var set in await DbContext.StringSets.Include(s => s.Strings).ThenInclude(si => si.String).ToListAsync())
            {
                var setElement = new XElement("StringSet",
                    new XAttribute("Name", set.Name),
                    new XAttribute("Brand", set.Brand),
                    new XAttribute("InstrumentType", set.InstrumentType),
                    new XAttribute("NumberOfStrings", set.NumberOfStrings)
                );
                foreach (var item in set.Strings.OrderBy(s => s.SortOrder)) // Order by SortOrder to maintain bass to treble
                {
                    var stringElement = new XElement("String",
                        //new XAttribute("ID", item.StringSpecId),
                        new XAttribute("Name", item.String.Name),
                        new XAttribute("Order", item.SortOrder)
                    );
                    setElement.Add(stringElement);
                }
                stringSetsElement.Add(setElement);
            }

            xmlDbDocument.Save(filepath);
        }
    }
}
