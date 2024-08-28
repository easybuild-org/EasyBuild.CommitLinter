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

[<RequireQualifiedAccess>]
module internal Prompt =

    let commitType (console: IAnsiConsole) (config: CommitParserConfig) =
        console.Clear()

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

        choices |> prompt.AddChoices |> console.Prompt |> choiceToCommitType

    let commitTags (console: IAnsiConsole) (config: CommitParserConfig) (commitType: CommitType) =
        console.Clear()

        if commitType.SkipTagLine then
            None
        else
            match config.Tags with
            | None ->
                console.MarkupLine("[red]Error:[/] No tags defined in the configuration file.")
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

                tags |> prompt.AddChoices |> console.Prompt |> Seq.toList |> Some

    let shortMessage (console: IAnsiConsole) =
        console.Clear()

        TextPrompt<string>("Short message:") |> console.Prompt

    let description (console: IAnsiConsole) =
        let description =
            TextPrompt<string>("Long description (optional): ", AllowEmpty = true)
            |> console.Prompt

        if String.IsNullOrWhiteSpace description then
            None
        else
            Some description

    let isBreakingChange (console: IAnsiConsole) =
        console.Clear()

        let prompt = ConfirmationPrompt("Is this a breaking change?", DefaultValue = false)

        prompt |> console.Prompt

    let commitConfirmation (console: IAnsiConsole) (commitMessage: string) =
        console.Clear()

        let headerRule = Rule("Commit message preview")
        headerRule.Justification <- Justify.Left

        console.Write(headerRule)

        commitMessage |> StringExtensions.EscapeMarkup |> console.MarkupLine

        console.Write(Rule())

        ConfirmationPrompt("Submit the commit?") |> console.Prompt

type CommitMessageConfig =
    {
        CommitType: CommitType
        Tags: string list option
        ShortMessage: string
        Description: string option
        IsBreakingChange: bool
    }

let internal generateCommitMessage (config: CommitMessageConfig) =
    let breakingChangeSuffix =
        if config.IsBreakingChange then
            "!"
        else
            ""

    let inline newLine () = "\n"

    [
        $"%s{config.CommitType.Name}%s{breakingChangeSuffix}: %s{config.ShortMessage}"
        newLine () // Empty line

        // Tags:
        // [tag1][tag2][tag3]
        match config.Tags with
        | Some tags ->
            newLine ()
            tags |> List.map (fun tag -> $"[%s{tag}]") |> String.concat ""
            newLine ()
        | None -> ""

        match config.Description with
        | Some longDescription ->
            newLine ()
            longDescription
            newLine ()
        | None -> ""
    ]
    |> String.concat ""

let internal promptCommitMessage (console: IAnsiConsole) (commitConfig: CommitParserConfig) =
    let commitType = Prompt.commitType console commitConfig

    {
        CommitType = commitType
        Tags = Prompt.commitTags console commitConfig commitType
        ShortMessage = Prompt.shortMessage console
        Description = Prompt.description console
        // Breaking change are only allowed for certain commit types?
        IsBreakingChange = Prompt.isBreakingChange console
    }

open Terminal.Gui

type InteractiveWindow() as this =
    inherit Window()

    do
        this.Title <- $"Commit editor (%O{Application.QuitKey} to quit)"

        let userNameLabel = new Label(Text = "Username: ")

        let input =
            new TextView(
                Y = Pos.Bottom(userNameLabel) + Pos.op_Implicit 1,
                Text =
                    """Line 1
Line 2""",
                Width = Dim.Fill(),
                Height = Dim.op_Implicit 10
            )

        input.ReadOnly <- true

        // input.ba

        let exitButton =
            new Button(Text = "Exit", Y = Pos.Bottom(input) + Pos.op_Implicit 1)

        exitButton.Accept.Add(fun _ -> Application.RequestStop())

        this.Add(userNameLabel, input, exitButton)

type InteractiveCommand() =
    inherit Command<InteractiveSettings>()

    interface ICommandLimiter<InteractiveSettings>

    override __.Execute(context, settings) =
        // printfn "%A" settings.CommitFile

        // match tryLoadConfig settings.Config with
        // | LoadConfig.Failed -> 1
        // | LoadConfig.Success config ->
        //     let console = AnsiConsole.Console

        //     let commitMessageText = promptCommitMessage console config |> generateCommitMessage

        //     // Ask confirmation before shipping the commit
        //     if not settings.SkipConfirmation then
        //         // User rejected the commit
        //         if not (Prompt.commitConfirmation console commitMessageText) then
        //             console.WriteLine("[red]Commit aborted[/]")
        //             exit 1

        //     // Write the commit message to the file
        //     File.WriteAllText(settings.CommitFile, commitMessageText)

        //     0

        // Application.QuitKey <- Key.C

        Application.Init()
        Application.Run<InteractiveWindow>() |> ignore
        Application.Shutdown()

        0
