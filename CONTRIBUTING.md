# Contributing to .NET SDK for Qdrant

We love your input! We want to make contributing to this project as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features

## We Develop with GitHub

We use github to host code, to track issues and feature requests, as well as accept pull requests.

We Use [GitHub Flow](https://docs.github.com/en/get-started/quickstart/github-flow), so all code changes
happen through Pull Requests. Pull requests are the best way to propose changes to the codebase. 

It's usually best to open an issue first to discuss a feature or bug before opening a pull request. 
Doing so can save time and help further ascertain the crux of an issue.

1. See if there is an existing issue
2. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. Ensure the test suite passes.
4. Issue that pull request!

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

```
./build.sh format
```

before submitting a PR.

## License

By contributing, you agree that your contributions will be licensed under its Apache License 2.0.

# Building the solution

The solution uses several open source software tools:

## Docker

Qdrant docker image is used to run integration tests. Be sure to 
[install docker](https://docs.docker.com/engine/install/) and have it running when running tests.

## Bullseye

[Bullseye](https://github.com/adamralph/bullseye) is used as the build automation system for the solution.
To get started after cloning the solution, it's best to run the build script in the root

for Windows

```
.\build.bat
```

for OSX/Linux

```
./build.sh
```

This will

- Pull down all the dependencies for the build process as well as the solution
- Run the default build target for the solution

You can also compile the solution within Visual Studio or Rider if you prefer, but the build script is 
going to be _much_ faster.

## Tests

xUnit tests are run as part of the default build target. These can also be run with

```
./build.sh test
```

## Updating the client

A large portion of the client is generated from the upstream qdrant proto definitions, which are
downloaded locally as needed, based on the version defined in `<QdrantVersion>` in Directory.Build.props
in the root directory.

When a new qdrant version is released upstream, update the `<QdrantVersion>` value to the new version,
then run the build script

for Windows

```
.\build.bat
```

for OSX/Linux

```
./build.sh
```

The generated files do not form part of the checked in source code. Instead, they are generated
and emitted into the respective Target Framework Moniker (TFM) directory in the given
Configuration directory in the Qdrant.Client `obj` directory. For example, a Release build of
net6.0 emits generated files to `src/Qdrant.Client/obj/Release/net6.0`, and these generated
files are included in compilation.

If upstream changes to proto definitions change the API of generated code, you may need 
to fix compilation errors in code that relies on that generated code. 

## MinVer

[MinVer](https://github.com/adamralph/minver) is used for versioning projects and packages using
Git tags. To create a new release

1. Create a new tag and push it to origin

   ```
   git tag 1.6.0
   git push --tags
   ```
   
2. Create a new release on GitHub from the tag
3. Upload the nuget package artifact from the GitHub action workflow run triggered by creating the release.
