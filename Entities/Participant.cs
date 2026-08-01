using SQLite;
using System.ComponentModel.DataAnnotations.Schema;

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

        public decimal MonthlyContribution { get; set; }
    }
}
