import { parse } from "./fable_modules/fable-library-js.4.24.0/Int32.js";
import { printf, toConsole, isNullOrEmpty } from "./fable_modules/fable-library-js.4.24.0/String.js";
import { Item } from "./HomeInventory/Types.js";
import { some } from "./fable_modules/fable-library-js.4.24.0/Option.js";
import { PromiseBuilder__Delay_62FBFDE1, PromiseBuilder__Run_212F1D4B } from "./fable_modules/Fable.Promise.2.2.2/Promise.fs.js";
import { promise } from "./fable_modules/Fable.Promise.2.2.2/PromiseImpl.fs.js";
import { fetch$, Types_RequestProperties } from "./fable_modules/Fable.Fetch.2.7.0/Fetch.fs.js";
import { empty, ofArray } from "./fable_modules/fable-library-js.4.24.0/List.js";
import { defaultOf, equals } from "./fable_modules/fable-library-js.4.24.0/Util.js";

export function submitItemForm(e) {
    e.preventDefault();
    const form = e.target;
    const idInput = form.querySelector("input[name=\'id\']");
    const nameInput = form.querySelector("input[name=\'name\']");
    const descriptionInput = form.querySelector("[name=\'description\']");
    const tagsInput = form.querySelector("[name=\'tags\']");
    const containerInput = form.querySelector("[name=\'parent_id\']");
    const item = new Item(parse(idInput.value, 511, false, 32), isNullOrEmpty(containerInput.value) ? undefined : parse(containerInput.value, 511, false, 32), nameInput.value, descriptionInput.value, tagsInput.value);
    console.log(some(item));
    PromiseBuilder__Run_212F1D4B(promise, PromiseBuilder__Delay_62FBFDE1(promise, () => (PromiseBuilder__Delay_62FBFDE1(promise, () => {
        const headers = {
            "Content-Type": "application/json",
        };
        const requestOptions = ofArray([new Types_RequestProperties(0, ["POST"]), new Types_RequestProperties(2, [JSON.stringify(item)]), new Types_RequestProperties(1, [headers])]);
        return fetch$("/item/update", requestOptions).then((_arg) => {
            const response = _arg;
            if (response.ok) {
                const dialog = document.getElementById("moveItemDialog");
                dialog.close();
                window.location.reload();
                return Promise.resolve();
            }
            else {
                const arg = (response.status) | 0;
                toConsole(printf("Error updating item: %A"))(arg);
                return Promise.resolve();
            }
        });
    }).catch((_arg_1) => {
        const arg_1 = _arg_1.message;
        toConsole(printf("Error: %s"))(arg_1);
        return Promise.resolve();
    }))));
}

export function searchKeyUpHandler(e) {
    const input = e.target;
    const query = input.value;
    PromiseBuilder__Run_212F1D4B(promise, PromiseBuilder__Delay_62FBFDE1(promise, () => (PromiseBuilder__Delay_62FBFDE1(promise, () => (fetch$(`/search/${query}`, empty()).then((_arg) => {
        const response = _arg;
        return (response.ok) ? (response.text().then((_arg_1) => {
            const divs = document.querySelectorAll(".resultSet");
            divs.forEach((div) => {
                div.innerHTML = _arg_1;
            });
            return Promise.resolve();
        })) : (Promise.resolve());
    }))).catch((_arg_2) => {
        toConsole(`Error: ${_arg_2.message}`);
        return Promise.resolve();
    }))));
}

export function attachSearchEvent() {
    const searchInput = document.querySelector("input[name=\'search\']");
    if (equals(searchInput, defaultOf())) {
    }
    else {
        const value = searchInput.addEventListener("keyup", (e) => {
            searchKeyUpHandler(e);
        });
    }
}

export function attachClickEvents() {
    document.body.addEventListener("click", (ev) => {
        const itemDialog = document.getElementById("moveItemDialog");
        const itemForm = document.getElementById("moveItemForm");
        const idInput = itemForm.querySelector("input[name=\'id\']");
        const nameInput = itemForm.querySelector("input[name=\'name\']");
        const descriptionInput = itemForm.querySelector("[name=\'description\']");
        const tagsInput = itemForm.querySelector("[name=\'tags\']");
        const containerInput = itemForm.querySelector("[name=\'parent_id\']");
        const dialogHeader = itemForm.querySelector("h3");
        const matchValue = ev.target;
        let matchResult;
        if (matchValue instanceof HTMLElement) {
            if (matchValue.classList.contains("add-item")) {
                matchResult = 0;
            }
            else if (matchValue.classList.contains("move-item")) {
                matchResult = 1;
            }
            else if (matchValue.classList.contains("use-item")) {
                matchResult = 2;
            }
            else if (matchValue.classList.contains("close-modal")) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
        }
        else {
            matchResult = 4;
        }
        switch (matchResult) {
            case 0: {
                idInput.value = "0";
                nameInput.value = "";
                descriptionInput.value = "";
                tagsInput.value = "";
                containerInput.value = "";
                dialogHeader.innerText = "Add Item";
                itemDialog.showModal();
                break;
            }
            case 1: {
                const target_5 = matchValue;
                idInput.value = target_5.getAttribute("data-item-id");
                nameInput.value = target_5.getAttribute("data-item-name");
                descriptionInput.value = target_5.getAttribute("data-description");
                tagsInput.value = target_5.getAttribute("data-tags");
                containerInput.value = target_5.getAttribute("data-parent-id");
                dialogHeader.innerText = "Edit Item";
                itemDialog.showModal();
                break;
            }
            case 2: {
                const target_6 = matchValue;
                const itemId = target_6.getAttribute("data-item-id");
                PromiseBuilder__Run_212F1D4B(promise, PromiseBuilder__Delay_62FBFDE1(promise, () => (PromiseBuilder__Delay_62FBFDE1(promise, () => (fetch$(`/checkout/${itemId}`, empty()).then((_arg) => {
                    if (_arg.ok) {
                        const event = document.createEvent("KeyboardEvent");
                        event.initEvent("keyup", true, true);
                        document.getElementById("search").dispatchEvent(event);
                        target_6.remove();
                        return Promise.resolve();
                    }
                    else {
                        return Promise.resolve();
                    }
                }))).catch((_arg_1) => {
                    toConsole(`Error: ${_arg_1.message}`);
                    return Promise.resolve();
                }))));
                break;
            }
            case 3: {
                itemDialog.close();
                break;
            }
            case 4: {
                break;
            }
        }
    });
}

export function pageLoaded(e) {
    const itemForm = document.getElementById("moveItemForm");
    itemForm.addEventListener("submit", (e_1) => {
        submitItemForm(e_1);
    });
    attachSearchEvent();
    attachClickEvents();
}

window.addEventListener("DOMContentLoaded", (e) => {
    pageLoaded(e);
});

