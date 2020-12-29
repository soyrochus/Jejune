namespace Jejune

module Files =
    open System.IO
    open System.Text.RegularExpressions
    open System.Collections.Generic

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

    // let createFile

    let getAllTemplates rootdir = 
        
        allFiles [rootdir] |> Seq.choose (fun f -> match f with
                                                   | Ext ".hbs" f -> Some f
                                                   | _ -> None) 
                                                   |> Seq.map (fun e -> getTemplatePath rootdir e )

    open FSharp.Data    
    let deserialize<'a> str =
        try
          JsonValue.Parse(str)  
          //JsonConvert.DeserializeObject<'a> str
          |> Result.Ok
        with
          // catch all exceptions and convert to Result
          | ex -> Result.Error ex  
    
    let getDataObject(path) = 
        use file = File.OpenText(path)
        deserialize(file.ReadToEnd())

    let getChildObject (chunks: PathChunk seq) (data: JsonValue) = 
        let lst = List.ofSeq chunks
        let rec child (l : PathChunk list) (data: JsonValue) =             
            match l.Head with
            | FileVar (f, n) -> data.[f]
            | DirVar n -> child l.Tail data.[n]
            | Name n -> child l.Tail data
        child lst data

    let rec jsonArray (arr : JsonValue array):  obj array  = 
        let s : (obj) seq = seq {
            for e in arr do match e with              
                                        | JsonValue.Null -> yield null
                                        | JsonValue.Boolean b -> yield b
                                        | JsonValue.String s -> yield s
                                        | JsonValue.Float f -> yield f
                                        | JsonValue.Number n -> yield n
                                        | JsonValue.Array a -> yield jsonArray a :> obj              
                                        | JsonValue.Record r -> yield jsonRecord r :> obj
        }
        s |> Seq.toArray

    and jsonRecord (record : (string * JsonValue) array): IDictionary<string, obj>  = 

        let s : (string * obj) seq = seq {
                for n, v in record do match v with              
                                        | JsonValue.Null -> yield (n, null)
                                        | JsonValue.Boolean b -> yield (n, b :> obj)
                                        | JsonValue.String s -> yield (n, s :> obj)
                                        | JsonValue.Float f -> yield (n, f :> obj)
                                        | JsonValue.Number d -> yield (n, d :> obj)
                                        | JsonValue.Array a -> yield (n, jsonArray a :> obj)              
                                        | JsonValue.Record r ->yield  (n, jsonRecord r :> obj)                   
        } 
        s |> dict

    let jsonpropsToDict (jsonvalue: JsonValue) : IDictionary<string, obj>= 
        
        match jsonvalue with 
            | JsonValue.Record r -> jsonRecord (jsonvalue.Properties())
            | _ -> failwith "Data can only be an JsonObject"
        
    module Tests =  
        open NUnit.Framework        
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
            
            Assert.AreEqual([DirVar "level1"; FileVar ("level2", ".txt")], chunks)
            
        [<Test>]
        let ``Get json deserialized objects`` () =
            let root = Path.Combine [|__SOURCE_DIRECTORY__; "test"; "testdata.json" |]

            let ob = match getDataObject root with       
                        | Ok o -> o
                        | Error e -> failwith e.Message    
          
            Assert.AreEqual(10, ob.["int"].AsInteger())
            let ob2 = getChildObject [|DirVar "level1"; Name "skip"; FileVar ("level2", ".txt") |] ob
            Assert.AreEqual(1000, ob2.["int"].AsInteger())


