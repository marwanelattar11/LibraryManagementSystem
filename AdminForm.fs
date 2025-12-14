namespace LibraryManagementSystem

open System
open System.Drawing
open System.Windows.Forms
open LibraryManagementSystem.LibraryManager

type AdminForm() as this =
    inherit Form()

    let lblTitle =
        new Label(
            Text = "Admin Panel - Manage Books",
            Dock = DockStyle.Top,
            Height = 60,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 20.0f, FontStyle.Bold),
            ForeColor = Color.DarkBlue
        )

    let searchPanel = new FlowLayoutPanel(Dock = DockStyle.Top, Height = 70, Padding = Padding(15))
    let txtSearch = new TextBox(Width = 350, Font = new Font("Segoe UI", 11.0f))
    let btnSearch = new Button(Text = "Search", Width = 110, Height = 35)

    let inputPanel = new FlowLayoutPanel(Dock = DockStyle.Top, Height = 110, Padding = Padding(15))
    let txtTitle = new TextBox(Width = 220, PlaceholderText = "Book Title")
    let txtAuthor = new TextBox(Width = 220, PlaceholderText = "Author Name")
    let btnAdd =
        new Button(
            Text = "Add Book",
            Width = 130,
            Height = 40,
            BackColor = Color.ForestGreen,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10.0f, FontStyle.Bold)
        )

    let actionPanel = new FlowLayoutPanel(Dock = DockStyle.Top, Height = 80, Padding = Padding(15))
    let txtId = new TextBox(Width = 100, PlaceholderText = "ID")
    let btnEdit = new Button(Text = "Edit", Width = 90, Height = 35, BackColor = Color.Goldenrod)
    let btnDelete = new Button(Text = "Delete", Width = 90, Height = 35, BackColor = Color.Crimson, ForeColor = Color.White)
    let btnSaveEdit =
        new Button(
            Text = "Save Edit",
            Width = 110,
            Height = 35,
            BackColor = Color.MediumSeaGreen,
            ForeColor = Color.White,
            Visible = false
        )

    let lstBooks =
        new ListView(
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("Segoe UI", 10.5f)
        )

    let mutable editingId = -1

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
        inputPanel.Controls.AddRange(
            [| new Label(Text = "Title:", Width = 60)
               txtTitle
               new Label(Text = "Author:", Width = 70)
               txtAuthor
               btnAdd |]
        )

        actionPanel.Controls.AddRange(
            [| new Label(Text = "Book ID:", Width = 80)
               txtId
               btnEdit
               btnDelete
               btnSaveEdit |]
        )

        this.Controls.AddRange([| lstBooks; actionPanel; inputPanel; searchPanel; lblTitle |])

        this.Text <- "Admin Panel - F# Library"
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

        btnAdd.Click.Add(fun _ ->
            if String.IsNullOrWhiteSpace(txtTitle.Text) || String.IsNullOrWhiteSpace(txtAuthor.Text) then
                MessageBox.Show("Please enter title and author.") |> ignore
            else
                AddBook txtTitle.Text txtAuthor.Text
                txtTitle.Clear()
                txtAuthor.Clear()
        )

        btnEdit.Click.Add(fun _ ->
            if lstBooks.SelectedItems.Count = 0 then
                MessageBox.Show("Please select a book to edit.") |> ignore
            else
                let item = lstBooks.SelectedItems.[0]
                let id = int item.SubItems.[0].Text
                let book = GetAllBooks() |> List.tryFind (fun b -> b.Id = id)

                match book with
                | Some b when not b.IsAvailable ->
                    MessageBox.Show("Cannot edit borrowed book.") |> ignore
                | Some b ->
                    editingId <- id
                    txtTitle.Text <- b.Title
                    txtAuthor.Text <- b.Author
                    btnSaveEdit.Visible <- true
                    btnAdd.Enabled <- false
                | None -> ()
        )

        btnSaveEdit.Click.Add(fun _ ->
            if editingId > 0 && UpdateBook editingId txtTitle.Text txtAuthor.Text then
                MessageBox.Show("Book updated successfully!") |> ignore
                editingId <- -1
                txtTitle.Clear()
                txtAuthor.Clear()
                btnSaveEdit.Visible <- false
                btnAdd.Enabled <- true
            else
                MessageBox.Show("Failed to update.") |> ignore
        )

        btnDelete.Click.Add(fun _ ->
            if lstBooks.SelectedItems.Count = 0 then
                MessageBox.Show("Please select a book.") |> ignore
            else
                let item = lstBooks.SelectedItems.[0]
                let id = int item.SubItems.[0].Text
                let title = item.SubItems.[1].Text

                if MessageBox.Show($"Delete \"{title}\"?", "Confirm",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes then
                    if not (DeleteBook id) then
                        MessageBox.Show("Cannot delete borrowed book.") |> ignore
        )

        this.FormClosing.Add(fun _ -> SaveData())
