namespace EduQuest.API.Models.Entities
{
    public class Province
    {
        public int ProvinceID { get; set; }

        public required string ProvinceName { get; set; }

        public ICollection<School> Schools { get; set; }
            = new List<School>();
    }
}