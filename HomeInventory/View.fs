module HomeInventory.View

open Types
open Giraffe.ViewEngine

let appSettings = {
    name = "Home Inventory"
}

let Layout (pageTitle : string Option) content=
    html [] [
        head [] [
            title [] [
                match pageTitle with
                    | Some title -> str $"{title} | {appSettings.name}"
                    | None -> str appSettings.name
            ]
        ]
        body [] [
            main [] [
                header [] [
                    h1 [] [str "🏠 Home Inventory"]
                ]
                section [] [
                    yield! content
                ]
                footer [] [
                    small [] [str "Home Inventory"]
                ]
            ]
        ]
    ]

let Index appSettings = Layout None [||]