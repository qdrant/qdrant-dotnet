namespace Qdrant.Grpc.Tests;

public static class SolutionPaths
{
	private static readonly Lazy<string> LazyRoot = new(FindSolutionRoot);

	private static string FindSolutionRoot()
	{
		var buildBat = "build.bat";
		var startDir = Directory.GetCurrentDirectory();
		var currentDirectory = new DirectoryInfo(startDir);
		do
		{
			if (File.Exists(Path.Combine(currentDirectory.FullName, buildBat)))
				return currentDirectory.FullName;

			currentDirectory = currentDirectory.Parent;
		} while (currentDirectory != null);

		throw new InvalidOperationException(
			$"Could not find solution root directory from the current directory {startDir}");
	}

	public static string Root => LazyRoot.Value;
}
