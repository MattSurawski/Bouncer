open Mono.Cecil
open System
open System.IO
open Mono.Cecil.Cil
open System

// Based off of https://stackoverflow.com/questions/39366963/how-to-use-mono-cecil-to-create-helloworld-exe?noredirect=1&lq=1

[<EntryPoint>]
let main argv =
    if argv.Length = 0 then
        printfn "Usage: bouncerc file1.bouncer"
        System.Console.ReadKey() |> ignore
        1
    else
        let infiles = argv
        let programName = Path.GetFileNameWithoutExtension infiles.[0]
        let outputApp = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(programName, new Version(1, 0, 0, 0)), programName, ModuleKind.Console)
        let outputAppModule = outputApp.MainModule

        let programType = new TypeDefinition(programName, "Program", TypeAttributes.Class ||| TypeAttributes.Public, outputAppModule.TypeSystem.Object)
        outputAppModule.Types.Add programType

        let ctor = new MethodDefinition(".ctor", MethodAttributes.Public ||| MethodAttributes.HideBySig ||| MethodAttributes.SpecialName ||| MethodAttributes.RTSpecialName, outputAppModule.TypeSystem.Void)
        let ctorIl = ctor.Body.GetILProcessor()
        ctorIl.Append(ctorIl.Create(OpCodes.Ldarg_0))
        ctorIl.Append(ctorIl.Create(OpCodes.Call, outputAppModule.ImportReference(typeof<Object>.GetConstructor(Array.Empty<Type>()))))
        ctorIl.Append(ctorIl.Create(OpCodes.Nop))
        ctorIl.Append(ctorIl.Create(OpCodes.Ret))
        programType.Methods.Add(ctor)

        let mainMethod = new MethodDefinition("Main", MethodAttributes.Public ||| MethodAttributes.Static, outputAppModule.TypeSystem.Void)
        let argsParameter = new ParameterDefinition("args", ParameterAttributes.None, outputAppModule.ImportReference(typeof<string[]>))
        mainMethod.Parameters.Add argsParameter
        let mainIl = mainMethod.Body.GetILProcessor()
        mainIl.Append(mainIl.Create(OpCodes.Nop))
        mainIl.Append(mainIl.Create(OpCodes.Ldstr, "Hello World!"))
        let writeLineMethod = mainIl.Create(OpCodes.Call, outputAppModule.ImportReference(typeof<Console>.GetMethod("WriteLine", [| typeof<string> |])))
        mainIl.Append(writeLineMethod)
        mainIl.Append(mainIl.Create(OpCodes.Nop))
        mainIl.Append(mainIl.Create(OpCodes.Ret))
        programType.Methods.Add mainMethod

        outputApp.EntryPoint <- mainMethod

        outputApp.Write(programName + ".exe")

        System.Console.ReadKey() |> ignore
        0
