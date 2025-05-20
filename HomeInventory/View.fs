module HomeInventory.View

open HomeInventory.Model
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
        _name "parent_id"
        _class "form-select"
        if required then _required
    ] [
        option [_value ""] [ str "-- Select a container --" ]
        yield! createOptions nodes
    ]

let containerSelectBox required=
        Model.getAllContainerItems ()
        |> Model.buildTree
        |> containerSelect required

let formControl labelText inputNodes =
    div [_class "formControl"] [
        label [] [str labelText]
        yield! inputNodes
    ]

let moveItemDialog () =
    dialog [_id "moveItemDialog"] [
        form [ _id "moveItemForm"] [
            h3 [] [str "Move Item"]
            input [_name "id" ;_required; _type "hidden"]
            formControl "Item Name/Code" [input [_name "name" ;_required; _autocomplete "off"]]
            formControl "Item Description" [input [_name "description"; _required; _autocomplete "off"]]
            formControl "Item Tags" [textarea [_name "tags"; _required; _autocomplete "off"] []]
            formControl "Item Container" [containerSelectBox true]
            div [_class "modalButtons"] [
                button [_class "button button-primary close-modal"; _type "button"] [str "❌ Close"]
                button [_class "button button-primary create-item"] [str "💾 Save"]
            ]
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
                    button [_class "button button-primary add-item"; _type "button"; (attr "data-action") "add"] [str "➕ Add Item"]
                ]

            ]
        ]
    ]

let Index ()  =
    Layout None [
        nav [] [
            form [] [
                input [_type "text"; _placeholder "Search for an item"; _id "search"; _name "search"; _autocomplete "off"]
            ]
        ]
        section [_class "resultSet"] []
        moveItemDialog ()

    ]

let itemCardList itemCards =
    section [_class "searchResults"] [
        yield! itemCards
    ]

let ItemCard (item: Item) (breadcrumbs: string[]) =
    let bcList = String.Join(" » ",( Array.take (breadcrumbs.Length - 1) breadcrumbs))
    let showInUseButton =
        match item.parent_id with
            | None -> true
            | Some parent_id -> parent_id <> InUseContainer.id
    let bcListWithPointer =
        match item.parent_id with
            | Some _ -> bcList + " » "
            | None -> ""
    div [_class "itemCard"] [
        section [_class "image"] [

        ]
        section [_class "details"] [
            small [] [str bcListWithPointer]
            h3 [] [str item.name]
            p [] [str item.description]
            div [_class "actionButtons"] [
            button [
                _class "button button-primary move-item"
                _type "button"
                (attr "data-item-id" (string item.id))
                (attr "data-item-name" (string item.name))
                (attr "data-parent-id" (if item.parent_id.IsSome then string item.parent_id.Value else ""))
                (attr "data-tags" item.tags)
                (attr "data-description" item.description)
                (attr "data-action") "edit"
            ] [str "📦 Move Item"]
            if (not (item.tags.Contains("container"))) && showInUseButton then
                button [
                    _class "button button-primary use-item"
                    _type "button"
                    (attr "data-item-id" (string item.id))
                ] [str "🔒 Mark as In Use"]
            ]
        ]
    ]