using System;
using System.Collections.Generic;
using System.Text;

namespace EFTest.Entities
{
    public class Company
    {
        public int Id { get; set; }
      
        public List<City> Cities { get; set; }
    }
}
