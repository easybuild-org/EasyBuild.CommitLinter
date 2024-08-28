module Tests.Lint

open CommitLinter.Commands.Lint
open Workspace
open Tests.Setup

[<Test>]
let ``LintCommand.Execute should return 1 when the commit file does not exist`` () =
    let actual =
        LintCommand()
            .Execute(null, LintSettings(CommitFile = "this-file-does-not-exist"))

    Expect.equal actual 1

[<Test>]
let ``LintCommand.Execute should return 1 when the config file does not exist`` () =
    let actual =
        LintCommand()
            .Execute(
                null,
                LintSettings(
                    CommitFile = Workspace.fixtures.``shortCommitOnly.txt``,
                    Config = Some "this-file-does-not-exist"
                )
            )

    Expect.equal actual 1

[<Test>]
let ``LintCommand.Execute should return 1 when the config file is invalid`` () =
    let actual =
        LintCommand()
            .Execute(
                null,
                LintSettings(
                    CommitFile = Workspace.fixtures.``shortCommitOnly.txt``,
                    Config = Some Workspace.fixtures.``invalid-custom-config.json``
                )
            )

    Expect.equal actual 1

[<Test>]
let ``LintCommand.Execute use the provided config file`` () =
    let actual =
        LintCommand()
            .Execute(
                null,
                LintSettings(
                    CommitFile = Workspace.fixtures.``custom-type.txt``,
                    Config = Some Workspace.fixtures.``custom-config.json``
                )
            )

    Expect.equal actual 0

[<Test>]
let ``LintCommand.Execute should return 0 when the commit file exists and is valid`` () =
    Expect.equal
        (LintCommand()
            .Execute(null, LintSettings(CommitFile = Workspace.fixtures.``shortCommitOnly.txt``)))
        0

    Expect.equal
        (LintCommand()
            .Execute(null, LintSettings(CommitFile = Workspace.fixtures.``shortCommitWithTag.txt``)))
        0

    Expect.equal
        (LintCommand()
            .Execute(
                null,
                LintSettings(CommitFile = Workspace.fixtures.``shortCommitWithTags.txt``)
            ))
        0

    Expect.equal
        (LintCommand()
            .Execute(null, LintSettings(CommitFile = Workspace.fixtures.``commitWithBody.txt``)))
        0

    Expect.equal
        (LintCommand()
            .Execute(
                null,
                LintSettings(CommitFile = Workspace.fixtures.``commitWithTagsAndBody.txt``)
            ))
        0

[<Test>]
let ``LintCommand.Execute works with relative file path for the commit file`` () =
    let actual =
        LintCommand()
            .Execute(null, LintSettings(CommitFile = "../../../fixtures/shortCommitOnly.txt"))

    Expect.equal actual 0

[<Test>]
let ``LintCommand.Execute works with relative file path for the config file`` () =
    let actual =
        LintCommand()
            .Execute(
                null,
                LintSettings(
                    CommitFile = Workspace.fixtures.``custom-type.txt``,
                    Config = Some "../../../fixtures/custom-config.json"
                )
            )

    Expect.equal actual 0

[<Test>]
let ``LintCommand.Execute should return 1 when the commit file exists and is invalid`` () =
    let actual =
        LintCommand()
            .Execute(
                null,
                LintSettings(
                    CommitFile =
                        Workspace.fixtures.``invalidCommitMissingEmptyLineAfterShortCommit.txt``
                )
            )

    Expect.equal actual 1
