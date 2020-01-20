using System;

namespace DTO
{
    public class AddChatDto
    {
        public int ChatId { get; set; } = default!;

        public string UserName { get; set; } = default!;
    }
}