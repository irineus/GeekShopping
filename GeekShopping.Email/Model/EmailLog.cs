using GeekShopping.Email.Model.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeekShopping.Email.Model
{
    [Table("email_logs")]
    public class EmailLog : BaseEntity
    {
        [Column("from")]
        public string From { get; set; }

        [Column("to")]
        public string To { get; set; }

        [Column("subject")]
        public string Subject { get; set; }

        [Column("body")]
        public string Body { get; set; }

        [Column("sent_date")]
        public DateTime SentDate { get; set; }
    }
}
