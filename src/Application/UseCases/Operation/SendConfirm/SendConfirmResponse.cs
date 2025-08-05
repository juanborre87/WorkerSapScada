using HostWorker.Models;

namespace Application.UseCases.Operation.SendConfirm
{
    public class SendConfirmResponse : Notify
    {
        public bool Result { get; set; }
    }
}
