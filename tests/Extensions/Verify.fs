module Verify.Extensions

open VerifyTests
open Workspace
open VerifyFixie
open Argon

let verifySettings =
    let settings = new VerifySettings()
    settings.UseDirectory(Workspace.snapshots.``.``)
    settings.AddExtraSettings(fun settings -> settings.AddFSharpConverters())
    settings

type VerifierExt =
    static member Verify(text: string) =
        Verifier.Verify(text, verifySettings).ToTask()

    static member Verify(o: obj) =
        Verifier.Verify(o, verifySettings).ToTask()
