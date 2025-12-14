namespace LibraryManagementSystem

open System
open System.Collections.Generic
open System.IO
open System.Text.Json

module LibraryManager =

    
    let DataChanged = new Event<unit>()
    let notifyChange () = DataChanged.Trigger()

    let mutable books = ResizeArray<Book>()
    let mutable filePath = "library.json"

    let LoadData () =
        if File.Exists(filePath) then
            try
                let json = File.ReadAllText(filePath)
                let loaded = JsonSerializer.Deserialize<ResizeArray<Book>>(json)
                books.Clear()
                books.AddRange(loaded)
            with _ -> ()

    let SaveData () =
        try
            let options = JsonSerializerOptions(WriteIndented = true)
            let json = JsonSerializer.Serialize(books, options)
            File.WriteAllText(filePath, json)
        with _ -> ()

    let AddBook (title: string) (author: string) =
        let newId = if books.Count = 0 then 1 else books.[books.Count - 1].Id + 1
        let book = { Id = newId; Title = title.Trim(); Author = author.Trim(); IsAvailable = true }
        books.Add(book)
        SaveData()
        notifyChange()

    let SearchBooks (query: string) =
        if String.IsNullOrWhiteSpace(query) then
            books |> Seq.toList
        else
            books
            |> Seq.filter (fun b ->
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase))
            |> Seq.toList

    let BorrowBook id =
        let idx = books |> Seq.tryFindIndex (fun b -> b.Id = id && b.IsAvailable)
        match idx with
        | Some i ->
            books.[i] <- { books.[i] with IsAvailable = false }
            SaveData()
            notifyChange()
            true
        | None -> false

    let ReturnBook id =
        let idx = books |> Seq.tryFindIndex (fun b -> b.Id = id && not b.IsAvailable)
        match idx with
        | Some i ->
            books.[i] <- { books.[i] with IsAvailable = true }
            SaveData()
            notifyChange()
            true
        | None -> false

    let UpdateBook (id: int) (newTitle: string) (newAuthor: string) =
        let idx = books |> Seq.tryFindIndex (fun b -> b.Id = id)
        match idx with
        | Some i when books.[i].IsAvailable ->
            books.[i] <- { books.[i] with Title = newTitle.Trim(); Author = newAuthor.Trim() }
            SaveData()
            notifyChange()
            true
        | _ -> false

    let DeleteBook (id: int) =
        let idx = books |> Seq.tryFindIndex (fun b -> b.Id = id)
        match idx with
        | Some i when books.[i].IsAvailable ->
            books.RemoveAt(i) |> ignore
            for j in 0 .. books.Count - 1 do
                books.[j] <- { books.[j] with Id = j + 1 }
            SaveData()
            notifyChange()
            true
        | _ -> false

    let GetAllBooks () = books |> Seq.toList
