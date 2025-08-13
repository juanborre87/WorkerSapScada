using Arq.Host;

namespace Application.UseCases.Operation.SendConfirm.ToSap;

public class SendConfirmToSapResponse : Notify
{
    public bool Result { get; set; }
}
