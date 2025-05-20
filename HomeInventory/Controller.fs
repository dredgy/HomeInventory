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

let UpdateItem (item: Item) =
    match item.id with
    | 0 -> Model.CreateItem item
    | _ -> Model.UpdateItem item
    View.containerSelectBox true

let CheckoutItem id =
    if id <> InUseContainer.id then
        let item = GetItemById id
        let updatedItem = {item with parent_id = Some InUseContainer.id}
        Model.UpdateItem updatedItem |> ignore
    "Updated Successfully"



let Search string =
    Model.Search string
    |> Array.filter (fun item -> not(item.tags.Contains("hidden")))
    |> Array.map (fun item -> View.ItemCard item (GetBreadCrumbs item [||]))
    |> itemCardList