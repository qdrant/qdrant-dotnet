using System.Collections.Generic;
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

const string packOutput = "nuget";
const string protosDir = "protos";
const string project = "Qdrant.Client";
const string grpcNamespace = "Qdrant.Client.Grpc";

var doc = XDocument.Load("Directory.Build.props");
var qdrantVersion = doc.Descendants(XName.Get("QdrantVersion", "http://schemas.microsoft.com/developer/msbuild/2003"))
	.First().Value;

var cmd = new RootCommand
{
	new Argument<string[]>("targets")
	{
		Description =
			"A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.",
	},
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
		Run("dotnet", "tool restore");
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
		var url = $"https://api.github.com/repos/qdrant/qdrant/tarball/{qdrantVersion}";
		var protoFileRegex = new Regex(".*?lib/api/src/grpc/proto/.*?.proto");
		var privateProtoFileRegex = new Regex("(?:.*?internal.*?|raft_service|health_check|shard_snapshots_service).proto");
		var client = new HttpClient
		{
			DefaultRequestHeaders = { UserAgent = { new ProductInfoHeaderValue("qdrant-dotnet", "1.0.0") } },
		};

		var response = await client.GetAsync(url);
		await using var stream = await response.Content.ReadAsStreamAsync();
		await using var gzip = new GZipStream(stream, CompressionMode.Decompress);
		var reader = ReaderFactory.Open(gzip);
		while (reader.MoveToNextEntry())
		{
			if (!reader.Entry.IsDirectory && protoFileRegex.IsMatch(reader.Entry.Key) && !privateProtoFileRegex.IsMatch(reader.Entry.Key))
				reader.WriteEntryToDirectory(protosTagDir);
		}

		{
			// remove references to private proto files from qdrant.proto, to allow protogen to work
			var file = $"{protosTagDir}/qdrant.proto";
			var contents = File.ReadAllLines(file).ToList();
			for (var i = contents.Count - 1; i >= 0; i--)
			{
				var line = contents[i];
				if (line.StartsWith("import") && privateProtoFileRegex.IsMatch(line))
					contents.RemoveAt(i);
			}
			File.WriteAllLines(file, contents);
		}

		// add csharp namespace to qdrant package proto files if they don't contain one
		foreach (var file in Directory.EnumerateFiles(protosTagDir))
		{
			var contents = File.ReadAllLines(file).ToList();
			if (contents.Any(line => line.Contains("option csharp_namespace")))
				continue;

			for (var i = 0; i < contents.Count; i++)
			{
				if (contents[i].StartsWith("package qdrant"))
				{
					contents.Insert(i + 1, $"option csharp_namespace = \"{grpcNamespace}\";");
					break;
				}
			}

			File.WriteAllLines(file, contents);
		}
	});

	Target(Format, DependsOn(Restore), () =>
	{
		Run("dotnet", "format");
	});

	Target(Build, DependsOn(DownloadProtos, CleanBuildOutput, Format), () =>
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

	await RunTargetsAndExitAsync(targets, options, messageOnly: ex => ex is SimpleExec.ExitCodeException);
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
	public const string Format = "format";
	public const string Pack = "pack";
	public const string DownloadProtos = "download-protos";
}
