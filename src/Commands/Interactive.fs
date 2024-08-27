module CommitLinter.Commands.Interactive

open System
open System.IO
open Spectre.Console
open Spectre.Console.Cli
open System.ComponentModel
open CommitLinter.ConfigLoader
open EasyBuild.CommitParser.Types

type InteractiveSettings() =
    inherit CommandSettings()

    [<CommandOption("-c|--config")>]
    [<Description("Path to the configuration file")>]
    member val Config: string option = None with get, set

    [<CommandArgument(0, "<commit-file>")>]
    [<Description("Path to the commit message file")>]
    member val CommitFile = "" with get, set

    [<CommandOption("--skip-confirmation")>]
    [<Description("Skip confirmation before shipping the commit")>]
    member val SkipConfirmation = false with get, set

let private promptCommitType (config: CommitParserConfig) =
    AnsiConsole.Clear()

    // Helper functions to convert between CommitType and string
    let commitTypeToChoice (commitType: CommitType) =
        match commitType.Description with
        | Some description -> $"%s{commitType.Name} [grey]%s{description}[/]"
        | None -> $"%s{commitType.Name}"

    // Helper function to convert (back) from a choice string to CommitType
    let choiceToCommitType (choice: string) =
        config.Types
        |> List.find (fun commitType -> commitTypeToChoice commitType = choice)

    let prompt = SelectionPrompt<string>(Title = "Select the type of commit")

    let choices = config.Types |> List.map commitTypeToChoice |> Array.ofList

    choices |> prompt.AddChoices |> AnsiConsole.Prompt |> choiceToCommitType

let private promptCommitTags (config: CommitParserConfig) (commitType: CommitType) =
    AnsiConsole.Clear()

    if commitType.SkipTagLine then
        None
    else
        match config.Tags with
        | None ->
            AnsiConsole.MarkupLine("[red]Error:[/] No tags defined in the configuration file.")
            exit 1

        | Some tags ->
            let instructionsText =
                [
                    "[grey](Multiple tags can be selected)[/]"
                    "[grey](Press [blue]<up>[/] and [blue]<down>[/] to navigate, [blue]<space>[/] to toggle a tag, [green]<enter>[/] to accept)[/]"
                ]
                |> String.concat "\n"

            let prompt =
                MultiSelectionPrompt<string>(
                    Title = "Select tags",
                    InstructionsText = instructionsText
                )

            tags |> prompt.AddChoices |> AnsiConsole.Prompt |> Seq.toList |> Some

let private promptShortMessage () =
    AnsiConsole.Clear()

    TextPrompt<string>("Short message:") |> AnsiConsole.Prompt

let private promptDescription () =
    AnsiConsole.Clear()

    let description =
        TextPrompt<string>("Long description (optional): ", AllowEmpty = true)
        |> AnsiConsole.Prompt

    if String.IsNullOrWhiteSpace description then
        None
    else
        Some description

let private promptIsBreakingChange () =
    AnsiConsole.Clear()

    let prompt = ConfirmationPrompt("Is this a breaking change?", DefaultValue = false)

    prompt |> AnsiConsole.Prompt

let private promptCommitConfirmation (commitMessage: string) =
    AnsiConsole.Clear()

    AnsiConsole.MarkupLine "=== Commit message ==="

    commitMessage |> StringExtensions.EscapeMarkup |> AnsiConsole.MarkupLine

    AnsiConsole.MarkupLine "=== End of commit message ==="

    ConfirmationPrompt("Is this commit message correct?") |> AnsiConsole.Prompt

type CommitMessageConfig =
    {
        CommitType: CommitType
        Tags: string list option
        ShortMessage: string
        Description: string option
        IsBreakingChange: bool
    }

let private generateCommitMessage (config: CommitMessageConfig) =
    let breakingChangeSuffix =
        if config.IsBreakingChange then
            "!"
        else
            ""

    let inline newLine () = "\n"

    [
        $"%s{config.CommitType.Name}%s{breakingChangeSuffix}: %s{config.ShortMessage}"
        newLine ()

        // Tags:
        // [tag1][tag2][tag3]
        match config.Tags with
        | Some tags ->
            newLine ()
            tags |> List.map (fun tag -> $"[%s{tag}]") |> String.concat ""
        | None -> ""

        match config.Description with
        | Some longDescription ->
            newLine ()
            longDescription
        | None -> ""
    ]
    |> String.concat ""

type InteractiveCommand() =
    inherit Command<InteractiveSettings>()

    interface ICommandLimiter<InteractiveSettings>

    override __.Execute(context, settings) =
        // if settings.CommitFile

        match tryLoadConfig settings.Config with
        | LoadConfig.Failed -> 1
        | LoadConfig.Success config ->
            let commitMessageText =
                let commitType = promptCommitType config

                {
                    CommitType = commitType
                    Tags = promptCommitTags config commitType
                    ShortMessage = promptShortMessage ()
                    Description = promptDescription ()
                    // Breaking change are only allowed for certain commit types?
                    IsBreakingChange = promptIsBreakingChange ()
                }
                |> generateCommitMessage

            // Ask confirmation before shipping the commit
            if not settings.SkipConfirmation then
                // User rejected the commit
                if not (promptCommitConfirmation commitMessageText) then
                    exit 1

            // Write the commit message to the file
            File.WriteAllText(settings.CommitFile, commitMessageText)

            0
