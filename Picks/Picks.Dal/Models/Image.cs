using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Picks.Dal.Models
{
    public class Image
    {
        public int Id { get; set; }

        public string Tags { get; set; }

        public string FileName { get; set; }

        public DateTime Order { get; set; } // Order by date
    }
}
