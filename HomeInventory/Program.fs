﻿open HomeInventory
open Microsoft.Extensions.DependencyInjection
open Saturn
open Giraffe
open Types

open System.Net
open System.Net.Sockets

module Program =

    let router = router {
        not_found_handler (setStatusCode 404 >=> text "404")
        get "/" ( View.Index |> htmlView)
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

        // Print the server IP address
        printfn $"{boldCode}Now Running On: {greenCode}%s{serverIpAddress}{resetCode}"
        services.AddHttpContextAccessor()


    let app =
        application {
            use_mime_types [(".woff", "application/font-woff")]
            use_static "wwwroot"
            use_router router
            use_developer_exceptions
            service_config ServiceConfig
            url "http://0.0.0.0:5001"
        }

    run app
