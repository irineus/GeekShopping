using GeekShopping.Email.Messages;
using GeekShopping.Email.Model;
using GeekShopping.Email.Model.Context;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace GeekShopping.Email.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly DbContextOptions<MySQLContext> _context;
        private readonly IConfiguration _configuration;

        private const string Remetente = "noreply@noreply.com";

        public EmailRepository(DbContextOptions<MySQLContext> context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task ProcessEmail(UpdatePaymentResultMessage message)
        {
            try
            {
                EmailLog email = new EmailLog()
                {
                    From = "GeekShoppingWeb@gmail.com",
                    To = message.Email,
                    Subject = $"Pedido - {message.OrderId} foi criado com sucesso!",
                    Body = "Mensagem vazia, por enquanto.",
                    SentDate = DateTime.Now
                };
                SendEmail(email);
                await LogEmail(email);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao processar a mensagem de e-mail: {ex.Message}");
            }
        }

        private void SendEmail(EmailLog email)
        {
            try
            {
                // valida o email
                bool bValidaEmail = ValidaEnderecoEmail(email.To);

                // Se o email não é validao retorna uma mensagem
                if (bValidaEmail == false)
                    throw new Exception("Email do destinatário inválido: " + email.To);

                string emailPassword = _configuration["GeekShoppingWebEmailPassword"];

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(email.From, emailPassword),
                    EnableSsl = true,
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email.From),
                    Subject = email.Subject,
                    Body = email.Body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email.To);

                smtpClient.Send(mailMessage);

                Console.WriteLine("Mensagem enviada para  " + email.To + " às " + DateTime.Now.ToString() + ".");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao enviar a mensagem de e-mail: {ex.Message}");
            }
        }

        private static bool ValidaEnderecoEmail(string enderecoEmail)
        {
            try
            {
                //define a expressão regulara para validar o email
                string texto_Validar = enderecoEmail;
                Regex expressaoRegex = new Regex(@"\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}");

                // testa o email com a expressão
                if (expressaoRegex.IsMatch(texto_Validar))
                {
                    // o email é valido
                    return true;
                }
                else
                {
                    // o email é inválido
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task LogEmail(EmailLog emailLog)
        {
            await using var _db = new MySQLContext(_context);
            await _db.Emails.AddAsync(emailLog);
            await _db.SaveChangesAsync();
        }
    }
}
