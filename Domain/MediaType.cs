using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class MediaType
    {
        public int MediaTypeId { get; set; } = default!;
        
        [MinLength(2)]
        [MaxLength(32)]
        public string MediaTypeName { get; set; } = default!;
        
    }
}