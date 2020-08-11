using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using EFTest.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace EFTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var seattle = new Point(-122.333056, 47.609722) { SRID = 4326 };
            var redmond = new Point(-122.123889, 47.669444) { SRID = 4326 };

        //    var distance = seattle.ProjectTo(2855).Distance(redmond.ProjectTo(2855));

            //seed data
            // await AddData();
            var context = new BloggingContext();
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var work = geometryFactory.CreatePoint(new Coordinate(20.4976303, 54.718202));
            var cities = context.Cities.Where(c => c.Location.Distance(work) < 10/111.0).ToList();

            var pionerskiy = geometryFactory.CreatePoint(new Coordinate(20.2377613, 54.9487813));
            var dst = pionerskiy.Distance(geometryFactory.CreatePoint(new Coordinate(20.4640505, 54.7406165)));
        }

        private static async Task AddData()
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var company = new Company()
            {
                Cities = new List<City>()
                {
                    new City() {Name = "Kaliningrad", Location = geometryFactory.CreatePoint(new Coordinate(20.4640505, 54.7406165))},
                    new City() {Name = "Pionerskiy", Location = geometryFactory.CreatePoint(new Coordinate(20.2377613, 54.9487813))}
                }
            };

            var context = new BloggingContext();
            await context.Database.MigrateAsync();

            context.Companies.Add(company);
            context.SaveChanges();
        }
    }
}
