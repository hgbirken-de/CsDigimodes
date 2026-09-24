
using DigitalVoice.AmbeSupport;

public class MyFixture : IDisposable
{

    Ambe3000RController Controller;

    public MyFixture()
    {
        // One-time setup here ("before class")
        Console.WriteLine("MyFixture: Setup");
    }

    public void Dispose()
    {
        // One-time cleanup here ("after class")
        Console.WriteLine("MyFixture: Cleanup");
    }
}