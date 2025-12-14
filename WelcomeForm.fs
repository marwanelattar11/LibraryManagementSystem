namespace LibraryManagementSystem

open System
open System.Drawing
open System.Windows.Forms

type WelcomeForm() as this =
    inherit Form()

    do
        this.Text <- "Library Management System"
        this.Size <- Size(600, 400)
        this.StartPosition <- FormStartPosition.CenterScreen
        this.FormBorderStyle <- FormBorderStyle.FixedDialog
        this.MaximizeBox <- false
        this.BackColor <- Color.FromArgb(245, 248, 250)

        let lblTitle = new Label(
            Text = "Welcome To Library Management System",
            Dock = DockStyle.Top,
            Height = 100,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 22.0f, FontStyle.Bold),
            ForeColor = Color.DarkBlue)

        let btnUser = new Button(
            Text = " User\n(Borrow - Return - Search)",
            Size = Size(400, 90),
            Location = Point(100, 120),
            BackColor = Color.Orange,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16.0f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat)
        btnUser.FlatAppearance.BorderSize <- 0

        let btnAdmin = new Button(
            Text = "Admin\n(Add - Edit - Delete)",
            Size = Size(400, 90),
            Location = Point(100, 230),
            BackColor = Color.DarkRed,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16.0f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat)
        btnAdmin.FlatAppearance.BorderSize <- 0

        btnUser.Click.Add(fun _ ->
            this.Hide()
            (new UserForm()).ShowDialog() |> ignore
            this.Close())

        btnAdmin.Click.Add(fun _ ->
            this.Hide()
            (new AdminForm()).ShowDialog() |> ignore
            this.Close())

        this.Controls.AddRange([| lblTitle; btnUser; btnAdmin |])