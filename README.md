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
dotnet husky add commit-msg -c 'dotnet commit-linter "$1"'
# $1 is provided by Husky and contains the path to the commit message file
```

## Commit Format

> [!NOTE]
> EasyBuild.CommitLinter format is based on [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) and extends it to work in a monorepo environment.

```text
<type>[optional scope][optional !]: <description>

[optional tags]

[optional body]

[optional footer]
```

-   `[optional tags]` format is as follows:

    ```text
    [tag1][tag2][tag3]
    ```

-   `[optional body]` is a free-form text.

    ```text
    This is a single line body.
    ```

    ```text
    This is a

    multi-line body.
    ```

-   `[optional footer]` follows [git trailer format](https://git-scm.com/docs/git-interpret-trailers)

    ```text
    BREAKING CHANGE: <description>
    Signed-off-by: Alice <alice@example.com>
    Signed-off-by: Bob <bob@example.com>
    ```

## Configuration

EasyBuild.CommitLinter comes with a default configuration to validate your commit.

The default configuration allows the following commit types with no tags required:

- `feat` - A new feature
- `fix` - A bug fix
- `ci` - Changes to CI/CD configuration
- `chore` - Changes to the build process or auxiliary tools and libraries such as documentation generation
- `docs` - Documentation changes
- `test` - Adding missing tests or correcting existing tests
- `style` - Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
- `refactor` - A code change that neither fixes a bug nor adds a feature

If needed, you can provide a custom configuration file by using the `--config` option.

### Configuration File Format

The configuration file is a JSON file with the following properties:

#### types

-   Required: ✅
-   Type: `{ name: string, description?: string, skipTagLine?: bool } []`

List of accepted commit types.

| Property    | Type   | Required | Description                           |
| ----------- | ------ | :------: | ------------------------------------- |
| name        | string |    ✅    | The name of the commit type.          |
| description | string |    ❌    | The description of the commit type.   |
| skipTagLine | bool   |    ❌    | If `true` skip the tag line validation. <br> If `false`, checks that the tag line format is valid and contains knows tags. <br><br>Default is `true`. |

#### tags

-   Required: ❌
-   Type: `string []`

List of accepted commit tags.

#### Examples

```json
{
    "types": [
        { "name": "feat", "description": "A new feature", "skipTagLine": false },
        { "name": "fix", "description": "A bug fix", "skipTagLine": false },
        { "name": "docs", "description": "Documentation changes", "skipTagLine": false },
        { "name": "test", "description": "Adding missing tests or correcting existing tests", "skipTagLine": false },
        { "name": "style", "description": "Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)", "skipTagLine": false },
        { "name": "refactor", "description": "A code change that neither fixes a bug nor adds a feature", "skipTagLine": false },
        { "name": "ci", "description": "Changes to CI/CD configuration" },
        { "name": "chore", "description": "Changes to the build process or auxiliary tools and libraries such as documentation generation" }
    ],
    "tags": [
        "cli",
        "converter"
    ]
}
```
