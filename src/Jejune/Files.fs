namespace Jejune

module Files =
    open System.IO
    open System.Text.RegularExpressions

    type PathChunk = 
        | Name of string
        | Var of (string * string) 
    

    let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
        else None
 
    let getChunk chunkstr =
        match chunkstr with
        | Regex @"^\{.*\}$" [ var; ] -> Var (var, chunkstr)  //Select string with pattern "{variable-name}"
        | _ -> Name chunkstr

    let splitChunks (filename: string) = 
        let directories = filename.Split Path.DirectorySeparatorChar 
        directories

    let rec allFiles dirs =
        
        if Seq.isEmpty dirs then Seq.empty else
        seq { yield! dirs |> Seq.collect Directory.EnumerateFiles
              yield! dirs |> Seq.collect Directory.EnumerateDirectories |> allFiles } 
   
    let getAllTemplates dirs = 
        //[Path.Combine [|__SOURCE_DIRECTORY__; "test"|]]        
        seq {
            seq { Var ("level1", "{level1}"); Var ("filename", "{filename}.json") }
            seq { Name "testdata.json"}
        }

module Tests = 
    open NUnit.Framework
    open Files
    open System.IO
    
    [<Test>]
    let ``Get all files recursively in test directory``() = 
               
        let files = allFiles [Path.Combine [|__SOURCE_DIRECTORY__; "test"|]]        
        Assert.AreEqual(2, Seq.length files)


    [<Test>]
    let ``Get local templates with expansion variables in path names``() = 
        let res = Seq.toArray (getAllTemplates [Path.Combine [|__SOURCE_DIRECTORY__; "test"|]])

        Assert.AreEqual([| Var ("level1", "{level1}"); Var ("filename", "{filename}.json")|],  res.[0])
        Assert.AreEqual([| Name "testdata.json"|],  res.[1])
        