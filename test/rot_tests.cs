namespace VsTool.Unittests;

[TestClass]
public class RotTests
{
    [TestMethod]
    public void RotEachRunningDoesntFail()
    {
        if (OperatingSystem.IsWindows())
        {
            // TODO - for now this isn't a test - its just showing it
            // doesn't crash (maybe with COM that's a plus) because we
            // can't manipulate the ROT ourselves
            foreach (var e in ROT.EachRunning())
            {
                Assert.IsTrue(e.Length > 0);
            }
        }
    }

    [TestMethod]
    public void RotIsRunningDoesntFindSomethingBogus()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.IsFalse(ROT.IsRunning("c:\\Foo\\Doesntexist"));
        }
    }

}
