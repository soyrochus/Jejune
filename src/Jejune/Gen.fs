namespace Jejune

module Gen =
    open System.IO
    open System.Text.RegularExpressions
    open HandlebarsDotNet
    open FSharp.Data 
    open Files
    open NUnit.Framework
    
    let expand ((path, chunks):TemplatePath) (data: JsonValue) =         
        
        use f = File.OpenText(path)
        let template = Handlebars.Compile(f.ReadToEnd())
        let _data = getChildObject chunks data
        
        template.Invoke(jsonpropsToDict _data)
        //TestContext.Out.WriteLine(res)        
        //"1001"
        //template.Invoke(getChildObject chunks data)

        
    module Tests = 
        open NUnit.Framework    
        open System.IO
        open NUnit.Framework
        open Files
        

        [<Test>]
        let ``Expand source template with traversed data object``() = 
                           
            let datapath = Path.Combine [|__SOURCE_DIRECTORY__; "test"; "testdata.json" |]
            let templpath = Path.Combine [|__SOURCE_DIRECTORY__; "test"; "{level1}"; "{level2}.txt.hbs" |]

            let ob = match getDataObject datapath with       
                     | Ok o -> o
                     | Error e -> failwith e.Message    
                        
            let res = expand (templpath, [|DirVar "level1"; FileVar ("level2", ".txt") |]) ob

            //TestContext.Out.WriteLine("Joder")           
            Assert.AreEqual("1000", res)




