open System
open HomeInventory
open HomeInventory.Controller
open HomeInventory.Types
open Microsoft.Extensions.DependencyInjection
open Saturn
open Giraffe
open Microsoft.Extensions.Configuration
open System.IO

open System.Net
open System.Net.Sockets
open System.Text.Json
open System.Reflection
open System.Text.Json.Serialization

module Program =

    let jsonOptions =
        let options = JsonSerializerOptions(JsonSerializerDefaults.Web)
        options.Converters.Add(JsonFSharpConverter()) // Enable F# option type support
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase // Use camelCase JSON keys
        options
    Dapper.FSharp.PostgreSQL.OptionTypes.register()

    let exeDir = AppContext.BaseDirectory

    let config =
        ConfigurationBuilder()
            .SetBasePath(exeDir)
            .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
            .Build()

    let connectionString = config.GetConnectionString("HomeInventory")
    Model.ConnectionString <- connectionString


    let router = router {
        not_found_handler (setStatusCode 404 >=> text "404")
        get "/" (warbler(fun _ -> htmlView (View.Index ())))
        getf "/search/%s" (fun string -> htmlView (Controller.Search string))
        getf "/checkout/%i" (fun itemId -> text (CheckoutItem itemId))
        post "/item/update" (bindJson<Item> (fun item ->
            fun next ctx ->
                htmlView (UpdateItem item) next ctx
        ))
    }

    let ServiceConfig (services: IServiceCollection) =
        // Get the server IP address
        let serverIpAddress =
            match Dns.GetHostEntry(Dns.GetHostName()).AddressList |> Array.tryFind(fun ip -> ip.AddressFamily = AddressFamily.InterNetwork) with
            | Some ip -> ip.ToString()
            | None -> "IP address not found"

        let boldCode = "\u001b[1m"
        let greenCode = "\u001b[32m"
        let resetCode = "\u001b[0m"


        printfn $"{boldCode}Now Running On: {greenCode}%s{serverIpAddress}{resetCode}"
        services.AddHttpContextAccessor()


    let app =
        application {
            use_mime_types [(".woff", "application/font-woff")]
            use_static (Path.Combine(AppContext.BaseDirectory, "wwwroot"))
            use_router router
            use_json_serializer (SystemTextJson.Serializer jsonOptions)
            use_developer_exceptions
            service_config ServiceConfig
            url "http://0.0.0.0:5001"
        }

    run app
