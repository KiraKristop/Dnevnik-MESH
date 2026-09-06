using System.Text.Json;

namespace MeshDiary;

public partial class Form1 : Form
{
    private TextBox txtToken = null!;
    private Button btnLogin = null!;
    private Button btnLogout = null!;
    private Label lblUser = null!;
    private Label lblStatus = null!;
    private Button[] tabButtons = null!;
    private Panel[] tabPanels = null!;
    private DataGridView gridSchedule = null!;
    private DataGridView gridMarks = null!;
    private DataGridView gridHw = null!;
    private DataGridView gridNotif = null!;
    private DateTimePicker dtSchedule = null!;
    private DateTimePicker dtMarksFrom = null!;
    private DateTimePicker dtMarksTo = null!;
    private DateTimePicker dtHwFrom = null!;
    private DateTimePicker dtHwTo = null!;
    private int activeTab = 0;

    private MeshClient? client;

    private readonly Color BgMain = Color.FromArgb(14, 22, 38);
    private readonly Color BgCard = Color.FromArgb(22, 34, 54);
    private readonly Color BgInput = Color.FromArgb(30, 43, 66);
    private readonly Color Border = Color.FromArgb(45, 60, 85);
    private readonly Color Accent = Color.FromArgb(56, 189, 248);
    private readonly Color AccentHover = Color.FromArgb(14, 165, 233);
    private readonly Color TextMain = Color.FromArgb(241, 245, 249);
    private readonly Color TextMuted = Color.FromArgb(148, 163, 184);
    private readonly Color Success = Color.FromArgb(74, 222, 128);
    private readonly Color Danger = Color.FromArgb(248, 113, 113);

    public Form1()
    {
        InitializeComponent();
        BuildUI();
    }

    private Panel footer = null!;
    private LinkLabel linkSite = null!;
    private LinkLabel linkTg = null!;

    private void BuildUI()
    {
        Font = new Font("Segoe UI", 9);
        BackColor = BgMain;
        AutoScaleMode = AutoScaleMode.Dpi;

        // ===== FOOTER — ссылки KiraKris (всегда внизу) =====
        footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 26,
            BackColor = Color.FromArgb(18, 28, 48),
            Padding = new Padding(12, 0, 12, 0)
        };
        var lblFooter = new Label
        {
            Text = "© DnevnikMESH By KiraKris  •  ",
            ForeColor = TextMuted,
            Font = new Font("Segoe UI", 8f),
            AutoSize = true,
            Location = new Point(12, 6)
        };
        linkSite = new LinkLabel
        {
            Text = "kirakristop.vibecoder.help",
            LinkColor = Accent,
            ActiveLinkColor = AccentHover,
            VisitedLinkColor = Accent,
            Font = new Font("Segoe UI", 8f, FontStyle.Underline),
            AutoSize = true,
            Location = new Point(lblFooter.Right + 2, 6)
        };
        linkSite.LinkClicked += (s, e) => OpenUrl("https://kirakristop.vibecoder.help/");
        // меряем ширину после текста
        lblFooter.AutoSize = true;
        linkSite.Location = new Point(175, 6);
        var lblSep = new Label { Text = " • ", ForeColor = TextMuted, Font = new Font("Segoe UI", 8f), AutoSize = true, Location = new Point(330, 6) };
        linkTg = new LinkLabel
        {
            Text = "t.me/KiraKris02",
            LinkColor = Accent,
            ActiveLinkColor = AccentHover,
            VisitedLinkColor = Accent,
            Font = new Font("Segoe UI", 8f, FontStyle.Underline),
            AutoSize = true,
            Location = new Point(350, 6)
        };
        linkTg.LinkClicked += (s, e) => OpenUrl("https://t.me/KiraKris02");
        footer.Controls.Add(lblFooter);
        footer.Controls.Add(linkSite);
        footer.Controls.Add(lblSep);
        footer.Controls.Add(linkTg);
        Controls.Add(footer);

        // ===== TOP CARD — используем Dock Top ряды, а не абсолютные координаты =====
        var pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 150,
            BackColor = BgCard,
            Padding = new Padding(20, 10, 20, 10)
        };
        Controls.Add(pnlTop);

        // ряды сверху вниз — Dock = Top, поэтому добавляем в обратном порядке
        var pnlStatusRow = new Panel { Dock = DockStyle.Top, Height = 18, BackColor = Color.Transparent };
        lblStatus = new Label { Dock = DockStyle.Fill, ForeColor = TextMuted, Font = new Font("Segoe UI", 8f), Text = "", TextAlign = ContentAlignment.MiddleLeft };
        pnlStatusRow.Controls.Add(lblStatus);

        var pnlUserRow = new Panel { Dock = DockStyle.Top, Height = 20, BackColor = Color.Transparent };
        lblUser = new Label { Dock = DockStyle.Fill, ForeColor = TextMuted, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Text = "●  Не вошел — вставь токен и нажми Войти" };
        pnlUserRow.Controls.Add(lblUser);

        var pnlTokenRow = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.Transparent, Padding = new Padding(0, 4, 0, 4) };
        btnLogout = CreateGhostButton("Выйти", new Size(84, 28));
        btnLogout.Dock = DockStyle.Right;
        btnLogout.Margin = new Padding(8, 0, 0, 0);
        btnLogout.Click += (s, e) => { client = null; lblUser.Text = "●  Не вошел — вставь токен и нажми Войти"; lblUser.ForeColor = TextMuted; lblStatus.Text = ""; SetTabsEnabled(false); UpdateTabButtons(); };

        btnLogin = CreatePrimaryButton("Войти", new Size(96, 28));
        btnLogin.Dock = DockStyle.Right;
        btnLogin.Margin = new Padding(8, 0, 0, 0);
        btnLogin.Click += async (s, e) => await DoLogin();

        // панель для кнопок справа, чтобы не растягивались
        var pnlButtons = new Panel { Dock = DockStyle.Right, Width = 196, BackColor = Color.Transparent };
        pnlButtons.Controls.Add(btnLogout);
        pnlButtons.Controls.Add(btnLogin);
        // порядок Dock Right — последний добавленный правее, поэтому добавляем в порядке Выйти, Войти
        btnLogin.BringToFront();

        txtToken = new TextBox
        {
            PlaceholderText = "eyJhbGciOiJSUzI1NiJ9...  (можно вставить всю строку куков с aupd_token=)",
            Font = new Font("Consolas", 9f),
            BackColor = BgInput,
            ForeColor = TextMain,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 8, 0)
        };
        pnlTokenRow.Controls.Add(txtToken);
        pnlTokenRow.Controls.Add(pnlButtons);

        var lblHint = new Label
        {
            Dock = DockStyle.Top,
            Height = 16,
            ForeColor = TextMuted,
            Font = new Font("Segoe UI", 7.5f),
            Text = "где взять: school.mos.ru  →  F12  →  Network  →  Fetch/XHR  →  userinfo  →  Request Headers  →  Authorization: Bearer <токен>",
            TextAlign = ContentAlignment.MiddleLeft
        };
        var lblTitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 18,
            ForeColor = TextMuted,
            Font = new Font("Segoe UI", 7f, FontStyle.Bold),
            Text = "DnevnikMESH  •  By KiraKris  —  ВХОД ПО ТОКЕНУ МЭШ",
            TextAlign = ContentAlignment.MiddleLeft
        };
        // верх — бренд
        var lblBrand = new Label
        {
            Dock = DockStyle.Top,
            Height = 16,
            ForeColor = Accent,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            Text = "DnevnikMESH By KiraKris",
            TextAlign = ContentAlignment.MiddleLeft
        };

        // добавляем сверху вниз — Dock Top стакает, поэтому добавляем в обратном порядке: сначала низ, потом верх
        pnlTop.Controls.Add(pnlStatusRow);
        pnlTop.Controls.Add(pnlUserRow);
        pnlTop.Controls.Add(pnlTokenRow);
        pnlTop.Controls.Add(lblHint);
        pnlTop.Controls.Add(lblTitle);
        pnlTop.Controls.Add(lblBrand);

        var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Border };
        Controls.Add(sep);

        // ===== TAB BAR =====
        var pnlTabBar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = BgMain, Padding = new Padding(20, 8, 20, 8) };
        Controls.Add(pnlTabBar);
        string[] tabNames = ["📅  Расписание", "⭐  Оценки", "📝  Домашка", "🔔  Уведомления"];
        tabButtons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            var b = new Button
            {
                Text = tabNames[i],
                Size = new Size(152, 32),
                Location = new Point(20 + i * 160, 8),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = idx
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (s, e) => SwitchTab(idx);
            tabButtons[i] = b;
            pnlTabBar.Controls.Add(b);
        }
        UpdateTabButtons();

        // ===== CONTENT =====
        var pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = BgMain, Padding = new Padding(20, 4, 20, 20) };
        Controls.Add(pnlContent);

        tabPanels = new Panel[4];
        for (int i = 0; i < 4; i++)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = BgCard, Padding = new Padding(10), Visible = false };
            p.Paint += (s, e) =>
            {
                var r = ((Panel)s!).ClientRectangle;
                r.Inflate(-1, -1);
                using var pen = new Pen(Border, 1);
                e.Graphics.DrawRectangle(pen, r);
            };
            tabPanels[i] = p;
            pnlContent.Controls.Add(p);
        }
        tabPanels[0].Visible = true;

        BuildScheduleTab(tabPanels[0]);
        BuildMarksTab(tabPanels[1]);
        BuildHwTab(tabPanels[2]);
        BuildNotifTab(tabPanels[3]);

        SetTabsEnabled(false);

        Controls.SetChildIndex(pnlContent, 0);
        Controls.SetChildIndex(pnlTabBar, 1);
        Controls.SetChildIndex(sep, 2);
        Controls.SetChildIndex(pnlTop, 3);
        Controls.SetChildIndex(footer, 4);
    }

    private void OpenUrl(string url)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show("Не удалось открыть ссылку:\n" + url + "\n\n" + ex.Message); }
    }

    private void BuildScheduleTab(Panel host)
    {
        // ВАЖНО: сначала grid (Fill), потом top (Top) — иначе Dock перекрывает заголовки
        gridSchedule = MakeGrid(new[] { "Предмет", "Начало", "Кабинет" }, new[] { 600, 140, 140 });
        var top = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 6) };
        var lbl = new Label { Text = "Дата", ForeColor = TextMuted, Font = new Font("Segoe UI", 8f, FontStyle.Bold), AutoSize = true, Location = new Point(4, 12), TextAlign = ContentAlignment.MiddleLeft };
        dtSchedule = StyledDatePicker(new Point(48, 8));
        var btn = CreatePrimaryButton("Показать", new Size(118, 28));
        btn.Location = new Point(210, 8);
        btn.Click += async (s, e) => await LoadSchedule();
        top.Controls.Add(lbl); top.Controls.Add(dtSchedule); top.Controls.Add(btn);
        host.Controls.Add(gridSchedule);
        host.Controls.Add(top);
    }

    private void BuildMarksTab(Panel host)
    {
        gridMarks = MakeGrid(new[] { "Предмет", "Оценка", "Вес", "Форма", "Дата", "Комментарий" }, new[] { 240, 80, 60, 170, 110, 240 });
        var top = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 6) };
        var l1 = new Label { Text = "С", ForeColor = TextMuted, Font = new Font("Segoe UI", 8f, FontStyle.Bold), AutoSize = true, Location = new Point(4, 12) };
        var l2 = new Label { Text = "по", ForeColor = TextMuted, Font = new Font("Segoe UI", 8f, FontStyle.Bold), AutoSize = true, Location = new Point(216, 12) };
        dtMarksFrom = StyledDatePicker(new Point(24, 8));
        dtMarksFrom.Value = DateTime.Today.AddDays(-7);
        dtMarksTo = StyledDatePicker(new Point(242, 8));
        var btn = CreatePrimaryButton("Показать", new Size(118, 28));
        btn.Location = new Point(410, 8);
        btn.Click += async (s, e) => await LoadMarks();
        top.Controls.AddRange([l1, dtMarksFrom, l2, dtMarksTo, btn]);
        host.Controls.Add(gridMarks);
        host.Controls.Add(top);
    }

    private void BuildHwTab(Panel host)
    {
        gridHw = MakeGrid(new[] { "Предмет", "Описание", "Дата", "Готово" }, new[] { 180, 520, 110, 70 });
        var top = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 6) };
        var l1 = new Label { Text = "С", ForeColor = TextMuted, AutoSize = true, Location = new Point(4, 12), Font = new Font("Segoe UI", 8f, FontStyle.Bold) };
        var l2 = new Label { Text = "по", ForeColor = TextMuted, AutoSize = true, Location = new Point(216, 12), Font = new Font("Segoe UI", 8f, FontStyle.Bold) };
        dtHwFrom = StyledDatePicker(new Point(24, 8));
        dtHwTo = StyledDatePicker(new Point(242, 8));
        var btn = CreatePrimaryButton("Показать", new Size(118, 28));
        btn.Location = new Point(410, 8);
        btn.Click += async (s, e) => await LoadHw();
        top.Controls.AddRange([l1, dtHwFrom, l2, dtHwTo, btn]);
        host.Controls.Add(gridHw);
        host.Controls.Add(top);
    }

    private void BuildNotifTab(Panel host)
    {
        gridNotif = MakeGrid(new[] { "Дата", "Предмет", "Событие", "Описание" }, new[] { 150, 170, 140, 420 });
        var top = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 6) };
        var btn = CreatePrimaryButton("↻  Обновить", new Size(128, 28));
        btn.Location = new Point(4, 8);
        btn.Click += async (s, e) => await LoadNotif();
        top.Controls.Add(btn);
        host.Controls.Add(gridNotif);
        host.Controls.Add(top);
    }

    private DateTimePicker StyledDatePicker(Point loc)
    {
        return new DateTimePicker
        {
            Value = DateTime.Today,
            Format = DateTimePickerFormat.Short,
            Width = 152,
            Height = 28,
            Location = loc,
            Font = new Font("Segoe UI", 9f)
        };
    }

    private Button CreatePrimaryButton(string text, Size size)
    {
        var b = new Button
        {
            Text = text,
            Size = size,
            FlatStyle = FlatStyle.Flat,
            BackColor = Accent,
            ForeColor = Color.FromArgb(14, 22, 38),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        b.MouseEnter += (s, e) => b.BackColor = AccentHover;
        b.MouseLeave += (s, e) => b.BackColor = Accent;
        return b;
    }

    private Button CreatePrimaryButton(string text, Point loc, Size size)
    {
        var b = CreatePrimaryButton(text, size);
        b.Location = loc;
        return b;
    }

    private Button CreateGhostButton(string text, Size size)
    {
        var b = new Button
        {
            Text = text,
            Size = size,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = TextMuted,
            Font = new Font("Segoe UI", 9f),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = Border;
        b.FlatAppearance.BorderSize = 1;
        return b;
    }

    private void SwitchTab(int idx)
    {
        activeTab = idx;
        for (int i = 0; i < tabPanels.Length; i++) tabPanels[i].Visible = i == idx;
        UpdateTabButtons();
    }

    private void UpdateTabButtons()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            bool active = i == activeTab;
            tabButtons[i].BackColor = active ? Accent : BgCard;
            tabButtons[i].ForeColor = active ? Color.FromArgb(14, 22, 38) : TextMuted;
            tabButtons[i].FlatAppearance.BorderColor = active ? Accent : Border;
            tabButtons[i].FlatAppearance.BorderSize = 1;
        }
    }

    private void SetTabsEnabled(bool enabled)
    {
        foreach (var b in tabButtons) b.Enabled = enabled;
        foreach (var p in tabPanels) p.Enabled = enabled;
        if (!enabled) { foreach (var p in tabPanels) p.Visible = false; tabPanels[0].Visible = true; activeTab = 0; }
        else tabPanels[activeTab].Visible = true;
        UpdateTabButtons();
    }

    private DataGridView MakeGrid(string[] cols, int[] widths)
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = BgCard,
            ForeColor = TextMain,
            GridColor = Border,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            ColumnHeadersHeight = 30,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            RowTemplate = { Height = 28 },
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            EnableHeadersVisualStyles = false,
            ScrollBars = ScrollBars.Both
        };
        g.DefaultCellStyle.BackColor = BgCard;
        g.DefaultCellStyle.ForeColor = TextMain;
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(36, 56, 84);
        g.DefaultCellStyle.SelectionForeColor = TextMain;
        g.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
        g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 43, 66);
        g.ColumnHeadersDefaultCellStyle.ForeColor = TextMuted;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
        g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(26, 39, 62);

        for (int i = 0; i < cols.Length; i++)
        {
            var c = new DataGridViewTextBoxColumn
            {
                HeaderText = cols[i].ToUpper(),
                Name = cols[i],
                Width = widths[i],
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            g.Columns.Add(c);
        }
        if (cols.Length > 0) g.Columns[cols.Length - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        return g;
    }

    private async Task DoLogin()
    {
        var token = txtToken.Text.Trim();
        if (string.IsNullOrWhiteSpace(token)) { lblStatus.Text = "⚠  Вставь токен"; lblStatus.ForeColor = Danger; return; }
        if (token.StartsWith("Bearer ")) token = token[7..];
        if (token.Contains("aupd_token="))
        {
            var start = token.IndexOf("aupd_token=") + 11;
            var end = token.IndexOf(";", start);
            if (end == -1) end = token.Length;
            token = token[start..end].Trim();
            txtToken.Text = token;
        }
        btnLogin.Enabled = false; lblStatus.Text = "⏳  Проверяю токен..."; lblStatus.ForeColor = TextMuted;
        try
        {
            client = new MeshClient(token);
            var ui = await client.GetUserInfoAsync();
            var prof = await client.GetProfileAsync();
            lblUser.Text = $"●  {ui.Info.LastName} {ui.Info.FirstName} {ui.Info.MiddleName}  —  {prof.ClassName}  •  {prof.SchoolName}  •  ID {prof.StudentId}";
            lblUser.ForeColor = Success;
            lblStatus.Text = $"✓  Успешно  •  {ui.Login}  •  {ui.Info.Mail}  •  Расписание {DateTime.Today:yyyy-MM-dd}: загружается...";
            lblStatus.ForeColor = Success;
            SetTabsEnabled(true);
            SwitchTab(0);
            await LoadSchedule();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "✕  Ошибка: " + ex.Message;
            lblStatus.ForeColor = Danger;
            MessageBox.Show(ex.Message, "DnevnikMESH — ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { btnLogin.Enabled = true; }
    }

    private async Task LoadSchedule()
    {
        if (client == null) return;
        try
        {
            var date = dtSchedule.Value.ToString("yyyy-MM-dd");
            var doc = await client.GetScheduleAsync(date);
            var acts = doc.RootElement.GetProperty("activities");
            gridSchedule.Rows.Clear();
            foreach (var a in acts.EnumerateArray())
            {
                if (a.TryGetProperty("type", out var t) && t.GetString() == "BREAK") continue;
                var begin = a.TryGetProperty("begin_time", out var bt) ? bt.GetString() ?? "" : "";
                var room = a.TryGetProperty("room_number", out var rn) ? (rn.ValueKind == JsonValueKind.Null ? "—" : rn.GetString() ?? "—") : "—";
                string subj = a.TryGetProperty("lesson", out var lesson) && lesson.TryGetProperty("subject_name", out var sn) ? sn.GetString() ?? "" : "";
                // если обрезанное название группы — показываем полностью
                if (subj.Length > 60) subj = subj[..60] + "…";
                gridSchedule.Rows.Add(subj, begin, room);
            }
            lblStatus.Text = gridSchedule.Rows.Count == 0 ? $"—  На {date} уроков нет" : $"✓  Расписание {date}: {gridSchedule.Rows.Count} уроков";
            lblStatus.ForeColor = gridSchedule.Rows.Count == 0 ? TextMuted : Success;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Расписание"); }
    }

    private async Task LoadMarks()
    {
        if (client == null) return;
        try
        {
            var from = dtMarksFrom.Value.ToString("yyyy-MM-dd");
            var to = dtMarksTo.Value.ToString("yyyy-MM-dd");
            var doc = await client.GetMarksAsync(from, to);
            var payload = doc.RootElement.GetProperty("payload");
            gridMarks.Rows.Clear();
            foreach (var m in payload.EnumerateArray())
            {
                var subj = m.GetProperty("subject_name").GetString() ?? "";
                var val = m.GetProperty("value").GetString() ?? m.GetProperty("value").ToString();
                var weight = m.TryGetProperty("weight", out var w) ? w.GetInt32().ToString() : "";
                var form = m.TryGetProperty("control_form_name", out var cf) ? cf.GetString() ?? "" : "";
                var date = m.TryGetProperty("created_at", out var ca) ? ca.GetString() ?? "" : "";
                var comment = m.TryGetProperty("comment", out var cm) ? cm.GetString() ?? "" : "";
                gridMarks.Rows.Add(subj, val, weight, form, date.Length >= 10 ? date[..10] : date, comment);
            }
            lblStatus.Text = $"✓  Оценок: {gridMarks.Rows.Count}  •  {from} → {to}";
            lblStatus.ForeColor = Success;
            foreach (DataGridViewRow r in gridMarks.Rows)
            {
                var v = r.Cells[1].Value?.ToString();
                r.Cells[1].Style.ForeColor = v == "5" ? Color.FromArgb(74, 222, 128) : v == "4" ? Color.FromArgb(163, 230, 53) : v == "3" ? Color.FromArgb(251, 191, 36) : v == "2" ? Danger : TextMain;
                r.Cells[1].Style.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                r.Cells[1].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Оценки"); }
    }

    private async Task LoadHw()
    {
        if (client == null) return;
        try
        {
            var from = dtHwFrom.Value.ToString("yyyy-MM-dd");
            var to = dtHwTo.Value.ToString("yyyy-MM-dd");
            var doc = await client.GetHomeworksAsync(from, to);
            var payload = doc.RootElement.GetProperty("payload");
            gridHw.Rows.Clear();
            foreach (var h in payload.EnumerateArray())
            {
                var subj = h.GetProperty("subject_name").GetString() ?? "";
                var desc = h.GetProperty("description").GetString() ?? "";
                var date = h.TryGetProperty("date_assigned_on", out var da) ? da.GetString() ?? "" : "";
                var done = h.TryGetProperty("is_done", out var d) ? (d.GetBoolean() ? "✓" : "—") : "—";
                gridHw.Rows.Add(subj, desc, date.Length >= 10 ? date[..10] : date, done);
            }
            lblStatus.Text = $"✓  Домашка: {gridHw.Rows.Count}  •  {from} → {to}";
            lblStatus.ForeColor = Success;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Домашка"); }
    }

    private async Task LoadNotif()
    {
        if (client == null) return;
        try
        {
            var doc = await client.GetNotificationsAsync();
            gridNotif.Rows.Clear();
            foreach (var e in doc.RootElement.EnumerateArray())
            {
                var dt = e.TryGetProperty("created_at", out var ca) ? ca.GetString() ?? e.GetProperty("datetime").GetString() ?? "" : "";
                var subj = e.TryGetProperty("subject_name", out var sn) ? sn.GetString() ?? "" : "";
                var type = e.GetProperty("event_type").GetString() ?? "";
                string desc = type switch
                {
                    "create_homework" or "update_homework" => e.TryGetProperty("new_hw_description", out var d) ? d.GetString() ?? "" : "",
                    "create_mark" or "update_mark" => "Оценка: " + (e.TryGetProperty("new_mark_value", out var mv) ? mv.GetString() ?? "" : ""),
                    "delete_mark" => "Удалена: " + (e.TryGetProperty("old_mark_value", out var om) ? om.GetString() ?? "" : ""),
                    _ => e.GetRawText().Length > 80 ? e.GetRawText()[..80] : e.GetRawText()
                };
                gridNotif.Rows.Add(dt.Length >= 19 ? dt[..19] : dt, subj, type, desc);
            }
            lblStatus.Text = $"✓  Событий: {gridNotif.Rows.Count}";
            lblStatus.ForeColor = Success;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Уведомления"); }
    }
}
