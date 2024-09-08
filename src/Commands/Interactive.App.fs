namespace CommitLinter.Commands.Interactive

open System
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

module Elmish =

    type FormValues =
        {
            CommitTypeIndex: int
            ShortMessage: string
            Description: string
            Tags: string list
            IsBreakingChange: bool
        }

    type FormErrors =
        {
            ShortMessage: string option
            Tags: string option
        }

        member this.HasErrors = this.ShortMessage.IsSome || this.Tags.IsSome

    type Model =
        {
            Config: CommitParserConfig
            FormValues: FormValues
            FormErrors: FormErrors
        }

    type Msg =
        | ChangeCommitTypeIndex of int
        | ChangeShortDescription of string
        | WriteCommit

    let init (config: CommitParserConfig) =
        {
            Config = config
            FormValues =
                {
                    CommitTypeIndex = 0
                    ShortMessage = ""
                    Description = ""
                    Tags = []
                    IsBreakingChange = false
                }
            FormErrors =
                {
                    ShortMessage = None
                    Tags = None
                }
        },
        Cmd.none

    let private validateForm (model: Model) =
        let errors =
            {
                ShortMessage =
                    if String.IsNullOrWhiteSpace model.FormValues.ShortMessage then
                        Some "This field is required"
                    else
                        None
                Tags = None
            }

        { model with
            FormErrors = errors
        }

    let update msg model =
        match msg with
        | ChangeCommitTypeIndex index ->
            { model with
                FormValues =
                    { model.FormValues with
                        CommitTypeIndex = index
                    }
            },
            Cmd.none

        | ChangeShortDescription shortDescription ->
            printfn "Short description: %s" shortDescription

            { model with
                FormValues =
                    { model.FormValues with
                        ShortMessage = shortDescription
                    }
            }
            |> validateForm,
            Cmd.none

        | WriteCommit ->
            let validatedModel = validateForm model

            if validatedModel.FormErrors.HasErrors then
                validatedModel, Cmd.none
            else
                validatedModel, Cmd.none

    let fieldLabel text =
        TextBlock.create [
            TextBlock.text text
        ]

    let errorMessage text =
        TextBlock.create [
            TextBlock.text text
            TextBlock.foreground Brushes.Red
        ]

    let withLabelAndError
        (label: string)
        (errorOpt: string option)
        (content: Types.IView)
        : Types.IView
        =
        StackPanel.create [
            StackPanel.orientation Orientation.Vertical
            StackPanel.spacing 5
            StackPanel.children [
                fieldLabel label
                content
                match errorOpt with
                | Some text -> errorMessage text
                | None -> TextBlock.create [] // Preserve layout
            ]
        ]

    let commitTypeField (model: Model) (dispatch: Dispatch<Msg>) =
        ComboBox.create [

            model.Config.Types |> List.map _.Name |> ComboBox.dataItems

            ComboBox.horizontalAlignment HorizontalAlignment.Stretch
            ComboBox.classes [
                "commit-type"
            ]
            ComboBox.selectedIndex model.FormValues.CommitTypeIndex
            ComboBox.onSelectedIndexChanged (ChangeCommitTypeIndex >> dispatch)
        ]
        |> withLabelAndError "Commit type" None

    let shortDescriptionField
        (shortDescription: string)
        (errorOpt: string option)
        (dispatch: Dispatch<Msg>)
        =
        TextBox.create [
            TextBox.text shortDescription
            TextBox.horizontalAlignment HorizontalAlignment.Stretch
            TextBox.classes [
                "short-description"
            ]
            TextBox.onTextChanged (ChangeShortDescription >> dispatch)
        ]
        |> withLabelAndError "Short description" errorOpt

    let view (model: Model) (dispatch: Dispatch<Msg>) =
        Grid.create [
            Grid.margin (Thickness 20)
            Grid.columnDefinitions "200, *"
            Grid.children [
                Panel.create [
                    Grid.column 0
                    Panel.children [
                        commitTypeField model dispatch
                    ]
                ]

                Panel.create [
                    Grid.column 1
                    Panel.children [
                        shortDescriptionField
                            model.FormValues.ShortMessage
                            model.FormErrors.ShortMessage
                            dispatch
                    ]
                ]

                Button.create [
                    Button.content (
                        TextBlock.create [
                            TextBlock.text "Write commit"
                        ]
                    )
                    Button.horizontalAlignment HorizontalAlignment.Right
                    Button.onClick (fun _ -> WriteCommit |> dispatch)
                ]
            ]
        ]

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
        // |> Program.withConsoleTrace
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

// [<RequireQualifiedAccess>]
// module AppState =

//     let commitMessageConfig =
//         {
//             CommitType = Unchecked.defaultof<CommitType>
//             Tags =
//                 Some [
//                     "converter"
//                     "web"
//                 ]
//             ShortMessage = "Add new feature"
//             Description = Some "This is a new feature"
//             IsBreakingChange = false
//         }

module Main =

    type FakeDataItem =
        {
            Name: string
            Description: string
        }

    let fieldWithLabel (label: string) column (field: #Types.IView) =
        StackPanel.create [
            StackPanel.orientation Orientation.Vertical
            StackPanel.spacing 5
            StackPanel.children [
                TextBlock.create [
                    TextBlock.text label
                ]
                field
            ]
            Grid.column column
        ]

    let chooseType (config: CommitParserConfig) =
        let types = config.Types |> List.map (fun commitType -> commitType.Name)

        ComboBox.create [
            ComboBox.dataItems types
            ComboBox.horizontalAlignment HorizontalAlignment.Stretch
            ComboBox.classes [
                "commit-type"
            ]
            ComboBox.selectedIndex 0
        ]
        |> fieldWithLabel "Type" 0

    let view (config: CommitParserConfig) =
        Component(fun ctx ->
            let state = ctx.useState 0

            Grid.create [
                Grid.margin (Thickness 20)
                Grid.columnDefinitions "200, *"
                Grid.children [
                    chooseType config

                    // TextBox.create [ ] |> fieldWithLabel "Short message" 1

                    Grid.create [
                        Grid.column 1
                        Grid.rowDefinitions "auto,auto,*,auto, auto"
                        Grid.children [
                            TextBox.create [
                                Grid.row 0
                            ]
                            // ScrollViewer.create
                            //     [
                            //         Grid.row 1
                            //         ScrollViewer.horizontalScrollBarVisibility
                            //             ScrollBarVisibility.Auto
                            //         ScrollViewer.verticalScrollBarVisibility
                            //             ScrollBarVisibility.Auto
                            //         ScrollViewer.maxHeight 200.0
                            //         ScrollViewer.content (

                            //         )
                            //     ]
                            WrapPanel.create [
                                Grid.row 1
                                WrapPanel.orientation Orientation.Horizontal
                                WrapPanel.maxHeight 200.0
                                WrapPanel.children [
                                    CheckBox.create [

                                        CheckBox.margin (Thickness(0, 0, 5, 0))
                                        CheckBox.content $"converter"
                                    ]
                                    CheckBox.create [

                                        CheckBox.margin (Thickness(0, 0, 5, 0))
                                        CheckBox.content $"web"
                                    ]
                                ]
                            ]
                            TextBox.create [
                                Grid.row 2
                                TextBox.acceptsReturn true
                                TextBox.textWrapping TextWrapping.WrapWithOverflow
                            ]
                            CheckBox.create [
                                Grid.row 3
                                CheckBox.isChecked false
                                CheckBox.content "Is breaking change?"
                            ]
                            StackPanel.create [
                                StackPanel.children [
                                    Button.create [
                                        Button.content "Submit"
                                        Button.onClick (fun _ -> printfn "Submit")
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        )
