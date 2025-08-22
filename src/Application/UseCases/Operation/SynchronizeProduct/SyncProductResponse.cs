using Arq.Host;

namespace Application.UseCases.Operation.SynchronizeProduct;

public class SyncProductResponse : Notify
{
    public bool Result { get; set; }
}
