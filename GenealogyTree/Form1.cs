using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GenealogyTree
{

    public partial class Form1 : Form
    {
        private FamilyRepository _repository;
        private TreeRenderer _treeRenderer;

        public Form1()
        {
            InitializeComponent();
            _repository = new FamilyRepository();
            _treeRenderer = new TreeRenderer(pnlTree);
            LoadSampleData();
            RefreshPersonList();
        }


        private void LoadSampleData()
        {
            var repo = _repository;

            var great = repo.Add(new Person { LastName = "Коваленко", FirstName = "Іван", Patronymic = "Степанович", BirthDate = new DateTime(1920, 3, 5), Gender = Gender.Male, Citizenship = "Україна", BirthPlace = "Харків" });
            var greatW = repo.Add(new Person { LastName = "Коваленко", FirstName = "Марія", Patronymic = "Олексіївна", BirthDate = new DateTime(1923, 7, 12), Gender = Gender.Female, Citizenship = "Україна", BirthPlace = "Полтава" });

            var father = repo.Add(new Person { LastName = "Коваленко", FirstName = "Петро", Patronymic = "Іванович", BirthDate = new DateTime(1950, 5, 21), Gender = Gender.Male, Citizenship = "Україна", BirthPlace = "Харків", FatherId = great.Id, MotherId = greatW.Id });
            var mother = repo.Add(new Person { LastName = "Коваленко", FirstName = "Олена", Patronymic = "Василівна", BirthDate = new DateTime(1953, 11, 3), Gender = Gender.Female, Citizenship = "Україна", BirthPlace = "Суми" });

            var child1 = repo.Add(new Person { LastName = "Коваленко", FirstName = "Олег", Patronymic = "Петрович", BirthDate = new DateTime(1975, 2, 14), Gender = Gender.Male, Citizenship = "Україна", BirthPlace = "Харків", FatherId = father.Id, MotherId = mother.Id });
            var child2 = repo.Add(new Person { LastName = "Коваленко", FirstName = "Тетяна", Patronymic = "Петрівна", BirthDate = new DateTime(1978, 8, 30), Gender = Gender.Female, Citizenship = "Україна", BirthPlace = "Харків", FatherId = father.Id, MotherId = mother.Id });

            repo.Add(new Person { LastName = "Коваленко", FirstName = "Артем", Patronymic = "Олегович", BirthDate = new DateTime(2003, 6, 1), Gender = Gender.Male, Citizenship = "Україна", BirthPlace = "Харків", FatherId = child1.Id });
            repo.Add(new Person { LastName = "Мельник", FirstName = "Софія", Patronymic = "Олегівна", BirthDate = new DateTime(2006, 4, 18), Gender = Gender.Female, Citizenship = "Україна", BirthPlace = "Харків", FatherId = child1.Id });
        }


        private void RefreshPersonList()
        {
            lstPersons.Items.Clear();
            var query = txtSearch.Text.Trim().ToLower();
            var persons = _repository.GetAll()
                .Where(p => string.IsNullOrEmpty(query) ||
                            p.FullName.ToLower().Contains(query))
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName);

            foreach (var p in persons)
                lstPersons.Items.Add(p);

            lstPersons.DisplayMember = "DisplayInfo";
        }


        private void lstPersons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPersons.SelectedItem is Person selected)
            {
                ShowDetails(selected);
                _treeRenderer.Draw(selected, _repository);
            }
        }

        private void ShowDetails(Person p)
        {
            lblName.Text = p.FullName;
            lblBirth.Text = p.BirthDate.HasValue ? p.BirthDate.Value.ToString("dd.MM.yyyy") : "—";
            lblDeath.Text = p.DeathDate.HasValue ? p.DeathDate.Value.ToString("dd.MM.yyyy") : "—";
            lblBirthPlace.Text = p.BirthPlace ?? "—";
            lblCitizenship.Text = p.Citizenship ?? "—";
            lblNotes.Text = p.Notes ?? "—";

            var father = p.FatherId.HasValue ? _repository.GetById(p.FatherId.Value) : null;
            var mother = p.MotherId.HasValue ? _repository.GetById(p.MotherId.Value) : null;
            lblFather.Text = father?.FullName ?? "—";
            lblMother.Text = mother?.FullName ?? "—";

            var children = _repository.GetChildren(p.Id);
            lblChildren.Text = children.Any()
                ? string.Join(", ", children.Select(c => c.FirstName))
                : "—";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using var dlg = new PersonEditForm(_repository, null);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                RefreshPersonList();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstPersons.SelectedItem is not Person selected)
            {
                MessageBox.Show("Будь ласка, оберіть особу зі списку.", "Редагування", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var dlg = new PersonEditForm(_repository, selected);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                RefreshPersonList();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstPersons.SelectedItem is not Person selected)
            {
                MessageBox.Show("Будь ласка, оберіть особу зі списку.", "Видалення", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var hasChildren = _repository.GetChildren(selected.Id).Any();
            var hasDescendants = _repository.GetAll().Any(p => p.FatherId == selected.Id || p.MotherId == selected.Id);
            string warning = hasDescendants ? "\n\nУвага: ця особа є батьком/матір'ю інших осіб у базі." : "";

            var result = MessageBox.Show(
                $"Видалити «{selected.FullName}»?{warning}",
                "Підтвердження видалення",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _repository.Delete(selected.Id);
                RefreshPersonList();
                ClearDetails();
            }
        }

        private void ClearDetails()
        {
            lblName.Text = "—";
            lblBirth.Text = "—";
            lblDeath.Text = "—";
            lblBirthPlace.Text = "—";
            lblCitizenship.Text = "—";
            lblNotes.Text = "—";
            lblFather.Text = "—";
            lblMother.Text = "—";
            lblChildren.Text = "—";
            pnlTree.Invalidate();
        }


        private void txtSearch_TextChanged(object sender, EventArgs e) => RefreshPersonList();


        private void btnSave_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog { Filter = "JSON файли (*.json)|*.json", Title = "Зберегти базу даних" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _repository.SaveToFile(dlg.FileName);
                    MessageBox.Show("Дані успішно збережено.", "Збереження", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "JSON файли (*.json)|*.json", Title = "Завантажити базу даних" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _repository.LoadFromFile(dlg.FileName);
                    RefreshPersonList();
                    ClearDetails();
                    MessageBox.Show("Дані успішно завантажено.", "Завантаження", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка завантаження: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnAncestors_Click(object sender, EventArgs e)
        {
            if (lstPersons.SelectedItem is not Person selected)
            {
                MessageBox.Show("Будь ласка, оберіть особу зі списку.", "Пошук", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var ancestors = _repository.GetAncestors(selected.Id);
            ShowSearchResults($"Предки: {selected.FullName}", ancestors);
        }

        private void btnDescendants_Click(object sender, EventArgs e)
        {
            if (lstPersons.SelectedItem is not Person selected)
            {
                MessageBox.Show("Будь ласка, оберіть особу зі списку.", "Пошук", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var descendants = _repository.GetDescendants(selected.Id);
            ShowSearchResults($"Нащадки: {selected.FullName}", descendants);
        }

        private void ShowSearchResults(string title, IEnumerable<Person> persons)
        {
            var list = persons.ToList();
            if (!list.Any())
            {
                MessageBox.Show("Не знайдено жодної особи.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var text = string.Join("\n", list.Select(p => $"• {p.FullName} ({p.BirthDate?.Year.ToString() ?? "?"})"));
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                ShowHelp();
                return true;
            }
            if (keyData == Keys.Delete && lstPersons.Focused)
            {
                btnDelete_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "Генеалогічне дерево — довідка\n\n" +
                "• Додати особу — кнопка «Додати» або Ins\n" +
                "• Редагувати — кнопка «Редагувати» або Enter\n" +
                "• Видалити — кнопка «Видалити» або Del\n" +
                "• Пошук предків / нащадків — відповідні кнопки\n" +
                "• Зберегти / завантажити — кнопки у нижній панелі\n" +
                "• F1 — ця довідка",
                "Довідка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
