namespace GenealogyTree
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchHint;
        private System.Windows.Forms.ListBox lstPersons;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAncestors;
        private System.Windows.Forms.Button btnDescendants;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;

        private System.Windows.Forms.Panel pnlDetails;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblLblBirth;
        private System.Windows.Forms.Label lblBirth;
        private System.Windows.Forms.Label lblLblDeath;
        private System.Windows.Forms.Label lblDeath;
        private System.Windows.Forms.Label lblLblBirthPlace;
        private System.Windows.Forms.Label lblBirthPlace;
        private System.Windows.Forms.Label lblLblCitizenship;
        private System.Windows.Forms.Label lblCitizenship;
        private System.Windows.Forms.Label lblLblFather;
        private System.Windows.Forms.Label lblFather;
        private System.Windows.Forms.Label lblLblMother;
        private System.Windows.Forms.Label lblMother;
        private System.Windows.Forms.Label lblLblChildren;
        private System.Windows.Forms.Label lblChildren;
        private System.Windows.Forms.Label lblLblNotes;
        private System.Windows.Forms.Label lblNotes;

        private System.Windows.Forms.Panel pnlTree;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Text = "Генеалогічне дерево";
            this.Size = new System.Drawing.Size(1100, 700);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Font = new System.Drawing.Font("Segoe UI", 9f);

            pnlLeft = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Left,
                Width = 270,
                Padding = new System.Windows.Forms.Padding(8)
            };

            lblSearchHint = new System.Windows.Forms.Label
            {
                Text = "Пошук за прізвищем або ім'ям:",
                AutoSize = true,
                Location = new System.Drawing.Point(8, 8)
            };

            txtSearch = new System.Windows.Forms.TextBox
            {
                Location = new System.Drawing.Point(8, 28),
                Width = 248,
                PlaceholderText = "Введіть для пошуку..."
            };
            txtSearch.TextChanged += txtSearch_TextChanged;

            lstPersons = new System.Windows.Forms.ListBox
            {
                Location = new System.Drawing.Point(8, 58),
                Size = new System.Drawing.Size(248, 380),
                IntegralHeight = false,
                ItemHeight = 18
            };
            lstPersons.SelectedIndexChanged += lstPersons_SelectedIndexChanged;

            int btnY = 450;
            int btnH = 30;
            int btnW = 120;

            btnAdd = MakeButton("Додати", new System.Drawing.Point(8, btnY), btnW, btnH, System.Drawing.Color.FromArgb(70, 130, 180));
            btnAdd.Click += btnAdd_Click;
            btnEdit = MakeButton("Редагувати", new System.Drawing.Point(136, btnY), btnW, btnH, System.Drawing.Color.FromArgb(100, 160, 100));
            btnEdit.Click += btnEdit_Click;

            btnDelete = MakeButton("Видалити", new System.Drawing.Point(8, btnY + 38), 248, btnH, System.Drawing.Color.FromArgb(180, 80, 80));
            btnDelete.Click += btnDelete_Click;

            btnAncestors = MakeButton("Усі предки", new System.Drawing.Point(8, btnY + 80), btnW, btnH, System.Drawing.Color.FromArgb(130, 100, 160));
            btnAncestors.Click += btnAncestors_Click;
            btnDescendants = MakeButton("Усі нащадки", new System.Drawing.Point(136, btnY + 80), btnW, btnH, System.Drawing.Color.FromArgb(130, 100, 160));
            btnDescendants.Click += btnDescendants_Click;

            btnSave = MakeButton("Зберегти", new System.Drawing.Point(8, btnY + 122), btnW, btnH, System.Drawing.Color.FromArgb(90, 90, 90));
            btnSave.Click += btnSave_Click;
            btnLoad = MakeButton("Завантажити", new System.Drawing.Point(136, btnY + 122), btnW, btnH, System.Drawing.Color.FromArgb(90, 90, 90));
            btnLoad.Click += btnLoad_Click;

            pnlLeft.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblSearchHint, txtSearch, lstPersons,
                btnAdd, btnEdit, btnDelete,
                btnAncestors, btnDescendants,
                btnSave, btnLoad
            });

            pnlDetails = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 240,
                Padding = new System.Windows.Forms.Padding(10)
            };

            lblName = new System.Windows.Forms.Label
            {
                Text = "—",
                Font = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold),
                AutoSize = false,
                Location = new System.Drawing.Point(10, 8),
                Size = new System.Drawing.Size(590, 28)
            };

            int rowY = 40;
            AddDetailRow(pnlDetails, "Дата народження:", out lblLblBirth, out lblBirth, rowY); rowY += 22;
            AddDetailRow(pnlDetails, "Дата смерті:", out lblLblDeath, out lblDeath, rowY); rowY += 22;
            AddDetailRow(pnlDetails, "Місце народження:", out lblLblBirthPlace, out lblBirthPlace, rowY); rowY += 22;
            AddDetailRow(pnlDetails, "Громадянство:", out lblLblCitizenship, out lblCitizenship, rowY); rowY += 22;
            AddDetailRow(pnlDetails, "Батько:", out lblLblFather, out lblFather, rowY); rowY += 22;
            AddDetailRow(pnlDetails, "Мати:", out lblLblMother, out lblMother, rowY); rowY += 22;
            AddDetailRow(pnlDetails, "Діти:", out lblLblChildren, out lblChildren, rowY); rowY += 22;
            AddDetailRow(pnlDetails, "Примітки:", out lblLblNotes, out lblNotes, rowY);

            pnlDetails.Controls.Add(lblName);

            pnlTree = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                AutoScroll = true,
                BackColor = System.Drawing.Color.White
            };

            var pnlRight = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Fill };
            pnlRight.Controls.Add(pnlTree);
            pnlRight.Controls.Add(pnlDetails);

            var splitter = new System.Windows.Forms.Splitter
            {
                Dock = System.Windows.Forms.DockStyle.Left,
                Width = 4,
                BackColor = System.Drawing.Color.LightGray
            };

            this.Controls.Add(pnlRight);
            this.Controls.Add(splitter);
            this.Controls.Add(pnlLeft);
        }

        private System.Windows.Forms.Button MakeButton(string text, System.Drawing.Point loc, int w, int h, System.Drawing.Color back)
        {
            return new System.Windows.Forms.Button
            {
                Text = text,
                Location = loc,
                Size = new System.Drawing.Size(w, h),
                BackColor = back,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new System.Drawing.Font("Segoe UI", 9f)
            };
        }

        private void AddDetailRow(System.Windows.Forms.Panel parent, string caption, out System.Windows.Forms.Label lblCaption, out System.Windows.Forms.Label lblValue, int y)
        {
            lblCaption = new System.Windows.Forms.Label
            {
                Text = caption,
                AutoSize = false,
                Size = new System.Drawing.Size(160, 20),
                Location = new System.Drawing.Point(10, y),
                ForeColor = System.Drawing.Color.Gray,
                Font = new System.Drawing.Font("Segoe UI", 8.5f)
            };
            lblValue = new System.Windows.Forms.Label
            {
                Text = "—",
                AutoSize = false,
                Size = new System.Drawing.Size(430, 20),
                Location = new System.Drawing.Point(175, y),
                Font = new System.Drawing.Font("Segoe UI", 8.5f)
            };
            parent.Controls.Add(lblCaption);
            parent.Controls.Add(lblValue);
        }
    }
}
