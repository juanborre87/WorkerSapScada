using Arq.Host;

namespace Application.UseCases.Operation.SynchronizeRecipe;

public class SyncRecipeResponse : Notify
{
    public bool Result { get; set; }
}
