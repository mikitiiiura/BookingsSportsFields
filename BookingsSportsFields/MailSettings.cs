namespace BookingsSportsFields
{
    //public class MailSettings
    //{
    //    public string Server { get; set; }
    //    public int Port { get; set; }
    //    public string SenderName { get; set; }
    //    public string SenderEmail { get; set; }
    //    public string UserName { get; set; }
    //    public string Password { get; set; }
    //}
    //public class MailSettings
    //{
    //    public string EmailId { get; set; }
    //    public string Name { get; set; }
    //    public string UserName { get; set; }
    //    public string Password { get; set; }
    //    public string Host { get; set; }
    //    public int Port { get; set; }
    //    public bool UseSSL { get; set; }
    //}
    public class MailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string EmailId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Name { get; set; } // Назва відправника
    }
}
