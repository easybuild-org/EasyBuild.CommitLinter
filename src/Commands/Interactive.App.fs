namespace CommitLinter.Commands.Interactive

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Elmish
open Avalonia.FuncUI
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Controls.Primitives
open EasyBuild.CommitParser.Types

module Prelude =
    open System
    open System.Text.Json
    open System.Text.Json.Serialization

    let transferState<'t> oldState =
        let jsonOptions = JsonSerializerOptions()
        jsonOptions.Converters.Add(JsonFSharpConverter())

        try
            let json = JsonSerializer.Serialize(oldState, jsonOptions)
            let state = JsonSerializer.Deserialize<'t>(json, jsonOptions)

            match box state with
            | null -> None
            | _ -> Some state
        with ex ->
            Console.Write $"Error restoring state: {ex}"
            None

module Elmish =

    type FormValues =
        {
            CommitType: CommitType
            ShortMessage: string
            Description: string
            Tags: string list
            IsBreakingChange: bool
        }

    type Model =
        {
            Config: CommitParserConfig
            FormValues: FormValues
        }

    type Msg =
        | Increment
        | Decrement

    let init (config: CommitParserConfig) =
        {
            Config = config
            FormValues =
                {
                    CommitType = config.Types |> List.head
                    ShortMessage = ""
                    Description = ""
                    Tags = []
                    IsBreakingChange = false
                }
        },
        Cmd.none

    let update msg model = model, Cmd.none

    let fieldLabel text =
        TextBlock.create [ TextBlock.text text ]

    let errorMessage text =
        TextBlock.create [ TextBlock.text text; TextBlock.foreground Brushes.Red ]

    let withLabelAndError label error content =
        StackPanel.create
            [
                StackPanel.orientation Orientation.Vertical
                StackPanel.spacing 5
                StackPanel.children [ fieldLabel label; content; errorMessage error ]
            ]

    let view (model: Model) (dispatch: Dispatch<Msg>) =
        ComboBox.create
            [

                model.Config.Types |> List.map _.Name |> ComboBox.dataItems

                ComboBox.horizontalAlignment HorizontalAlignment.Stretch
                ComboBox.classes [ "commit-type" ]
                ComboBox.selectedIndex 0
            ]
        |> withLabelAndError "Commit type" "This field is required"

type MainWindow(config: CommitParserConfig) as this =
    inherit HostWindow()

    do
        base.Title <- "Commit editor"
        // base.Content <- Main.view config
        base.Width <- 1024.0
        base.Height <- 768.0
        base.CanResize <- false

#if DEBUG
        this.AttachDevTools()
#endif

        Program.mkProgram Elmish.init Elmish.update Elmish.view
        |> Program.withHost this
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.runWith config

type App(config: CommitParserConfig) =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add(FluentTheme())
        this.Styles.Load "avares://EasyBuild.CommitLinter/Styles.xaml"

    // this.RequestedThemeVariant <- Styling.ThemeVariant.Default

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- MainWindow config
        | _ -> ()
