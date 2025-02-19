module Frontend
open Browser
open Browser.Types
open Fable.Core.JsInterop
open HomeInventory.Types


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
        match ev.target with
        | :? HTMLElement as target when target.classList.contains("add-item") ->
            let dialog = document.getElementById("addItemDialog") :?> HTMLDialogElement
            dialog.showModal()
        | :? HTMLElement as target when target.classList.contains("create-item") ->
            let dialog = document.getElementById("addItemDialog") :?> HTMLDialogElement
            dialog.close()
        | :? HTMLElement as target when target.classList.contains("update-item") ->
            let dialog = document.getElementById("addItemDialog") :?> HTMLDialogElement
            dialog.close()
        | _ -> ()
)

let pageLoaded (e: Types.Event) =
    attachSearchEvent ()
    attachClickEvents ()
    ()

window.addEventListener("DOMContentLoaded", pageLoaded)