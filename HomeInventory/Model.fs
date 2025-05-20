module HomeInventory.Model

open Dapper.FSharp.PostgreSQL
open DataTypes
open HomeInventory.Types
open System.Collections.Generic
open System.Linq
open Npgsql.FSharp


let ConnectionString = $"Server=192.168.1.17;Port={5432};User Id=postgres;Password=root;Database=home_inventory;Include Error Detail=true"
let connectToDatabase () = (new Npgsql.NpgsqlConnection(ConnectionString))
let EnumerableToArray (enumerable: IEnumerable<'x>) =  enumerable.ToArray()

(*language=postgresql*)
let getAllContainerItems ()  =
    ConnectionString
    |> Sql.connect
    |> Sql.query """
        WITH RECURSIVE container_tree AS (
            SELECT id, parent_id, name, description, tags, 1 as level
            FROM items
            WHERE parent_id IS NULL
            AND tags LIKE '%container%'

            UNION ALL

            SELECT i.id, i.parent_id, i.name, i.description, i.tags, ct.level + 1
            FROM items i
            INNER JOIN container_tree ct ON i.parent_id = ct.id
            WHERE i.tags LIKE '%container%'
        )
        SELECT id, parent_id, name, description, tags, level
        FROM container_tree
        ORDER BY level, name;
    """
    |> Sql.execute (fun read ->
        {
            id = read.int "id"
            parent_id = read.intOrNone "parent_id"
            name = read.string "name"
            description = read.string "description"
            tags = read.string "tags"
        })

let rec buildTree (items: Item list) =

    let rec buildNode (item: Item) =
        let children =
            items
            |> List.filter (fun i -> i.parent_id = Some item.id)
            |> List.map buildNode
        { item = item; children = children }

    items
    |> List.filter (fun i -> i.parent_id.IsNone)
    |> List.map buildNode

let getItemById (id: int) =
    let connection = connectToDatabase()

    let results =
        select {
            for item in ItemTable do
            where (item.id = id)
        }
        |> connection.SelectAsync<Item>
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> EnumerableToArray
        |> Array.head

    connection.Close()
    results

let CreateItem (newItem: Item) =
    let connection = connectToDatabase()
    insert {
        for item in ItemTable do
        value newItem
        excludeColumn item.id
    }
    |> connection.InsertAsync
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore
    connection.Close()


let UpdateItem (updatedItem: Item) =
    let connection = connectToDatabase()
    update {
        for item in ItemTable do
        set updatedItem
        where (item.id = updatedItem.id)
    }
    |> connection.UpdateAsync
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore
    connection.Close()


let InUseContainer =
    let connection = connectToDatabase()
    let result =
        select {
            for item in ItemTable do
            where (item.name = "In Use")
        }
        |> connection.SelectAsync<Item>
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> EnumerableToArray
        |> Array.head
    connection.Close()
    result

let GetItemById id =
    let connection = connectToDatabase()
    let result =
        select {
            for item in ItemTable do
            where (item.id = id)
        }
        |> connection.SelectAsync<Item>
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> EnumerableToArray
        |> Array.head
    connection.Close()
    result


let Search searchString =
    let connection = connectToDatabase()

    let pattern = sprintf "%%%s%%" searchString

    let results =
        ConnectionString
        |> Sql.connect
        |> Sql.query """
            SELECT *
            FROM items
            WHERE (
                name ILIKE @pattern
                OR description ILIKE @pattern
                OR tags ILIKE @pattern
            )
            AND NOT (tags ILIKE '%hidden%')
            AND NOT (
                tags ILIKE '%container%'
                AND tags NOT ILIKE '%searchable%'
            )
            ORDER BY name;
            """
        |> Sql.parameters [ "pattern", Sql.string pattern ]
        |> Sql.execute (fun read ->
            {
                id = read.int "id"
                parent_id = read.intOrNone "parent_id"
                name = read.string "name"
                description = read.string "description"
                tags = read.string "tags"
            })
        |>Array.ofList

    results

