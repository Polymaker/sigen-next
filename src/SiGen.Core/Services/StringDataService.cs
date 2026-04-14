using Microsoft.EntityFrameworkCore;
using SiGen.Data.Common;
using SiGen.Data.Entities;
using SiGen.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Services
{
    public interface IStringDataService
    {
        Task<List<StringSet>> GetAvailableStringSetsAsync(int numberOfStrings, InstrumentType? instrumentType);

        Task<StringSpec?> FindClosestStringSpec(double gauge, StringMaterialType? materialType, InstrumentType? instrumentType);

        Task<double?> InterpolateUnitWeight(double gauge, StringMaterialType? materialType);
    }

    public class MockStringDataService : IStringDataService
    {
        public Task<StringSpec?> FindClosestStringSpec(double gauge, StringMaterialType? materialType, InstrumentType? instrumentType)
        {
            return Task.FromResult<StringSpec?>(null);
        }

        public Task<List<StringSet>> GetAvailableStringSetsAsync(int numberOfStrings, InstrumentType? instrumentType)
        {
            var sets = new List<StringSet>
            {
                new StringSet
                {
                    Id = 1,
                    Name = "Light",
                    Brand = "Mock Brand",
                    InstrumentType = "ElectricGuitar",
                    NumberOfStrings = numberOfStrings,
                    Strings = new List<SetItem>()
                },
                new StringSet
                {
                    Id = 2,
                    Name = "Regular",
                    Brand = "Mock Brand",
                    InstrumentType = "AcousticGuitar",
                    NumberOfStrings = numberOfStrings,
                    Strings = new List<SetItem>()
                }
            };

            return Task.FromResult(sets);
        }

        public Task<double?> InterpolateUnitWeight(double gauge, StringMaterialType? materialType)
        {
            return Task.FromResult<double?>(null);
        }
    }

    public class StringDataService : IStringDataService
    {
        #region String Sets

        public async Task<List<StringSet>> GetAvailableStringSetsAsync(int numberOfStrings, InstrumentType? instrumentType)
        {
            // 1. Fetch from System (Read-Only)
            using var systemDb = new SiGenDbContext();

            var query = systemDb.StringSets
                .AsNoTracking() // Performance boost for read-only
                .Include(s => s.Strings).ThenInclude(i => i.String)
                .Where(x => x.NumberOfStrings == numberOfStrings);

            if (instrumentType.HasValue)
            {
                query = query.Where(x => x.InstrumentType == instrumentType.ToString());
                //if (instrumentType.Value.ToString().Contains("Bass"))
                //{
                //    query = query.Where(x => x.InstrumentType == "Bass");
                //}
                //else if (instrumentType.Value.ToString().Contains("Guitar"))
                //{
                //    query = query.Where(x => x.InstrumentType == "Guitar");
                //}
                //else
                //{
                //    query = query.Where(x => x.InstrumentType == instrumentType.ToString());
                //}
            }

            var systemSets = await query
                .ToListAsync();

            return systemSets.OrderBy(s => s.Name).ToList();
            //// 2. Fetch from User (Read-Write)
            //using var userDb = CreateUserContext();
            //var userSets = await userDb.StringSets
            //    .AsNoTracking()
            //    .Include(s => s.Strings)
            //        .ThenInclude(i => i.String)
            //    .ToListAsync();

            //// 3. Merge and return
            //return systemSets.Concat(userSets)
            //    .OrderBy(s => s.Name)
            //    .ToList();
        }

        #endregion

        public async Task<StringSpec?> FindClosestStringSpec(double gauge, StringMaterialType? materialType, InstrumentType? instrumentType)
        {
            using var systemDb = new SiGenDbContext();
            var query = systemDb.StringSpecs
                .AsNoTracking();
            if (materialType.HasValue)
                query = query.Where(s => s.MaterialType == materialType.Value);
            //if (instrumentType.ToString().Contains("Bass"))
            //{
            //    query = query.Where(s => s.Name.Contains("Bass"));
            //}
            //else if (instrumentType.ToString().Contains("Guitar"))
            //{
            //    query = query.Where(s => s.Name.Contains("Guitar"));
            //}
            //else
            //{
            //    query = query.Where(s => s.Name.Contains(instrumentType.ToString()));
            //}
            var specs = await query.ToListAsync();
            return specs
                .OrderBy(s => Math.Abs(s.Gauge - gauge))
                .FirstOrDefault();
        }

        public async Task<double?> InterpolateUnitWeight(double gauge, StringMaterialType? materialType)
        {
            using var systemDb = new SiGenDbContext();
            var query = systemDb.StringSpecs
                .AsNoTracking();

            if (materialType.HasValue)
                query = query.Where(s => s.MaterialType == materialType.Value);


            var closests = await query
                .OrderBy(s => Math.Abs(s.Gauge - gauge))
                .Take(2)
                .ToListAsync();

            if (closests.Any(x=>x.Gauge == gauge && x.UnitWeight.HasValue))
            {
                return closests.First(x => x.Gauge == gauge).UnitWeight;
            }
            else if (closests.Count == 2 && closests.All(x => x.UnitWeight.HasValue))
            {
                var a = closests[0];
                var b = closests[1];

                // avoid division by zero if both gauges are somehow identical
                if (Math.Abs(a.Gauge - b.Gauge) < double.Epsilon)
                    return a.UnitWeight;

                double t = (gauge - a.Gauge) / (b.Gauge - a.Gauge);
                return a.UnitWeight!.Value + t * (b.UnitWeight!.Value - a.UnitWeight.Value);
            }
            else if (closests.Count == 1)
            {
                return closests[0].UnitWeight;
            }

            return null;
        }
    }
}
