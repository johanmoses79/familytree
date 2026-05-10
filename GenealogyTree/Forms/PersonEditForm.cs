using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GenealogyTree
{

    public class PersonEditForm : Form
    {
        private readonly FamilyRepository _repository;
        private readonly Person? _existing;


        private TextBox txtLastName = null!;
        private TextBox txtFirstName = null!;
        private TextBox txtPatronymic = null!;
        private DateTimePicker dtpBirth = null!;
        private CheckBox chkBirthUnknown = null!;
        private DateTimePicker dtpDeath = null!;
        private CheckBox chkAlive = null!;
        private TextBox txtBirthPlace = null!;
        private TextBox txtCitizenship = null!;
        private ComboBox cmbGender = null!;
        private ComboBox cmbFather = null!;
        private ComboBox cmbMother = null!;
        private TextBox txtNotes = null!;
        private Label lblError = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;

        public PersonEditForm(FamilyRepository repository, Person? existing)
        {
            _repository = repository;
            _existing = existing;
            BuildUI();
            PopulateParentCombos();
            if (existing != null) FillForm(existing);
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
                if (e.KeyCode == Keys.Enter && !txtNotes.Focused) { btnOk_Click(this, EventArgs.Empty); e.SuppressKeyPress = true; }
            };
        }

        private void BuildUI()
        {
            Text = _existing == null ? "Додавання особи" : "Редагування особи";
            Size = new Size(480, 520);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9f);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(12),
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            AddRow(layout, "Прізвище *", txtLastName = new TextBox(), ref row);
            AddRow(layout, "Ім'я *", txtFirstName = new TextBox(), ref row);
            AddRow(layout, "По батькові", txtPatronymic = new TextBox(), ref row);

   
            dtpBirth = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = new DateTime(1980, 1, 1) };
            chkBirthUnknown = new CheckBox { Text = "невідома", AutoSize = true };
            chkBirthUnknown.CheckedChanged += (s, e) => dtpBirth.Enabled = !chkBirthUnknown.Checked;
            var birthPanel = new FlowLayoutPanel { AutoSize = true };
            birthPanel.Controls.Add(dtpBirth);
            birthPanel.Controls.Add(chkBirthUnknown);
            AddRow(layout, "Дата народження", birthPanel, ref row);

   
            dtpDeath = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Enabled = false };
            chkAlive = new CheckBox { Text = "жива особа", AutoSize = true, Checked = true };
            chkAlive.CheckedChanged += (s, e) => dtpDeath.Enabled = !chkAlive.Checked;
            var deathPanel = new FlowLayoutPanel { AutoSize = true };
            deathPanel.Controls.Add(dtpDeath);
            deathPanel.Controls.Add(chkAlive);
            AddRow(layout, "Дата смерті", deathPanel, ref row);

            AddRow(layout, "Місце народження", txtBirthPlace = new TextBox(), ref row);
            AddRow(layout, "Громадянство", txtCitizenship = new TextBox { Text = "Україна" }, ref row);

            cmbGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new object[] { "Чоловіча", "Жіноча", "Невідома" });
            cmbGender.SelectedIndex = 2;
            AddRow(layout, "Стать", cmbGender, ref row);

            cmbFather = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddRow(layout, "Батько", cmbFather, ref row);

            cmbMother = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddRow(layout, "Мати", cmbMother, ref row);

            txtNotes = new TextBox { Multiline = true, Height = 55, ScrollBars = ScrollBars.Vertical };
            AddRow(layout, "Примітки", txtNotes, ref row);

 
            lblError = new Label
            {
                ForeColor = Color.Red,
                AutoSize = false,
                Size = new Size(420, 36),
                Visible = false
            };
            layout.Controls.Add(new Label(), 0, row);
            layout.Controls.Add(lblError, 1, row++);

            btnOk = new Button { Text = "OK", DialogResult = DialogResult.None, Width = 90, Height = 30, BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += btnOk_Click;
            btnCancel = new Button { Text = "Скасувати", DialogResult = DialogResult.Cancel, Width = 100, Height = 30 };
            var btnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            btnPanel.Controls.Add(btnOk);
            btnPanel.Controls.Add(btnCancel);
            layout.Controls.Add(new Label(), 0, row);
            layout.Controls.Add(btnPanel, 1, row);

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            scroll.Controls.Add(layout);
            Controls.Add(scroll);
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private static void AddRow(TableLayoutPanel tbl, string caption, Control ctrl, ref int row)
        {
            tbl.Controls.Add(new Label { Text = caption, AutoSize = false, Width = 145, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            if (ctrl is TextBox tb) tb.Dock = DockStyle.Fill;
            if (ctrl is ComboBox cb) cb.Dock = DockStyle.Fill;
            tbl.Controls.Add(ctrl, 1, row);
            row++;
        }

        private void PopulateParentCombos()
        {
            var none = new ComboItem(null, "— не вказано —");
            var males = _repository.GetAll().Where(p => p.Gender == Gender.Male || p.Gender == Gender.Unknown).Select(p => new ComboItem(p.Id, p.DisplayInfo)).ToList();
            var females = _repository.GetAll().Where(p => p.Gender == Gender.Female || p.Gender == Gender.Unknown).Select(p => new ComboItem(p.Id, p.DisplayInfo)).ToList();

            // Exclude self
            if (_existing != null)
            {
                males.RemoveAll(m => m.Id == _existing.Id);
                females.RemoveAll(f => f.Id == _existing.Id);
            }

            cmbFather.Items.Add(none);
            cmbFather.Items.AddRange(males.ToArray<object>());
            cmbFather.SelectedIndex = 0;

            cmbMother.Items.Add(none);
            cmbMother.Items.AddRange(females.ToArray<object>());
            cmbMother.SelectedIndex = 0;
        }

        private void FillForm(Person p)
        {
            txtLastName.Text = p.LastName;
            txtFirstName.Text = p.FirstName;
            txtPatronymic.Text = p.Patronymic;

            if (p.BirthDate.HasValue) { dtpBirth.Value = p.BirthDate.Value; chkBirthUnknown.Checked = false; }
            else chkBirthUnknown.Checked = true;

            if (p.DeathDate.HasValue) { dtpDeath.Value = p.DeathDate.Value; chkAlive.Checked = false; }
            else chkAlive.Checked = true;

            txtBirthPlace.Text = p.BirthPlace ?? "";
            txtCitizenship.Text = p.Citizenship ?? "";
            cmbGender.SelectedIndex = p.Gender == Gender.Male ? 0 : p.Gender == Gender.Female ? 1 : 2;
            txtNotes.Text = p.Notes ?? "";

            SelectParentCombo(cmbFather, p.FatherId);
            SelectParentCombo(cmbMother, p.MotherId);
        }

        private void SelectParentCombo(ComboBox cmb, int? id)
        {
            if (!id.HasValue) { cmb.SelectedIndex = 0; return; }
            for (int i = 0; i < cmb.Items.Count; i++)
                if (cmb.Items[i] is ComboItem ci && ci.Id == id) { cmb.SelectedIndex = i; return; }
            cmb.SelectedIndex = 0;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var errors = ValidateFields();
            if (errors.Count > 0)
            {
                lblError.Text = string.Join("\n", errors);
                lblError.Visible = true;
                return;
            }
            lblError.Visible = false;

            var person = _existing ?? new Person();
            person.LastName = txtLastName.Text.Trim();
            person.FirstName = txtFirstName.Text.Trim();
            person.Patronymic = txtPatronymic.Text.Trim();
            person.BirthDate = chkBirthUnknown.Checked ? null : dtpBirth.Value;
            person.DeathDate = chkAlive.Checked ? null : dtpDeath.Value;
            person.BirthPlace = txtBirthPlace.Text.Trim().NullIfEmpty();
            person.Citizenship = txtCitizenship.Text.Trim().NullIfEmpty();
            person.Gender = cmbGender.SelectedIndex == 0 ? Gender.Male : cmbGender.SelectedIndex == 1 ? Gender.Female : Gender.Unknown;
            person.FatherId = (cmbFather.SelectedItem as ComboItem)?.Id;
            person.MotherId = (cmbMother.SelectedItem as ComboItem)?.Id;
            person.Notes = txtNotes.Text.Trim().NullIfEmpty();

            if (_existing == null) _repository.Add(person);
            else _repository.Update(person);

            DialogResult = DialogResult.OK;
            Close();
        }

        private List<string> ValidateFields()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(txtLastName.Text)) errors.Add("• Прізвище є обов'язковим полем.");
            if (string.IsNullOrWhiteSpace(txtFirstName.Text)) errors.Add("• Ім'я є обов'язковим полем.");
            if (!chkBirthUnknown.Checked && !chkAlive.Checked && dtpDeath.Value < dtpBirth.Value)
                errors.Add("• Дата смерті не може бути раніше дати народження.");
            return errors;
        }

        private class ComboItem
        {
            public int? Id { get; }
            private readonly string _display;
            public ComboItem(int? id, string display) { Id = id; _display = display; }
            public override string ToString() => _display;
        }
    }

    internal static class StringExtensions
    {
        public static string? NullIfEmpty(this string s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
