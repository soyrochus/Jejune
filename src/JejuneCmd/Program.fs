// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Jejune

// Define a function to construct a message to print
let from whom =
    sprintf "from %s" whom

[<EntryPoint>]
let main argv =
    
    let result = Jejune.Template.test

    printfn "Generated:  %s" result
    0 // return an integer exit code