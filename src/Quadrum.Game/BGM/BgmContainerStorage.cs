using CSScripting;
using revghost.IO.Storage;

namespace Quadrum.Game.BGM;

public class BgmContainerStorage
{
    public readonly MultiStorage parent;

    public BgmContainerStorage()
    {
        parent = new MultiStorage();
    }

    public string CurrentPath => parent.CurrentPath;

    public void AddStorage(IStorage storage)
    {
        parent.Add(storage);
    }
    
    public async IAsyncEnumerable<BgmFile> GetBgmAsync(string pattern)
    {
        using var files = parent.GetPooledFiles(pattern + ".zip");
        foreach (var f in files)
        {
            var bgm = new BgmFile(f);
            await bgm.ComputeDescription();

            yield return bgm;
        }
    }
}