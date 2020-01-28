using System.ComponentModel.DataAnnotations;

namespace DAL
{
    public class MessageMedia
    {
        public int MessageMediaId { get; set; } = default!;

        public int MessageId { get; set; } = default!;

        public int MediaTypeId { get; set; } = default!;

        public byte[] File { get; set; } = default!;
        
        [MinLength(3)]
        [MaxLength(64)]
        public string FileName { get; set; } = default!;
    }
}