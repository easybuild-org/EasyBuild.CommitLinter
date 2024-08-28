module Tests.Interactive

open CommitLinter.Commands.Interactive
open Workspace
open Tests.Setup
open EasyBuild.CommitParser.Types
open System

open type Verify.Extensions.VerifierExt

module CommitType =

    let feat =
        {
            Name = "feat"
            Description = Some "A new feature"
            SkipTagLine = true
        }

module GenerateCommitMessage =

    [<Test>]
    let ``short message`` () =
        {
            CommitType = CommitType.feat
            Tags = None
            ShortMessage = "add new feature"
            Description = None
            IsBreakingChange = false
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + long message`` () =
        {
            CommitType = CommitType.feat
            Tags = None
            ShortMessage = "add new feature"
            Description = Some "This is a long description"
            IsBreakingChange = false
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short messag + breaking change`` () =
        {
            CommitType = CommitType.feat
            Tags = None
            ShortMessage = "add new feature"
            Description = None
            IsBreakingChange = true
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + long message + breaking change`` () =
        {
            CommitType = CommitType.feat
            Tags = None
            ShortMessage = "add new feature"
            Description = Some "This is a long description"
            IsBreakingChange = true
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + one tag`` () =
        {
            CommitType = CommitType.feat
            Tags = Some [ "tag1" ]
            ShortMessage = "add new feature"
            Description = None
            IsBreakingChange = false
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + multiple tags`` () =
        {
            CommitType = CommitType.feat
            Tags = Some [ "tag1"; "tag2"; "tag3" ]
            ShortMessage = "add new feature"
            Description = None
            IsBreakingChange = false
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + one tag + long message`` () =
        {
            CommitType = CommitType.feat
            Tags = Some [ "tag1" ]
            ShortMessage = "add new feature"
            Description = Some "This is a long description"
            IsBreakingChange = false
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + multiple tags + long message`` () =
        {
            CommitType = CommitType.feat
            Tags = Some [ "tag1"; "tag2"; "tag3" ]
            ShortMessage = "add new feature"
            Description = Some "This is a long description"
            IsBreakingChange = false
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + one tag + breaking change`` () =
        {
            CommitType = CommitType.feat
            Tags = Some [ "tag1" ]
            ShortMessage = "add new feature"
            Description = None
            IsBreakingChange = true
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + multiple tags + breaking change`` () =
        {
            CommitType = CommitType.feat
            Tags = Some [ "tag1"; "tag2"; "tag3" ]
            ShortMessage = "add new feature"
            Description = None
            IsBreakingChange = true
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + one tag + long message + breaking change`` () =
        {
            CommitType = CommitType.feat
            Tags = Some [ "tag1" ]
            ShortMessage = "add new feature"
            Description = Some "This is a long description"
            IsBreakingChange = true
        }
        |> generateCommitMessage
        |> Verify

    [<Test>]
    let ``short message + multiple tags + long message + breaking change`` () =
        {
            CommitType = CommitType.feat
            Tags = Some [ "tag1"; "tag2"; "tag3" ]
            ShortMessage = "add new feature"
            Description = Some "This is a long description"
            IsBreakingChange = true
        }
        |> generateCommitMessage
        |> Verify

module Workflow =

    open Spectre.Console.Testing
    open Spectre.Console

    [<Test>]
    let ``Prompt.commitType works`` () =
        let console = new TestConsole()
        console.Profile.Capabilities.Interactive <- true

        console.Input.PushKey(ConsoleKey.DownArrow)
        console.Input.PushKey(ConsoleKey.Enter)

        let selectedCommitType = Prompt.commitType console CommitParserConfig.Default

        Expect.equal selectedCommitType CommitParserConfig.Default.Types[1]

    [<Test>]
    let ``promptCommitMessage works with Default config`` () =
        let console = new TestConsole()
        console.Profile.Capabilities.Interactive <- true

        console.Input.PushKey(ConsoleKey.DownArrow)
        console.Input.PushKey(ConsoleKey.Enter)
        console.Input.PushTextWithEnter("an amazing fix")
        console.Input.PushKey(ConsoleKey.Enter)
        console.Input.PushKey(ConsoleKey.Enter)

        let commitMessage = promptCommitMessage console CommitParserConfig.Default

        Expect.equal
            commitMessage
            {
                CommitType = CommitParserConfig.Default.Types[1]
                Tags = None
                ShortMessage = "an amazing fix"
                Description = None
                IsBreakingChange = false
            }

    [<Test>]
    let ``user can mark a commit has breaking change`` () =
        let console = new TestConsole()
        console.Profile.Capabilities.Interactive <- true

        console.Input.PushKey(ConsoleKey.DownArrow)
        console.Input.PushKey(ConsoleKey.Enter)
        console.Input.PushTextWithEnter("an amazing fix")
        console.Input.PushKey(ConsoleKey.Enter)
        console.Input.PushTextWithEnter("y")

        let commitMessage = promptCommitMessage console CommitParserConfig.Default

        Expect.equal
            commitMessage
            {
                CommitType = CommitParserConfig.Default.Types[1]
                Tags = None
                ShortMessage = "an amazing fix"
                Description = None
                IsBreakingChange = true
            }

    [<Test>]
    let ``tags are requested if the CommitType requires tag line`` () =
        let console = new TestConsole()
        console.Profile.Capabilities.Interactive <- true

        console.Input.PushKey(ConsoleKey.Enter)
        console.Input.PushKey(ConsoleKey.DownArrow)
        console.Input.PushKey(ConsoleKey.Spacebar)
        console.Input.PushKey(ConsoleKey.Enter)
        console.Input.PushTextWithEnter("an amazing feature")
        console.Input.PushKey(ConsoleKey.Enter)
        console.Input.PushKey(ConsoleKey.Enter)

        let config =
            {
                Types =
                    [
                        {
                            Name = "feat"
                            Description = Some "A new feature"
                            SkipTagLine = false
                        }
                    ]
                Tags = Some [ "converter"; "web" ]
            }

        let commitMessage = promptCommitMessage console config

        Expect.equal
            commitMessage
            {
                CommitType = config.Types[0]
                Tags = Some [ "web" ]
                ShortMessage = "an amazing feature"
                Description = None
                IsBreakingChange = false
            }
