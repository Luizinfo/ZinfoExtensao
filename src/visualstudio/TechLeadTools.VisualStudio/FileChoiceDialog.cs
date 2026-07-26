using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TechLeadTools.VisualStudio
{
    internal sealed class FileChoiceDialog : Form
    {
        private readonly ListBox listBox;
        private readonly IReadOnlyList<FileChoice> choices;

        public FileChoiceDialog(string root, IEnumerable<string> paths)
        {
            choices = paths
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(path => new FileChoice(path, MakeRelative(root, path)))
                .ToArray();

            Text = "TechLeadTools — escolher arquivo";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowIcon = false;
            ClientSize = new Size(700, 360);

            var label = new Label
            {
                AutoSize = true,
                Location = new Point(12, 12),
                Text = "Mais de um arquivo corresponde ao bloco TLT. Escolha o destino:"
            };

            listBox = new ListBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                DisplayMember = nameof(FileChoice.DisplayPath),
                Location = new Point(12, 38),
                Size = new Size(676, 270)
            };
            listBox.DataSource = choices;
            listBox.DoubleClick += (_, __) => AcceptSelection();

            var openButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(532, 320),
                Size = new Size(75, 28),
                Text = "Abrir"
            };
            openButton.Click += (_, __) => AcceptSelection();

            var cancelButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel,
                Location = new Point(613, 320),
                Size = new Size(75, 28),
                Text = "Cancelar"
            };

            Controls.Add(label);
            Controls.Add(listBox);
            Controls.Add(openButton);
            Controls.Add(cancelButton);
            AcceptButton = openButton;
            CancelButton = cancelButton;

            if (choices.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }
        }

        public string? SelectedPath =>
            listBox.SelectedItem is FileChoice choice ? choice.FullPath : null;

        private void AcceptSelection()
        {
            if (SelectedPath != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private static string MakeRelative(string root, string path)
        {
            var rootWithSeparator = root.EndsWith(
                Path.DirectorySeparatorChar.ToString(),
                StringComparison.Ordinal)
                ? root
                : root + Path.DirectorySeparatorChar;
            return Uri.UnescapeDataString(
                new Uri(rootWithSeparator).MakeRelativeUri(new Uri(path)).ToString());
        }

        private sealed class FileChoice
        {
            public FileChoice(string fullPath, string displayPath)
            {
                FullPath = fullPath;
                DisplayPath = displayPath;
            }

            public string FullPath { get; }

            public string DisplayPath { get; }
        }
    }
}
