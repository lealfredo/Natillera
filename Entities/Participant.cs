using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Entities
{
    public class Participant
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string Name { get; set; }

        public string Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
