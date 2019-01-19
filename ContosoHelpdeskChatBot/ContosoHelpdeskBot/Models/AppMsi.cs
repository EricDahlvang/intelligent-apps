namespace ContosoHelpdeskChatBot.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("AppMsi")]
    public partial class AppMsi
    {
        [Key]
        public int Id { get; set; }

        public string AppName { get; set; }

        public string MsiPackage { get; set; }
    }
}
