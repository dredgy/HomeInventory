import { Record } from "../fable_modules/fable-library-js.4.24.0/Types.js";
import { list_type, option_type, int32_type, record_type, string_type } from "../fable_modules/fable-library-js.4.24.0/Reflection.js";

export class ApplicationSettings extends Record {
    constructor(name) {
        super();
        this.name = name;
    }
}

export function ApplicationSettings_$reflection() {
    return record_type("HomeInventory.Types.ApplicationSettings", [], ApplicationSettings, () => [["name", string_type]]);
}

export class Item extends Record {
    constructor(id, parent_id, name, description, tags) {
        super();
        this.id = (id | 0);
        this.parent_id = parent_id;
        this.name = name;
        this.description = description;
        this.tags = tags;
    }
}

export function Item_$reflection() {
    return record_type("HomeInventory.Types.Item", [], Item, () => [["id", int32_type], ["parent_id", option_type(int32_type)], ["name", string_type], ["description", string_type], ["tags", string_type]]);
}

export class ItemNode extends Record {
    constructor(item, children) {
        super();
        this.item = item;
        this.children = children;
    }
}

export function ItemNode_$reflection() {
    return record_type("HomeInventory.Types.ItemNode", [], ItemNode, () => [["item", Item_$reflection()], ["children", list_type(ItemNode_$reflection())]]);
}

