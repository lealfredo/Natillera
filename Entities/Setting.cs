using SQLite;

namespace Natillera.Entities
{
    public class Setting
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string WhatsAppNumber { get; set; }
        public string ContactName { get; set; }
    }
}
