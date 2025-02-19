module HomeInventory.Types

type ApplicationSettings = {
    name: string
}


type Item = {
    id : int
    parent_id : int option
    name: string
    description: string
    tags: string
}

type ItemNode = {
    item: Item
    children: ItemNode list
}