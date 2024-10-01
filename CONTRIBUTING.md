# Contributing to .NET SDK for Qdrant

We love your input! We want to make contributing to this project as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features

## We Develop with GitHub

We use GitHub to host code, to track issues and feature requests, as well as accept pull requests.

We Use [GitHub Flow](https://docs.github.com/en/get-started/quickstart/github-flow), so all code changes
happen through Pull Requests. Pull requests are the best way to propose changes to the codebase.

It's usually best to open an issue first to discuss a feature or bug before opening a pull request.
Doing so can save time and help further ascertain the crux of an issue.

1. See if there is an existing issue
2. Fork the repo and create your branch from `main`.
3. If you've added code that should be tested, add tests.
4. Ensure the test suite passes.
5. Issue that pull request!

### Any contributions you make will be under the Apache License 2.0

In short, when you submit code changes, your submissions are understood to be under the
same [Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/) that covers the project.
Feel free to contact the maintainers if that's a concern.

## Report bugs using GitHub's [issues](https://github.com/qdrant/qdrant-dotnet/issues)

We use GitHub issues to track public bugs. Report a bug by
[opening a new issue](https://github.com/qdrant/qdrant-dotnet/issues/new); it's that easy!

**Great Bug Reports** tend to have:

- A quick summary and/or background
- Steps to reproduce
  - Be specific!
  - Give sample code if you can.
- What you expected would happen
- What actually happens
- Notes (possibly including why you think this might be happening, or stuff you tried that didn't work)

## Coding Styleguide

If you are modifying code, make sure it has no warnings when building.
The project uses [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) formatter in
conjunction with rules defined in .editorconfig file. Please ensure to run it using

```bash
./build.sh format
```

before submitting a PR.

## License

By contributing, you agree that your contributions will be licensed under its Apache License 2.0.

## Preparing for a New Release

The client uses generated stubs from upstream Qdrant proto definitions, which are downloaded from [qdrant/qdrant](https://github.com/qdrant/qdrant/tree/master/lib/api/src/grpc/proto).

The generated files do not form part of the checked in source code. Instead, they are generated
and emitted into the `src/Qdrant.Client/obj/Release`, and included in compilation.

### Pre-requisites

Ensure the following are installed and available in the `PATH`.

- [Dotnet 6.0.x](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Docker](https://docs.docker.com/engine/install/) for tests

### Steps

1. Set the `<QdrantVersion>` value in [Directory.Build.props](https://github.com/qdrant/qdrant-dotnet/blob/main/Directory.Build.props) to `dev`. In order to download the `dev` Docker image for testing and use the `dev` branch for fetching the proto files.

2. Download and generate the latest client stubs by running the following command from the project root:

For Windows

```bash
.\build.bat build
```

For OSX/Linux

```bash
./build.sh build
```

This will

- Pull down all the dependencies for the build process and the project.
- Run the format and default build task.

For testing, ensure Docker is running and run the following command.

```bash
.\build.bat test
```

For OSX/Linux

```bash
./build.sh test
```

3. Implement new Qdrant methods in [`QdrantClient.cs`](https://github.com/qdrant/qdrant-dotnet/blob/main/src/Qdrant.Client/QdrantClient.cs) with associated tests in [tests/Qdrant.Client.Tests/](https://github.com/qdrant/qdrant-dotnet/tree/main/tests/Qdrant.Client.Tests).

4. If there are any new complex/frequently used properties in the proto definitions, add implicit converters to [`src/Qdrant.Client/Grpc`](https://github.com/qdrant/qdrant-dotnet/tree/main/src/Qdrant.Client/Grpc) following the existing patterns.

5. Submit your pull request and get those approvals.

### Releasing a New Version

Once the new Qdrant version is live:

1. Set the `<QdrantVersion>` value in [Directory.Build.props](https://github.com/qdrant/qdrant-dotnet/blob/main/Directory.Build.props) to the released Qdrant version. For eg: `v1.13.0`.

2. Merge the pull request.

3. [MinVer](https://github.com/adamralph/minver) is used for versioning the package using Git tags. To create a new release

```bash
git tag 1.6.0
git push --tags
```

4. Create a new release on GitHub from the tag. The CI will upload a NuGet package artifact after the release.
