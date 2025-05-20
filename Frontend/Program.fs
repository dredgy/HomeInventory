module Frontend
open Browser
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open HomeInventory.Types
open Fetch
open Fetch.Types

let submitItemForm (e: Event) =
    e.preventDefault() // Prevent default form submission

    let form = e.target :?> HTMLFormElement
    let idInput = form.querySelector("input[name='id']") :?> HTMLInputElement
    let nameInput = form.querySelector("input[name='name']") :?> HTMLInputElement
    let descriptionInput = form.querySelector("[name='description']") :?> HTMLInputElement
    let tagsInput = form.querySelector("[name='tags']") :?> HTMLTextAreaElement
    let containerInput = form.querySelector("[name='parent_id']") :?> HTMLSelectElement

    let item = {
        id = int idInput.value
        parent_id =
            if System.String.IsNullOrEmpty(containerInput.value) then None
            else Some(int containerInput.value)
        name = nameInput.value
        description = descriptionInput.value
        tags = tagsInput.value
    }

    console.log item

    // Send the POST request
    promise {
        try
            let headers = createObj [
                "Content-Type" ==> "application/json"
            ]

            let requestOptions = [
                RequestProperties.Method HttpMethod.POST
                RequestProperties.Body (!!Fable.Core.JS.JSON.stringify(item))
                RequestProperties.Headers (!!(headers))
            ]

            let! response = fetch "/item/update" requestOptions
            if response.Ok then
                // Close the dialog after successful submission
                let dialog = document.getElementById("moveItemDialog") :?> HTMLDialogElement
                dialog.close()

                // Optionally refresh the page or update the UI
                window.location.reload()
            else
                // Handle error case
                printfn "Error updating item: %A" response.Status
        with
        | ex -> printfn "Error: %s" ex.Message
    }
    |> ignore

let searchKeyUpHandler (e: Types.Event) =
    let input = e.target :?> HTMLInputElement
    let query = input.value
    let url = $"/search/{query}"

    promise {
        try
            let! response = Fetch.fetch url []
            if response.Ok then
                let! data = response.text()

                // Find all divs with the class 'my-class' and replace their content
                let divs = document.querySelectorAll(".resultSet")
                divs?forEach(fun div ->
                    div?innerHTML <- data
                )
        with
        | ex -> printfn $"Error: %s{ex.Message}"
    }
    |> ignore

let attachSearchEvent () =
    let searchInput = document.querySelector("input[name='search']")
    match searchInput with
    | null -> ()
    | _ ->
        searchInput.addEventListener("keyup", searchKeyUpHandler) |> ignore

let attachClickEvents () =
    document.body.addEventListener("click", fun (ev: Event) ->
        let itemDialog = document.getElementById("moveItemDialog") :?> HTMLDialogElement
        let itemForm = document.getElementById("moveItemForm")
        let idInput = itemForm.querySelector("input[name='id']") :?> HTMLInputElement
        let nameInput = itemForm.querySelector("input[name='name']") :?> HTMLInputElement
        let descriptionInput = itemForm.querySelector("[name='description']") :?> HTMLInputElement
        let tagsInput = itemForm.querySelector("[name='tags']") :?> HTMLTextAreaElement
        let containerInput = itemForm.querySelector("[name='parent_id']") :?> HTMLSelectElement
        let dialogHeader = itemForm.querySelector("h3") :?> HTMLHeadingElement


        match ev.target with
        | :? HTMLElement as target when target.classList.contains("add-item") ->
            idInput.value <- "0"
            nameInput.value <- ""
            descriptionInput.value <- ""
            tagsInput.value <- ""
            containerInput.value <- ""
            dialogHeader.innerText <- "Add Item"
            itemDialog.showModal()
        | :? HTMLElement as target when target.classList.contains("move-item") ->
            idInput.value <- target.getAttribute("data-item-id")
            nameInput.value <- target.getAttribute("data-item-name")
            descriptionInput.value <- target.getAttribute("data-description")
            tagsInput.value <- target.getAttribute("data-tags")
            containerInput.value <- target.getAttribute("data-parent-id")
            dialogHeader.innerText <- "Edit Item"
            itemDialog.showModal()
         | :? HTMLElement as target when target.classList.contains("use-item") ->
                let itemId = target.getAttribute("data-item-id")
                promise {
                    try
                        let! response = fetch $"/checkout/{itemId}" []
                        if response.Ok then
                            let event = document.createEvent("KeyboardEvent")
                            event.initEvent("keyup", true, true)
                            document.getElementById("search").dispatchEvent(event) |> ignore
                            target.remove()
                    with
                    | ex -> printfn $"Error: %s{ex.Message}"
                } |> ignore
        | :? HTMLElement as target when target.classList.contains("close-modal") ->
            itemDialog.close()
        | _ -> ()
)

let pageLoaded (e: Types.Event) =
    let itemForm = document.getElementById("moveItemForm")
    itemForm.addEventListener("submit", submitItemForm)
    attachSearchEvent ()
    attachClickEvents ()
    ()

window.addEventListener("DOMContentLoaded", pageLoaded)