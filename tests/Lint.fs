module Lint

open Expecto
open Expecto.NoMessage
open CommitLinter.Commands
open Workspace

[<Tests>]
let tests =
    testList
        "Lint"
        [
            test "LintCommand.Execute should return 1 when the commit file does not exist" {
                let actual =
                    LintCommand()
                        .Execute(null, LintSettings(CommitFile = "this-file-does-not-exist"))

                Expect.equal actual 1
            }

            test "LintCommand.Execute should return 1 when the config file does not exist" {
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
            }

            test "LintCommand.Execute should return 1 when the config file is invalid" {
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
            }

            test "LintCommand.Execute use the provided config file" {
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
            }

            test "LintCommand.Execute should return 0 when the commit file exists and is valid" {
                Expect.equal
                    (LintCommand()
                        .Execute(
                            null,
                            LintSettings(CommitFile = Workspace.fixtures.``shortCommitOnly.txt``)
                        ))
                    0

                Expect.equal
                    (LintCommand()
                        .Execute(
                            null,
                            LintSettings(CommitFile = Workspace.fixtures.``shortCommitWithTag.txt``)
                        ))
                    0

                Expect.equal
                    (LintCommand()
                        .Execute(
                            null,
                            LintSettings(
                                CommitFile = Workspace.fixtures.``shortCommitWithTags.txt``
                            )
                        ))
                    0

                Expect.equal
                    (LintCommand()
                        .Execute(
                            null,
                            LintSettings(CommitFile = Workspace.fixtures.``commitWithBody.txt``)
                        ))
                    0

                Expect.equal
                    (LintCommand()
                        .Execute(
                            null,
                            LintSettings(
                                CommitFile = Workspace.fixtures.``commitWithTagsAndBody.txt``
                            )
                        ))
                    0
            }

            test "LintCommand.Execute should return 1 when the commit file exists and is invalid" {
                Expect.equal
                    (LintCommand()
                        .Execute(
                            null,
                            LintSettings(
                                CommitFile =
                                    Workspace.fixtures.``invalidCommitMissingEmptyLineAfterShortCommit.txt``
                            )
                        ))
                    1
            }
        ]
