using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekShopping.PaymentProcessor
{
    public class ProcessPayment : IProcessPayment
    {
        public bool PaymentProcessor()
        {
            //Existe apenas para simular o processamento de pagamento. Aqui retorna sempre true. 
            return true;
        }
    }
}
