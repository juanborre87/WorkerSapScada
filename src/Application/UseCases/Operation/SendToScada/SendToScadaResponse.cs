using HostWorker.Models;

namespace Application.UseCases.Operation.SendToScada
{
    public class SendToScadaResponse : Notify
    {
        public bool Result { get; set; }
    }
}
