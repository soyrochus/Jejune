namespace Jejune

module Files =
    open System.IO
    open System.Text.RegularExpressions

    type PathChunk = 
        | Name of string
        | DirVar of string 
        | FileVar of (string * string) 
    
    type TemplatePath = (string * seq<PathChunk>)

    let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
        else None
 
    let getChunk chunkstr =
        match chunkstr with
        | Regex @"^\{(.*)}(.*)\.hbs$" [ name; ext] -> FileVar (name, ext)  //Select string with pattern "{variable-name}.{ext}.hbs"
        | Regex @"^\{(.*)}$" [ name ] -> DirVar name //Select string with pattern "{variable-name}"
        | _ -> Name chunkstr

    let splitChunks (filename: string) = 
        filename.Split Path.DirectorySeparatorChar |> Array.map getChunk 

    let rec allFiles dirs =        
        if Seq.isEmpty dirs then Seq.empty else
        seq { yield! dirs |> Seq.collect Directory.EnumerateFiles
              yield! dirs |> Seq.collect Directory.EnumerateDirectories |> allFiles } 
   
    let (|Ext|_|) (ext: string) (input: string) = 
        if Path.GetExtension input = ext then Some input
        else 
            None
    
    let getTemplatePath rootpath filepath = 
        let right = Path.GetRelativePath(rootpath, filepath)
        (filepath, splitChunks right)
         
    let getAllTemplates rootdir = 
        
        allFiles [rootdir] |> Seq.choose (fun f -> match f with
                                                   | Ext ".hbs" f -> Some f
                                                   | _ -> None) 
                                                   |> Seq.map (fun e -> getTemplatePath rootdir e )

   
        //seq {
        //    seq { DirVar "level1"; FileVar ("filename", ".json") }
        //    seq { Name "testdata.json"}
        //}

module Tests = 
    open NUnit.Framework
    open Files
    open System.IO
    
    [<Test>]
    let ``Get all files recursively in test directory``() = 
               
        let root = Path.Combine [|__SOURCE_DIRECTORY__; "test"|]
        let files = allFiles [ root ]        
        Assert.AreEqual(4, Seq.length files)

        let templates = getAllTemplates root 
        Assert.AreEqual(2, Seq.length templates)

    [<Test>]
    let ``Get chunks from strings``() = 
        
        Assert.AreEqual(DirVar "field", getChunk @"{field}")
        Assert.AreEqual(FileVar ("field", ".cs"), getChunk @"{field}.cs.hbs")
        

    [<Test>]
    let ``Get local templates with expansion variables in path names``() = 

        let root = Path.Combine [|__SOURCE_DIRECTORY__; "test"|]
         
        let templates = getAllTemplates root 

        let templarr = Seq.toArray templates 
        let file, chunks = templarr.[1]
        
        Assert.AreEqual([DirVar "level1"; FileVar ("filename", ".json")], chunks)
        