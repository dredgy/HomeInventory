module HomeInventory.DataTypes

open HomeInventory.Types
open Types
open Dapper.FSharp.PostgreSQL

let ItemTable = table'<Item> "items"