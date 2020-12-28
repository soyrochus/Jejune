namespace Jejune

module Template =

    open System.Text.RegularExpressions    
    open HandlebarsDotNet

    let source = @"<div class=""entry"">
        <h1>{{title}}</h1>
        <div class=""body"">
            {{body}}
        </div>
        </div>"

    let template = Handlebars.Compile source

    let data = {|title = "My new post";    body = "This is my first post!"|}
    
    let test =
        template.Invoke data
