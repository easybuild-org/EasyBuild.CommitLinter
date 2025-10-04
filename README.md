# EasyBuild.CommitLinter


[![NuGet](https://img.shields.io/nuget/v/EasyBuild.CommitLinter.svg)](https://www.nuget.org/packages/EasyBuild.CommitLinter)
[![](https://img.shields.io/badge/Sponsors-EA4AAA)](https://mangelmaxime.github.io/sponsors/)

EasyBuild.CommitLinter is a .NET tool to lint your commit messages. It checks if the commit message follows the [commit format described below](#commit-format).

> [!TIP]
> EasyBuild.CommitLinter has a companion tool called [EasyBuild.ChangelogGen](https://github.com/easybuild-org/EasyBuild.ChangelogGen) which generates a changelog based on the Git history.

## Usage

```bash
# Install the tool
dotnet tool install EasyBuild.CommitLinter

# Run the tool
dotnet commit-linter --help
```

```text
DESCRIPTION:
Lint your commit message based on EasyBuild.CommitLinter conventions

Learn more at https://github.com/easybuild-org/EasyBuild.CommitLinter

USAGE:
    commit-linter <commit-file> [OPTIONS]

ARGUMENTS:
    <commit-file>    Path to the commit message file

OPTIONS:
    -h, --help       Prints help information
    -v, --version    Prints version information
    -c, --config     Path to the configuration file
```

### Husky Integration

If you are using [Husky](https://alirezanet.github.io/Husky.Net/), you can register the CLI as a `commit-msg` hook to validate your commit messages by running:

```bash
# Setup Husky (if needed)
dotnet tool install Husky  
dotnet husky install

# Register hooks
dotnet husky add commit-msg -c 'dotnet commit-linter "$1"'
# $1 is provided by Husky and contains the path to the commit message file
```

## Commit Format

> [!NOTE]
> EasyBuild.CommitParser format is based on [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
>
> It add a special `Tag` footer, allowing it to be used in a mono-repo context.
>
> Tools like [EasyBuild.ChangelogGen](https://github.com/easybuild-org/EasyBuild.ChangelogGen) can use the tag to filter the commits to include in the changelog.

```text
<type>[optional scope][optional !]: <description>

[optional body]

[optional footer]
```

- `[optional body]` is a free-form text.

    ```text
    This is a single line body.
    ```

    ```text
    This is a

    multi-line body.
    ```

- `[optional footer]` is inspired by [git trailer format](https://git-scm.com/docs/git-interpret-trailers) `key: value` but also allows `key #value`

    ```text
    BREAKING CHANGE: <description>
    Signed-off-by: Alice <alice@example.com>
    Signed-off-by: Bob <bob@example.com>
    Refs #123
    Tag: cli
    ```

    💡 The `Tag` footer can be provided multiple times.

## Configuration

EasyBuild.CommitParser comes with a default configuration to validate your commit.

The default configuration allows the following commit types with no tags required:

- `feat` - A new feature
- `fix` - A bug fix
- `ci` - Changes to CI/CD configuration
- `chore` - Changes to the build process or auxiliary tools and libraries such as documentation generation
- `docs` - Documentation changes
- `test` - Adding missing tests or correcting existing tests
- `style` - Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- `refactor` - A code change that neither fixes a bug nor adds a feature
- `perf` - A code change that improves performance
- `revert` - Reverts a previous commit
- `build` - Changes that affect the build system or external dependencies


If needed, you can provide a custom configuration either by code directly or by using a configuration file using JSON format.

### Configuration File Format

The configuration file is a JSON file with the following properties:

#### types

- Required: ✅
- Type: `{ name: string, description?: string, skipTagFooter?: bool } []`

List of accepted commit types.

| Property      | Type   | Required | Description                           |
| --------------| ------ | :------: | ------------------------------------- |
| name          | string |    ✅    | The name of the commit type.          |
| description   | string |    ❌    | The description of the commit type.   |
| skipTagFooter | bool   |    ❌    | If `true` skip the tag footer validation. <br> If `false`, checks that the tag footer is provided and contains knows tags. <br><br>Default is `true`. |

#### tags

- Required: ❌
- Type: `string []`

List of accepted commit tags.

#### Examples

```json
{
    "types": [
        { "name": "feat", "description": "A new feature", "skipTagFooter": false },
        { "name": "perf", "description": "A code change that improves performance", "skipTagFooter": false },
        { "name": "fix", "description": "A bug fix", "skipTagFooter": false },
        { "name": "docs", "description": "Documentation changes", "skipTagFooter": false },
        { "name": "test", "description": "Adding missing tests or correcting existing tests", "skipTagFooter": false },
        { "name": "style", "description": "Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)", "skipTagFooter": false },
        { "name": "refactor", "description": "A code change that neither fixes a bug nor adds a feature", "skipTagFooter": false },
        { "name": "ci", "description": "Changes to CI/CD configuration" },
        { "name": "chore", "description": "Changes to the build process or auxiliary tools and libraries such as documentation generation" },
        { "name": "revert", "description": "Reverts a previous commit" },
        { "name": "build", "description": "Changes that affect the build system or external dependencies" }
    ],
    "tags": [
        "cli",
        "converter"
    ]
}
```
