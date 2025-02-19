module HomeInventory.View

open HomeInventory.Types
open Types
open Giraffe.ViewEngine
open System

let appSettings = {
    name = "🏠 Home Inventory"
}

let rec createOptions (nodes: ItemNode list) =
    let createIndent level = String.replicate (level*4) "&nbsp;"

    let rec createOptionElements level (nodes: ItemNode list) =
        nodes
        |> List.collect (fun node ->
            [
                option [
                    _value (string node.item.id)
                    attr "data-level" (string level)
                ] [
                    rawText (sprintf "%s%s" (createIndent level) node.item.name)
                ]
                yield! createOptionElements (level + 1) node.children
            ])

    createOptionElements 0 nodes

let containerSelect (required: bool) (nodes: ItemNode list) =
    select [
        _name "container"
        _id "container-select"
        _class "form-select"
        if required then _required
    ] [
        option [_value ""] [ str "-- Select a container --" ]
        yield! createOptions nodes
    ]

let containerSelectBox required =
        Model.getAllContainerItems
        |> Model.buildTree
        |> containerSelect required

let formControl labelText inputNodes =
    div [_class "formControl"] [
        label [] [str labelText]
        yield! inputNodes
    ]

let addItemDialog =
    dialog [_id "addItemDialog"] [
        form [_class "addItemForm"] [
            h3 [] [str "Add Item"]

            formControl "Item Name/Code" [input [_name "item_name" ;_required]]
            formControl "Item Description" [input [_name "item_description"; _required]]
            formControl "Item Tags" [textarea [_name "item_tags"; _required] []]
            formControl "Item Container" [containerSelectBox true]
            button [_class "button button-secondary create-item"; _type "button"] [str "💾 Save"]
        ]

    ]

let Layout (pageTitle : string Option) content=
    html [] [
        head [] [
            link [_rel "stylesheet" ; _href "/styles/core.css"]
            script [_src "/scripts/Program.js"; _type "module"] []
            title [] [
                match pageTitle with
                    | Some title -> str $"{title} | {appSettings.name}"
                    | None -> str appSettings.name
            ]
        ]
        body [] [
            main [] [
                header [] [
                    h1 [] [str appSettings.name]
                ]
                section [] [
                    yield! content
                ]
                footer [] [
                    button [_class "button button-primary add-item"; _type "button"] [str "➕ Add Item"]
                    button [_class "button button-primary move-item"; _type "button"] [str "📦 Move Item"]
                ]
                addItemDialog
            ]
        ]
    ]

let Index  =
    Layout None [
        nav [] [
            form [] [
                input [_type "text"; _placeholder "Search for an item"; _name "search"]
            ]
        ]
        section [_class "resultSet"] [

        ]
    ]

let itemCardList itemCards =
    section [_class "searchResults"] [
        yield! itemCards
    ]

let ItemCard (item: Item) (breadcrumbs: string[]) =
    let bcList = String.Join(" » ", breadcrumbs)
    div [_class "itemCard"] [
        section [_class "image"] [

        ]
        section [_class "details"] [
            h3 [] [str item.name]
            p [] [str item.description]
            p [] [str bcList]
        ]
    ]