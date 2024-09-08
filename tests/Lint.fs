module Tests.Lint

open CommitLinter.Commands.Lint
open Workspace
open Tests.Setup
open Faqt

[<Test>]
let ``LintCommand.Execute should return 1 when the commit file does not exist`` () =
    LintCommand()
        .Execute(null, LintSettings(CommitFile = "this-file-does-not-exist"))
        .Should()
        .Be(1)
    |> ignore

[<Test>]
let ``LintCommand.Execute should return 1 when the config file does not exist`` () =
    LintCommand()
        .Execute(
            null,
            LintSettings(
                CommitFile = Workspace.fixtures.``shortCommitOnly.txt``,
                Config = Some "this-file-does-not-exist"
            )
        )
        .Should()
        .Be(1)
    |> ignore

[<Test>]
let ``LintCommand.Execute should return 1 when the config file is invalid`` () =
    LintCommand()
        .Execute(
            null,
            LintSettings(
                CommitFile = Workspace.fixtures.``shortCommitOnly.txt``,
                Config = Some Workspace.fixtures.``invalid-custom-config.json``
            )
        )
        .Should()
        .Be(1)
    |> ignore

[<Test>]
let ``LintCommand.Execute use the provided config file`` () =
    LintCommand()
        .Execute(
            null,
            LintSettings(
                CommitFile = Workspace.fixtures.``custom-type.txt``,
                Config = Some Workspace.fixtures.``custom-config.json``
            )
        )
        .Should()
        .Be(0)
    |> ignore

[<Test>]
let ``LintCommand.Execute should return 0 when the commit file exists and is valid`` () =
    LintCommand()
        .Execute(null, LintSettings(CommitFile = Workspace.fixtures.``shortCommitOnly.txt``))
        .Should()
        .Be(0)
    |> ignore

    LintCommand()
        .Execute(null, LintSettings(CommitFile = Workspace.fixtures.``shortCommitWithTag.txt``))
        .Should()
        .Be(0)
    |> ignore

    LintCommand()
        .Execute(null, LintSettings(CommitFile = Workspace.fixtures.``shortCommitWithTags.txt``))
        .Should()
        .Be(0)
    |> ignore

    LintCommand()
        .Execute(null, LintSettings(CommitFile = Workspace.fixtures.``commitWithBody.txt``))
        .Should()
        .Be(0)
    |> ignore

    LintCommand()
        .Execute(null, LintSettings(CommitFile = Workspace.fixtures.``commitWithTagsAndBody.txt``))
        .Should()
        .Be(0)
    |> ignore

[<Test>]
let ``LintCommand.Execute works with relative file path for the commit file`` () =
    LintCommand()
        .Execute(null, LintSettings(CommitFile = "../../../fixtures/shortCommitOnly.txt"))
        .Should()
        .Be(0)
    |> ignore

[<Test>]
let ``LintCommand.Execute works with relative file path for the config file`` () =
    LintCommand()
        .Execute(
            null,
            LintSettings(
                CommitFile = Workspace.fixtures.``custom-type.txt``,
                Config = Some "../../../fixtures/custom-config.json"
            )
        )
        .Should()
        .Be(0)
    |> ignore

[<Test>]
let ``LintCommand.Execute should return 1 when the commit file exists and is invalid`` () =
    LintCommand()
        .Execute(
            null,
            LintSettings(
                CommitFile =
                    Workspace.fixtures.``invalidCommitMissingEmptyLineAfterShortCommit.txt``
            )
        )
        .Should()
        .Be(1)
    |> ignore
