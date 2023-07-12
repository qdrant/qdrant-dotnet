using System.CommandLine;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bullseye;
using SharpCompress.Readers;
using static BuildTargets;
using static Bullseye.Targets;
using static SimpleExec.Command;

const string envVarMissing = " environment variable is missing. Aborting.";
const string packOutput = "nuget";
const string protosDir = "protos";
const string project = "Qdrant.Grpc";

var doc = XDocument.Load("Directory.Build.props");
var qdrantVersion = doc.Descendants(XName.Get("QdrantVersion", "http://schemas.microsoft.com/developer/msbuild/2003"))
	.First().Value;

var cmd = new RootCommand
{
	new Argument<string[]>("targets")
	{
		Description =
			"A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.",
	}
};

foreach (var (aliases, description) in Options.Definitions)
	cmd.Add(new Option<bool>(aliases.ToArray(), description));

cmd.SetHandler(async () =>
{
	// translate from System.CommandLine to Bullseye
	var cmdLine = cmd.Parse(args);
	var targets = cmdLine.CommandResult.Tokens.Select(token => token.Value);
	var options = new Options(Options.Definitions.Select(d => (d.Aliases[0],
		cmdLine.GetValueForOption(cmd.Options.OfType<Option<bool>>().Single(o => o.HasAlias(d.Aliases[0]))))));

	Target(Restore, () =>
	{
		Run("dotnet", "restore");
	});

	Target(CleanBuildOutput, DependsOn(Restore), () =>
	{
		Run("dotnet", "clean -c Release -v m --nologo");
	});

	Target(DownloadProtos, async () =>
	{
		var protosTagDir = Path.Combine(protosDir, qdrantVersion);
		if (Directory.Exists(protosTagDir) && Directory.EnumerateFileSystemEntries(protosTagDir).Any())
		{
			Console.WriteLine($"Already downloaded protos for {qdrantVersion}");
			return;
		}

		Directory.CreateDirectory(protosTagDir);
		Console.WriteLine($"Downloading protos for tag {qdrantVersion} to {protosTagDir}");
		var url = $"https://api.github.com/repos/qdrant/qdrant/tarball/refs/tags/{qdrantVersion}";
		var protoFileRegex = new Regex(".*?lib/api/src/grpc/proto/.*?.proto");
		var client = new HttpClient
		{
			DefaultRequestHeaders = { UserAgent = { new ProductInfoHeaderValue("QdrantNet", "1.0.0") } },
		};

		var response = await client.GetAsync(url);
		await using var stream = await response.Content.ReadAsStreamAsync();
		await using var gzip = new GZipStream(stream, CompressionMode.Decompress);
		var reader = ReaderFactory.Open(gzip);
		while (reader.MoveToNextEntry())
		{
			if (!reader.Entry.IsDirectory && protoFileRegex.IsMatch(reader.Entry.Key))
				reader.WriteEntryToDirectory(protosTagDir);
		}

		// add csharp namespace to proto files if they don't contain one
		foreach (var file in Directory.EnumerateFiles(protosTagDir))
		{
			var contents = File.ReadAllLines(file).ToList();
			if (contents.Any(line => line.Contains("option csharp_namespace")))
				continue;

			var index = 0;
			for (var i = 0; i < contents.Count; i++)
			{
				if (contents[i].StartsWith("syntax") ||
				    contents[i].StartsWith("import") ||
				    contents[i].StartsWith("package") ||
				    contents[i].StartsWith("//") ||
				    string.IsNullOrWhiteSpace(contents[i]))
					continue;

				index = i;
				break;
			}

			contents.Insert(index,$"option csharp_namespace = \"{project}\";");
			File.WriteAllLines(file, contents);
		}
	});

	Target(Build, DependsOn(DownloadProtos, CleanBuildOutput), () =>
	{
		Run("dotnet", "build -c Release --nologo");
	});

	Target(Test, DependsOn(Build), () =>
	{
		Run("dotnet", "test -c Release --no-build");
	});

	Target(CleanPackOutput, () =>
	{
		if (Directory.Exists(packOutput))
			Directory.Delete(packOutput, true);
	});

	Target(Pack, DependsOn(Build, CleanPackOutput), () =>
	{
		var outputDir = Directory.CreateDirectory(packOutput);
		Run("dotnet",
			$"pack src/{project}/{project}.csproj -c Release -o \"{outputDir.FullName}\" --no-build --nologo");
	});

	Target(Default, DependsOn(Test));

	await RunTargetsAndExitAsync(targets, options,
		messageOnly: ex => ex is SimpleExec.ExitCodeException || ex.Message.EndsWith(envVarMissing));
});

return await cmd.InvokeAsync(args);

internal static class BuildTargets
{
	public const string CleanBuildOutput = "clean-build-output";
	public const string CleanPackOutput = "clean-pack-output";
	public const string Build = "build";
	public const string Test = "test";
	public const string Default = "default";
	public const string Restore = "restore";
	public const string Pack = "pack";
	public const string DownloadProtos = "download-protos";
}
