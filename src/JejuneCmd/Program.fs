// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open HandlebarsDotNet
// Define a function to construct a message to print

open JejuneCmd
open Gen
open System.IO
    

[<EntryPoint>]
let main argv =
    
    let entities_path = @"C:\src\CASO-Backend\source\Templates\WebAPI\Devon4Net.WebAPI.Implementation\entities"
    
    let entities = load_entities(Path.Combine(entities_path, "entities.txt")) 
    copyAndExpandFiles entities entities_path @"C:\src\CASO-Backend\source\Templates\WebAPI\Devon4Net.WebAPI.Implementation\template" @"C:\src\CASO-Backend\source\Templates\WebAPI\Devon4Net.WebAPI.Implementation"
    
    0
    
    