module JejuneCmd.Gen
open System.IO
open HandlebarsDotNet

let load_template base_path path =
    
    use f = File.OpenText(Path.Combine(base_path, path))
    Handlebars.Compile(f.ReadToEnd())
   
let expand_write_file entity (template: HandlebarsTemplate<obj,obj>) base_path (path: string) =
    let _path = Path.Combine(base_path, path.Replace("{{entity}}", entity))
    let value = dict["entity", entity]    
    
    let dir = Path.GetDirectoryName(_path)
    if not (File.Exists(dir)) then        
        Directory.CreateDirectory(dir) |> ignore
        
    File.WriteAllText(_path, template.Invoke(value))            
    
                
    
 
    
let copyAndExpandFiles (entities: string list) (frompath: string) (topath: string) =
    
    let repo = load_template frompath "Data/Repositories/{{entity}}Repository.cs.hbs" 
    let irepo = load_template frompath "Domain/RepositoryInterfaces/I{{entity}}Repository.cs.hbs" 
    let controller = load_template frompath "Business/{{entity}}Management/Controllers/{{entity}}Controller.cs.hbs" 
    let converter = load_template frompath "Business/{{entity}}Management/Converters/{{entity}}Converter.cs.hbs"
     
    let service = load_template frompath "Business/{{entity}}Management/Services/{{entity}}Service.cs.hbs"
    let iservice = load_template frompath "Business/{{entity}}Management/Services/I{{entity}}Service.cs.hbs" 
    
    for entity in entities do
        expand_write_file entity repo topath "Data/Repositories/{{entity}}Repository.cs"
        expand_write_file entity irepo topath "Domain/RepositoryInterfaces/I{{entity}}Repository.cs"
        expand_write_file entity controller topath "Business/{{entity}}Management/Controllers/{{entity}}Controller.cs"
        expand_write_file entity converter topath "Business/{{entity}}Management/Converters/{{entity}}Converter.cs"

        expand_write_file entity service topath "Business/{{entity}}Management/Services/{{entity}}Service.cs"
        expand_write_file entity iservice topath "Business/{{entity}}Management/Services/I{{entity}}Service.cs"

        
(*
Data/Repositories/{{entity}}Repository.cs
Domain/RepositoryInterfaces/I{{entity}}Repository.cs

Business/{{entity}}Management/Controllers/{{entity}}Controller.cs
Business/{{entity}}Management/Converters/{{entity}}Converter.cs
Business/{{entity}}Management/Dto/{{entity}}Dto.cs
Business/{{entity}}Management/Services/{{entity}}Service.cs
Business/{{entity}}Management/Services/I{{entity}}Service.cs
        
*)