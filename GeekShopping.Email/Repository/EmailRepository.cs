using GeekShopping.Email.Messages;
using GeekShopping.Email.Model;
using GeekShopping.Email.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.Email.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly DbContextOptions<MySQLContext> _context;

        public EmailRepository(DbContextOptions<MySQLContext> context)
        {
            _context = context;
        }

        public async Task LogEmail(UpdatePaymentResultMessage message)
        {
            EmailLog email = new EmailLog()
            {
                Email = message.Email,
                Log = $"Order - {message.OrderId} foi criada com sucesso!",
                SentDate = DateTime.Now
            };
            await using var _db = new MySQLContext(_context);
            await _db.Emails.AddAsync(email);
            await _db.SaveChangesAsync();
        }
    }
}
