namespace DigitalVoice.AmbeSupport;

public static class AmbeControllerFactory
{
    public static IAmbe3000RController Create(AmbeConfig cfg)
    {
        return cfg.AmbeServiceType switch
        {
            AmbeServiceType.Server => new AmbeUdpClient(cfg.ServerAddr, cfg.ServerPort),
            AmbeServiceType.Stick => new Ambe3000RController(cfg.StickComport, cfg.StickBaudrate),
            _ => throw new InvalidOperationException(),
        };
    }
}
