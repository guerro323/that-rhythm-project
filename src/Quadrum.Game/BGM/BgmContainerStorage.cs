using CSScripting;
using revghost.IO.Storage;

namespace Quadrum.Game.BGM;

public class BgmContainerStorage : IStorage
{
    public readonly IStorage parent;

    public BgmContainerStorage(IStorage parent)
    {
        this.parent = parent;
    }

    public string CurrentPath => parent.CurrentPath;

    public void GetFiles<TList>(string pattern, TList listToFill)
        where TList : IList<IFile> => parent.GetFiles(pattern, listToFill);
    public IStorage GetSubStorage(string path) => parent.GetSubStorage(path);
		
    public async IAsyncEnumerable<BgmFile> GetBgmAsync(string pattern)
    {
        using var files = this.GetPooledFiles(pattern);
        foreach (var f in files)
        {
            var bgm = new BgmFile(f);
            await bgm.ComputeDescription();

            yield return bgm;
        }
    }
}