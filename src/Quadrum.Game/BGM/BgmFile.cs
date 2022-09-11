using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using revghost.IO.Storage;

namespace Quadrum.Game.BGM;

public class BgmFile : IFile
{
	private readonly IFile parent;
	private readonly FileInfo cachedInfo;

	public BgmDescription Description { get; private set; }
	public JsonElement DirectorJson { get; private set; }

	public void GetContent<TList>(TList listToFill) where TList : IList<byte>
	{
		parent.GetContent(listToFill);
	}

	public Task GetContentAsync<TList>(TList listToFill) where TList : IList<byte>
	{
		return parent.GetContentAsync(listToFill);
	}

	public string Name => parent.Name;
	public string FullName => parent.FullName;
	
	public BgmFile(IFile parent)
	{
		this.parent = parent;

		cachedInfo = new FileInfo(FullName);
	}

	public async Task ComputeDescription()
	{
		Func<Task> call = cachedInfo.Extension switch
		{
			".zip" => FromZip,
			".json" => FromJson,
			_ => throw new NotImplementedException($"No convert implemented for '{cachedInfo.Extension}' extension")
		};

		await call();
	}

	private async Task FromZip()
	{
		using var bytes = this.GetPooledBytes();
		
		await using var memoryStream = new MemoryStream(bytes.ToArray());
		using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
		var descEntry = archive.GetEntry("description.json");

		// TODO: file errors should be more explicit to the end user
		if (descEntry == null)
			throw new NullReferenceException(nameof(descEntry));

		await using var stream = descEntry.Open();
		using var document = await JsonDocument.ParseAsync(stream);
		ReadDescription(document);
	}

	private async Task FromJson()
	{
		using var bytes = this.GetPooledBytes();

		using var document = JsonDocument.Parse(bytes.ToArray());
		ReadDescription(document);
	}

	private void ReadDescription(JsonDocument document)
	{
		var root = document.RootElement;
		string storePath;
		if (cachedInfo.Extension == ".zip")
		{
			storePath = "zip://";
		}
		else if (root.TryGetProperty("store", out var storeProperty))
		{
			storePath = storeProperty.GetString();
		}
		else
		{
			storePath = $"relative://{root.GetProperty("id").GetString()}/";
		}

		if (root.TryGetProperty("director", out var director))
		{
			DirectorJson = director;
		}
		else
		{
			DirectorJson = JsonDocument.Parse("{}").RootElement;
		}

		Description = new BgmDescription
		{
			Id = root.GetProperty("id").GetString(),
			Name = root.GetProperty("name").GetString(),
			Author = root.GetProperty("author").GetString(),
			Description = root.GetProperty("description").GetString(),
			StorePath = storePath
		};
	}
}

public struct BgmDescription
{
	public string Id;
	public string Name;
	public string Author;
	public string Description;
	public string StorePath;
}