// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open HandlebarsDotNet
// Define a function to construct a message to print

open JejuneCmd
open Gen
    

[<EntryPoint>]
let main argv =
    
    copyAndExpandFiles ["Test"] @"C:\src\CASO-Backend\source\Templates\WebAPI\Devon4Net.WebAPI.Implementation\template" @"C:\src\CASO-Backend\source\Templates\WebAPI\Devon4Net.WebAPI.Implementation"
    (*if argv.Length <> 3 then
        printf "Invalid number of arguments"
        printf "entity_file template_path destination_path"
        -1
    else
        let entityfile = argv.[0]
        let frompath = argv.[1]
        let topath = argv.[2]
        //copyAndExpandFiles "test" frompath topath
        copyAndExpandFiles ["Test"] @"C:\src\CASO-Backend\source\Templates\WebAPI\Devon4Net.WebAPI.Implementation\template" @"c:/temp"
        0
    *)
    0
    
    