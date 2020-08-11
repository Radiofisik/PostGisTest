using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NetTopologySuite.Geometries;

namespace EFTest.Entities
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "geometry (point)")]
        public Point Location { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
