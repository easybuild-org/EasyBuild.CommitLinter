namespace CommitLinter.Commands.Interactive

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

// [<RequireQualifiedAccess>]
// module internal Prompt =

//     let commitType (console: IAnsiConsole) (config: CommitParserConfig) =
//         console.Clear()

//         // Helper functions to convert between CommitType and string
//         let commitTypeToChoice (commitType: CommitType) =
//             match commitType.Description with
//             | Some description -> $"%s{commitType.Name} [grey]%s{description}[/]"
//             | None -> $"%s{commitType.Name}"

//         // Helper function to convert (back) from a choice string to CommitType
//         let choiceToCommitType (choice: string) =
//             config.Types
//             |> List.find (fun commitType -> commitTypeToChoice commitType = choice)

//         let prompt = SelectionPrompt<string>(Title = "Select the type of commit")

//         let choices = config.Types |> List.map commitTypeToChoice |> Array.ofList

//         choices |> prompt.AddChoices |> console.Prompt |> choiceToCommitType

//     let commitTags (console: IAnsiConsole) (config: CommitParserConfig) (commitType: CommitType) =
//         console.Clear()

//         if commitType.SkipTagLine then
//             None
//         else
//             match config.Tags with
//             | None ->
//                 console.MarkupLine("[red]Error:[/] No tags defined in the configuration file.")
//                 exit 1

//             | Some tags ->
//                 let instructionsText =
//                     [
//                         "[grey](Multiple tags can be selected)[/]"
//                         "[grey](Press [blue]<up>[/] and [blue]<down>[/] to navigate, [blue]<space>[/] to toggle a tag, [green]<enter>[/] to accept)[/]"
//                     ]
//                     |> String.concat "\n"

//                 let prompt =
//                     MultiSelectionPrompt<string>(
//                         Title = "Select tags",
//                         InstructionsText = instructionsText
//                     )

//                 tags |> prompt.AddChoices |> console.Prompt |> Seq.toList |> Some

//     let shortMessage (console: IAnsiConsole) =
//         console.Clear()

//         TextPrompt<string>("Short message:") |> console.Prompt

//     let description (console: IAnsiConsole) =
//         let description =
//             TextPrompt<string>("Long description (optional): ", AllowEmpty = true)
//             |> console.Prompt

//         if String.IsNullOrWhiteSpace description then
//             None
//         else
//             Some description

//     let isBreakingChange (console: IAnsiConsole) =
//         console.Clear()

//         let prompt = ConfirmationPrompt("Is this a breaking change?", DefaultValue = false)

//         prompt |> console.Prompt

//     let commitConfirmation (console: IAnsiConsole) (commitMessage: string) =
//         console.Clear()

//         let headerRule = Rule("Commit message preview")
//         headerRule.Justification <- Justify.Left

//         console.Write(headerRule)

//         commitMessage |> StringExtensions.EscapeMarkup |> console.MarkupLine

//         console.Write(Rule())

//         ConfirmationPrompt("Submit the commit?") |> console.Prompt

// type CommitMessageConfig =
//     {
//         mutable CommitType: CommitType
//         mutable Tags: string list option
//         mutable ShortMessage: string
//         mutable Description: string option
//         mutable IsBreakingChange: bool
//     }

// let internal generateCommitMessage (config: CommitMessageConfig) =
//     let breakingChangeSuffix =
//         if config.IsBreakingChange then
//             "!"
//         else
//             ""

//     let inline newLine () = "\n"

//     [
//         $"%s{config.CommitType.Name}%s{breakingChangeSuffix}: %s{config.ShortMessage}"
//         newLine () // Empty line

//         // Tags:
//         // [tag1][tag2][tag3]
//         match config.Tags with
//         | Some tags ->
//             newLine ()
//             tags |> List.map (fun tag -> $"[%s{tag}]") |> String.concat ""
//             newLine ()
//         | None -> ""

//         match config.Description with
//         | Some longDescription ->
//             newLine ()
//             longDescription
//             newLine ()
//         | None -> ""
//     ]
//     |> String.concat ""

// let internal promptCommitMessage (console: IAnsiConsole) (commitConfig: CommitParserConfig) =
//     let commitType = Prompt.commitType console commitConfig

//     {
//         CommitType = commitType
//         Tags = Prompt.commitTags console commitConfig commitType
//         ShortMessage = Prompt.shortMessage console
//         Description = Prompt.description console
//         // Breaking change are only allowed for certain commit types?
//         IsBreakingChange = Prompt.isBreakingChange console
//     }

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Controls.Primitives

// [<RequireQualifiedAccess>]
// module AppState =

//     let commitMessageConfig =
//         {
//             CommitType = Unchecked.defaultof<CommitType>
//             Tags = Some [ "converter"; "web" ]
//             ShortMessage = "Add new feature"
//             Description = Some "This is a new feature"
//             IsBreakingChange = false
//         }

// module Main =

//     type FakeDataItem = { Name: string; Description: string }

//     let fieldWithLabel (label: string) column (field: #Types.IView) =
//         StackPanel.create
//             [
//                 StackPanel.orientation Orientation.Vertical
//                 StackPanel.spacing 5
//                 StackPanel.children [ TextBlock.create [ TextBlock.text label ]; field ]
//                 Grid.column column
//             ]

//     let chooseType (config: CommitParserConfig) =
//         let types = config.Types |> List.map (fun commitType -> commitType.Name)

//         ComboBox.create
//             [
//                 ComboBox.dataItems types
//                 ComboBox.horizontalAlignment HorizontalAlignment.Stretch
//                 ComboBox.classes [ "commit-type" ]
//                 ComboBox.selectedIndex 0
//             ]
//         |> fieldWithLabel "Type" 0

//     let view (config: CommitParserConfig) =
//         Component(fun ctx ->
//             let state = ctx.useState 0

//             Grid.create
//                 [
//                     Grid.margin (Thickness 20)
//                     Grid.columnDefinitions "200, *"
//                     Grid.children
//                         [
//                             chooseType config

//                             // TextBox.create [ ] |> fieldWithLabel "Short message" 1

//                             Grid.create
//                                 [
//                                     Grid.column 1
//                                     Grid.rowDefinitions "auto,auto,*,auto, auto"
//                                     Grid.children
//                                         [
//                                             TextBox.create [ Grid.row 0 ]
//                                             // ScrollViewer.create
//                                             //     [
//                                             //         Grid.row 1
//                                             //         ScrollViewer.horizontalScrollBarVisibility
//                                             //             ScrollBarVisibility.Auto
//                                             //         ScrollViewer.verticalScrollBarVisibility
//                                             //             ScrollBarVisibility.Auto
//                                             //         ScrollViewer.maxHeight 200.0
//                                             //         ScrollViewer.content (

//                                             //         )
//                                             //     ]
//                                             WrapPanel.create
//                                                 [
//                                                     Grid.row 1
//                                                     WrapPanel.orientation Orientation.Horizontal
//                                                     WrapPanel.maxHeight 200.0
//                                                     WrapPanel.children
//                                                         [
//                                                             CheckBox.create
//                                                                 [

//                                                                     CheckBox.margin (
//                                                                         Thickness(
//                                                                             0,
//                                                                             0,
//                                                                             5,
//                                                                             0
//                                                                         )
//                                                                     )
//                                                                     CheckBox.content
//                                                                         $"converter"
//                                                                 ]
//                                                             CheckBox.create
//                                                                 [

//                                                                     CheckBox.margin (
//                                                                         Thickness(
//                                                                             0,
//                                                                             0,
//                                                                             5,
//                                                                             0
//                                                                         )
//                                                                     )
//                                                                     CheckBox.content
//                                                                         $"web"
//                                                                 ]
//                                                         ]
//                                                 ]
//                                             TextBox.create
//                                                 [
//                                                     Grid.row 2
//                                                     TextBox.acceptsReturn true
//                                                     TextBox.textWrapping
//                                                         TextWrapping.WrapWithOverflow
//                                                 ]
//                                             CheckBox.create
//                                                 [
//                                                     Grid.row 3
//                                                     CheckBox.isChecked false
//                                                     CheckBox.content "Is breaking change?"
//                                                 ]
//                                             StackPanel.create [
//                                                 Grid.row 4
//                                                 StackPanel.children [
//                                                     Button.create [
//                                                         Button.content "Submit"
//                                                         Button.onClick (fun _ ->
//                                                             printfn "Submit"
//                                                         )
//                                                     ]
//                                                 ]
//                                             ]
//                                         ]
//                                 ]
//                         ]
//                 ]
//         )

// type MainWindow(config: CommitParserConfig) as this =
//     inherit HostWindow()

//     do
//         base.Title <- "Commit editor"
//         base.Content <- Main.view config
//         base.Width <- 1024.0
//         base.Height <- 768.0
//         base.CanResize <- false

// #if DEBUG
//         this.AttachDevTools()
// #endif

// type App(config: CommitParserConfig) =
//     inherit Application()

//     override this.Initialize() =
//         this.Styles.Add(FluentTheme())
//         this.Styles.Load "avares://EasyBuild.CommitLinter/Styles.xaml"

//     // this.RequestedThemeVariant <- Styling.ThemeVariant.Default

//     override this.OnFrameworkInitializationCompleted() =
//         match this.ApplicationLifetime with
//         | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
//             desktopLifetime.MainWindow <- MainWindow config
//         | _ -> ()

type InteractiveCommand() =
    inherit Command<InteractiveSettings>()

    interface ICommandLimiter<InteractiveSettings>

    override __.Execute(context, settings) =
        match tryLoadConfig settings.Config with
        | LoadConfig.Failed -> 1
        | LoadConfig.Success config ->
            if List.isEmpty config.Types then
                AnsiConsole.MarkupLine(
                    $"[red]Error:[/] No commit types defined in the configuration file."
                )

                exit 1
            else
                // Initialize the commit message config with the first commit type
                // AppState.commitMessageConfig.CommitType <- config.Types |> List.head

                AppBuilder
                    .Configure(fun _ -> App config)
                    .UsePlatformDetect()
                    .UseSkia()
                    .StartWithClassicDesktopLifetime([||])
                |> ignore

                // // Write the commit message to the file
                // File.WriteAllText(settings.CommitFile, commitMessageText)

                0
