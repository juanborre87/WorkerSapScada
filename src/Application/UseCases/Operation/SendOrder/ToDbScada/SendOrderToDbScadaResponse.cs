using Arq.Host;

namespace Application.UseCases.Operation.SendOrder.ToDbScada
{
    public class SendOrderToDbScadaResponse : Notify
    {
        public bool Result { get; set; }
    }
}
