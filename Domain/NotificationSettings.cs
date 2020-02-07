namespace DAL
{
    public class NotificationSettings
    {
        public int NotificationSettingsId { get; set; } = default!;

        public bool? Sound { get; set; }
        public bool? NewChatReceived { get; set; } 
        public bool? NewMessageReceived { get; set; } 
        public bool? ConnectionChanged { get; set; } 

        public NotificationSettings()
        {
            NewChatReceived = true;
            NewMessageReceived = true;
            ConnectionChanged = true;
            Sound = true;
        }
    }
}