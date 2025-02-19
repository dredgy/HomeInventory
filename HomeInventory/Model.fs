module HomeInventory.Model

open Dapper.FSharp.PostgreSQL
open DataTypes
open HomeInventory.Types
open System.Collections.Generic
open System.Linq
open Npgsql.FSharp


let ConnectionString = $"Server=localhost;Port={5432};User Id=postgres;Password=root;Database=home_inventory;Include Error Detail=true"
let connectToDatabase () = (new Npgsql.NpgsqlConnection(ConnectionString))
let EnumerableToArray (enumerable: IEnumerable<'x>) =  enumerable.ToArray()
let getAllContainerItems  =
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
    let itemsById = items |> List.map (fun i -> i.id, i) |> Map.ofList

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

let Search searchString =
    let connection = connectToDatabase()

    let pattern = sprintf "%%%s%%" searchString

    let results =
        select {
            for item in ItemTable do
            where (ilike item.name pattern)
            orWhere (ilike item.description pattern)
            orWhere (ilike item.tags pattern)
        }
        |> connection.SelectAsync<Item>
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> EnumerableToArray

    connection.Close()
    results

