module Tests.Setup

open Fixie
open System
open System.Collections.Generic
open System.Reflection

[<AttributeUsage(AttributeTargets.Method)>]
type TestAttribute() =
    inherit Attribute()

type TestModuleDiscovery() =
    interface IDiscovery with
        member _.TestClasses(concreteClasses: IEnumerable<Type>) =
            concreteClasses
            |> Seq.filter (fun cls ->
                cls.GetMembers() |> Seq.exists (fun m -> m.Has<TestAttribute>())
            )

        member _.TestMethods(publicMethods: IEnumerable<MethodInfo>) =
            publicMethods |> Seq.filter (fun x -> x.Has<TestAttribute>() && x.IsStatic)

type TestProject() =
    interface ITestProject with
        member _.Configure(configuration: TestConfiguration, environment: TestEnvironment) =
            VerifyTests.VerifierSettings.AssignTargetAssembly(environment.Assembly)
            configuration.Conventions.Add<TestModuleDiscovery, TestProject>()

    interface IExecution with
        member _.Run(testSuite: TestSuite) =
            task {
                for testClass in testSuite.TestClasses do
                    for test in testClass.Tests do
                        use _ = VerifyFixie.ExecutionState.Set(testClass, test, null)
                        let! _ = test.Run()
                        ()
            }
