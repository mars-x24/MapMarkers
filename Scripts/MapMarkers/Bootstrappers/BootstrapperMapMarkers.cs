namespace CryoFall.Marketplace
{
  using AtomicTorch.CBND.CoreMod.Bootstrappers;
  using AtomicTorch.CBND.GameApi.Data;
  using AtomicTorch.CBND.GameApi.Data.Characters;
  using AtomicTorch.CBND.GameApi.Scripting;
  using CryoFall.MapMarkers.Scripts;

  [PrepareOrder(afterType: typeof(BootstrapperClientOptions))]
  public class BootstrapperMapMarkers : BaseBootstrapper
  {
    public override void ClientInitialize()
    {
      BootstrapperClientGame.InitEndCallback += GameInitHandler;
      BootstrapperClientGame.ResetCallback += ResetHandler;
    }
    private static void GameInitHandler(ICharacter currentCharacter)
    {
      CustomMarkManager.GetInstance().Load();
    }

    private static void ResetHandler()
    {

    }

  }
}