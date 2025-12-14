
namespace LibraryManagementSystem
open System
open System.Drawing
open System.Windows.Forms
open LibraryManagementSystem.LibraryManager

type UserForm() as this =
    inherit Form()

    let lblTitle =
        new Label(
            Text = "User Panel - Borrow & Return",
            Dock = DockStyle.Top,
            Height = 60,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 20.0f, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        )

    let searchPanel = new FlowLayoutPanel(Dock = DockStyle.Top, Height = 70, Padding = Padding(15))
    let txtSearch = new TextBox(Width = 350, Font = new Font("Segoe UI", 11.0f))
    let btnSearch = new Button(Text = "Search", Width = 110, Height = 35)

    let actionPanel = new FlowLayoutPanel(Dock = DockStyle.Top, Height = 80, Padding = Padding(15))
    let txtBookName = new TextBox(Width = 300, Font = new Font("Segoe UI", 11.0f))
    let btnBorrow =
        new Button(
            Text = "Borrow",
            Width = 130,
            Height = 40,
            BackColor = Color.Orange,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10.0f, FontStyle.Bold)
        )

    let btnReturn =
        new Button(
            Text = "Return",
            Width = 130,
            Height = 40,
            BackColor = Color.RoyalBlue,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10.0f, FontStyle.Bold)
        )

    let lstBooks =
        new ListView(
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("Segoe UI", 10.5f)
        )

    let refreshList () =
        lstBooks.Items.Clear()
        for book in GetAllBooks() do
            let item =
                ListViewItem(
                    [| string book.Id
                       book.Title
                       book.Author
                       if book.IsAvailable then "Available" else "Borrowed" |]
                )

            item.BackColor <- if book.IsAvailable then Color.White else Color.LightCoral
            lstBooks.Items.Add(item) |> ignore

    do
        lstBooks.Columns.Add("ID", 70) |> ignore
        lstBooks.Columns.Add("Title", 300) |> ignore
        lstBooks.Columns.Add("Author", 220) |> ignore
        lstBooks.Columns.Add("Status", 120) |> ignore

        searchPanel.Controls.AddRange([| new Label(Text = "Search:", Width = 70); txtSearch; btnSearch |])
        actionPanel.Controls.AddRange(
            [| new Label(Text = "Book Title:", Width = 90)
               txtBookName
               btnBorrow
               btnReturn |]
        )

        this.Controls.AddRange([| lstBooks; actionPanel; searchPanel; lblTitle |])

        this.Text <- "User Panel - F# Library"
        this.Size <- Size(940, 680)
        this.StartPosition <- FormStartPosition.CenterScreen

        LoadData()
        this.Load.Add(fun _ -> refreshList())

        LibraryManager.DataChanged.Publish.Add(fun _ ->
            refreshList()
        )

        btnSearch.Click.Add(fun _ ->
            let results = SearchBooks txtSearch.Text
            lstBooks.Items.Clear()
            for book in results do
                let item =
                    ListViewItem(
                        [| string book.Id
                           book.Title
                           book.Author
                           if book.IsAvailable then "Available" else "Borrowed" |]
                    )

                item.BackColor <- if book.IsAvailable then Color.White else Color.LightCoral
                lstBooks.Items.Add(item) |> ignore
        )

        lstBooks.SelectedIndexChanged.Add(fun _ ->
            if lstBooks.SelectedItems.Count > 0 then
                txtBookName.Text <- lstBooks.SelectedItems.[0].SubItems.[1].Text
        )

        btnBorrow.Click.Add(fun _ ->
            let title = txtBookName.Text.Trim()
            let books = GetAllBooks() |> List.filter (fun b -> b.Title.Equals(title, StringComparison.OrdinalIgnoreCase))

            match books with
            | [book] ->
                if not (BorrowBook book.Id) then
                    MessageBox.Show("Book already borrowed.") |> ignore
            | _ ->
                MessageBox.Show("Please select a book from the list.") |> ignore
        )

        btnReturn.Click.Add(fun _ ->
            let title = txtBookName.Text.Trim()
            let books = GetAllBooks() |> List.filter (fun b -> b.Title.Equals(title, StringComparison.OrdinalIgnoreCase))

            match books with
            | [book] ->
                if not (ReturnBook book.Id) then
                    MessageBox.Show("Book was not borrowed.") |> ignore
            | _ ->
                MessageBox.Show("Please select a book from the list.") |> ignore
        )

        this.FormClosing.Add(fun _ -> SaveData())
