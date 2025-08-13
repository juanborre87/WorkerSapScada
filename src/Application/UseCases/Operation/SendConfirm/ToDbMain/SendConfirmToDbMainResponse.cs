using Arq.Host;

namespace Application.UseCases.Operation.SendConfirm.ToDbMain;

public class SendConfirmToDbMainResponse : Notify
{
    public bool Result { get; set; }
}
