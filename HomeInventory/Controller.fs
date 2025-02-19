module HomeInventory.Controller
open DataTypes
open Dapper.FSharp
open Giraffe.ViewEngine
open HomeInventory.Model
open HomeInventory.Types
open HomeInventory.View
open Saturn

let rec GetBreadCrumbs (item: Item) (itemsSoFar : string[]) =
    match item.parent_id with
    | Some parent_id ->
        let newItems = itemsSoFar |> Array.append [|item.name|]
        let parent = Model.getItemById parent_id
        GetBreadCrumbs parent newItems
    | None -> itemsSoFar |> Array.append [|item.name|]

let Search string =
    Model.Search string
    |> Array.map (fun item -> View.ItemCard item (GetBreadCrumbs item [||]))
    |> itemCardList