using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace GenealogyTree
{
    public class TreeRenderer
    {
        private readonly Panel _panel;

        private const int BoxW  = 160;
        private const int BoxH  = 50;
        private const int HGap  = 24;
        private const int VGap  = 60;

        private static readonly Color MaleColor     = Color.FromArgb(200, 220, 245);
        private static readonly Color FemaleColor   = Color.FromArgb(245, 210, 220);
        private static readonly Color NeutralColor  = Color.FromArgb(230, 230, 230);
        private static readonly Color SelectedColor = Color.FromArgb(255, 230, 100);
        private static readonly Color BorderColor   = Color.FromArgb(100, 100, 140);
        private static readonly Font  NameFont = new Font("Segoe UI", 8f, FontStyle.Bold);
        private static readonly Font  YearFont = new Font("Segoe UI", 7.5f);

        private FamilyRepository? _repo;
        private Person?           _root;
        private List<TreeNode>    _cached = new();

        public TreeRenderer(Panel panel)
        {
            _panel = panel;
            typeof(Panel).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)!
                .SetValue(panel, true);
            _panel.Paint += OnPaint;
        }

        public void Draw(Person root, FamilyRepository repo)
        {
            _root = root;
            _repo = repo;
            RebuildCache();
            _panel.Invalidate();
        }

        private void RebuildCache()
        {
            if (_root == null || _repo == null) { _cached = new(); return; }
            _cached = BuildTree(_root, _repo);

            if (_cached.Count > 0)
            {
                int w = _cached.Max(n => n.X) + BoxW + 20;
                int h = _cached.Max(n => n.Y) + BoxH + 20;
                var newSize = new Size(w, h);
                if (_panel.AutoScrollMinSize != newSize)
                    _panel.AutoScrollMinSize = newSize;
            }
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            if (_cached.Count == 0) return;

            int offX = _panel.AutoScrollPosition.X;
            int offY = _panel.AutoScrollPosition.Y;

            // Лінії
            using var linePen = new Pen(Color.FromArgb(140, 140, 180), 1.5f);
            foreach (var n in _cached)
            {
                foreach (var child in n.Children)
                {
                    int x1 = n.X     + offX + BoxW / 2;
                    int y1 = n.Y     + offY + BoxH;
                    int x2 = child.X + offX + BoxW / 2;
                    int y2 = child.Y + offY;
                    int midY = (y1 + y2) / 2;
                    g.DrawBezier(linePen, x1, y1, x1, midY, x2, midY, x2, y2);
                }
            }

            foreach (var n in _cached)
                DrawBox(g, n, offX, offY);
        }

        private void DrawBox(Graphics g, TreeNode n, int offX, int offY)
        {
            int x = n.X + offX;
            int y = n.Y + offY;
            var rect = new Rectangle(x, y, BoxW, BoxH);

            Color fill = n.IsSelected  ? SelectedColor
                       : n.Person.Gender == Gender.Male   ? MaleColor
                       : n.Person.Gender == Gender.Female ? FemaleColor
                       : NeutralColor;

            using var brush = new SolidBrush(fill);
            using var pen   = new Pen(BorderColor, n.IsSelected ? 2f : 1f);
            g.FillRoundedRect(brush, rect, 8);
            g.DrawRoundedRect(pen,   rect, 8);

            var name  = n.Person.FullName.Length > 22
                        ? n.Person.FullName[..22] + "…"
                        : n.Person.FullName;
            var years = BuildYearString(n.Person);

            var sf = new StringFormat
            {
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Center,
            };
            g.DrawString(name,  NameFont, Brushes.Black,
                new RectangleF(x + 4, y + 4,  BoxW - 8, 24), sf);
            g.DrawString(years, YearFont, Brushes.DimGray,
                new RectangleF(x + 4, y + 28, BoxW - 8, 18), sf);
        }

        private static string BuildYearString(Person p)
        {
            var b = p.BirthDate.HasValue  ? p.BirthDate.Value.Year.ToString()  : "?";
            var d = p.DeathDate.HasValue  ? p.DeathDate.Value.Year.ToString()  : "";
            return d.Length > 0 ? $"{b} – {d}" : $"н. {b}";
        }

        private List<TreeNode> BuildTree(Person root, FamilyRepository repo)
        {
            var nodeById    = new Dictionary<int, TreeNode>();
            var nodeLevel   = new Dictionary<int, int>();

            var rootNode = new TreeNode(root, true);
            nodeById[root.Id]  = rootNode;
            nodeLevel[root.Id] = 0;

            AddAncestors(rootNode, repo, nodeById, nodeLevel, depth: 0, maxDepth: 3);

            AddDescendants(rootNode, repo, nodeById, nodeLevel, depth: 0, maxDepth: 2);

            int minLvl = nodeLevel.Values.Min();
            if (minLvl != 0)
                foreach (var key in nodeLevel.Keys.ToList())
                    nodeLevel[key] -= minLvl;

            Layout(nodeById, nodeLevel);
            return nodeById.Values.ToList();
        }

        private void AddAncestors(TreeNode node, FamilyRepository repo,
            Dictionary<int, TreeNode> byId, Dictionary<int, int> levels,
            int depth, int maxDepth)
        {
            if (depth >= maxDepth) return;
            var p = node.Person;

            void TryAddParent(int? parentId)
            {
                if (!parentId.HasValue) return;
                if (byId.ContainsKey(parentId.Value)) return;
                var parent = repo.GetById(parentId.Value);
                if (parent == null) return;

                var pn = new TreeNode(parent, false);
                byId[parent.Id]   = pn;
                levels[parent.Id] = levels[node.Person.Id] - 1;
                pn.Children.Add(node);
                AddAncestors(pn, repo, byId, levels, depth + 1, maxDepth);
            }

            TryAddParent(p.FatherId);
            TryAddParent(p.MotherId);
        }

        private void AddDescendants(TreeNode node, FamilyRepository repo,
            Dictionary<int, TreeNode> byId, Dictionary<int, int> levels,
            int depth, int maxDepth)
        {
            if (depth >= maxDepth) return;
            foreach (var child in repo.GetChildren(node.Person.Id))
            {
                if (byId.ContainsKey(child.Id)) continue;
                var cn = new TreeNode(child, false);
                byId[child.Id]   = cn;
                levels[child.Id] = levels[node.Person.Id] + 1;
                node.Children.Add(cn);
                AddDescendants(cn, repo, byId, levels, depth + 1, maxDepth);
            }
        }

        private void Layout(Dictionary<int, TreeNode> byId, Dictionary<int, int> levels)
        {

            var byLevel = new Dictionary<int, List<TreeNode>>();
            foreach (var (id, lvl) in levels)
            {
                if (!byLevel.ContainsKey(lvl)) byLevel[lvl] = new();
                byLevel[lvl].Add(byId[id]);
            }

            int maxNodes = byLevel.Values.Max(n => n.Count);
            int canvasW  = Math.Max(620, maxNodes * (BoxW + HGap) + HGap);

            foreach (var (lvl, nodes) in byLevel)
            {
                int totalW  = nodes.Count * BoxW + (nodes.Count - 1) * HGap;
                int startX  = (canvasW - totalW) / 2;
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].X = startX + i * (BoxW + HGap);
                    nodes[i].Y = 10     + lvl * (BoxH + VGap);
                }
            }

            int minX = byId.Values.Min(n => n.X);
            if (minX < 10)
            {
                int shift = 10 - minX;
                foreach (var n in byId.Values) n.X += shift;
            }
        }
    }

    internal class TreeNode
    {
        public Person       Person     { get; }
        public bool         IsSelected { get; }
        public List<TreeNode> Children { get; } = new();
        public int X { get; set; }
        public int Y { get; set; }

        public TreeNode(Person person, bool isSelected)
        {
            Person     = person;
            IsSelected = isSelected;
        }
    }

    internal static class GraphicsExtensions
    {
        public static void FillRoundedRect(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using var path = GetRoundedPath(rect, radius);
            g.FillPath(brush, path);
        }

        public static void DrawRoundedRect(this Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using var path = GetRoundedPath(rect, radius);
            g.DrawPath(pen, path);
        }

        private static GraphicsPath GetRoundedPath(Rectangle r, int rad)
        {
            int d = rad * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X,          r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d,  r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d,  r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,          r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
