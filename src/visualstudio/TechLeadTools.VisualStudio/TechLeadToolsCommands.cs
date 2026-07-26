using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using TechLeadTools.Protocol;

namespace TechLeadTools.VisualStudio
{
    internal sealed class TechLeadToolsCommands
    {
        private const int CopyCommandId = 0x0100;
        private const int PasteCommandId = 0x0101;

        private static readonly Guid CommandSet =
            new Guid("671AA08A-B72C-45CD-B513-A418D16214A2");

        private readonly AsyncPackage package;

        private TechLeadToolsCommands(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package;
            commandService.AddCommand(new OleMenuCommand(
                ExecuteCopy,
                new CommandID(CommandSet, CopyCommandId)));
            commandService.AddCommand(new OleMenuCommand(
                ExecutePaste,
                new CommandID(CommandSet, PasteCommandId)));
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService))
                as OleMenuCommandService;
            if (commandService != null)
            {
                _ = new TechLeadToolsCommands(package, commandService);
            }
        }

        private void ExecuteCopy(object sender, EventArgs args)
        {
            _ = package.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    await CopyAsync();
                }
                catch (Exception exception)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    ShowError(exception.Message);
                }
            });
        }

        private void ExecutePaste(object sender, EventArgs args)
        {
            _ = package.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    await PasteAsync();
                }
                catch (Exception exception)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    ShowError(exception.Message);
                }
            });
        }

        private async Task CopyAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = await GetDteAsync();
            var document = dte?.ActiveDocument;
            var selection = document?.Selection as TextSelection;
            var textDocument = document?.Object("TextDocument") as TextDocument;

            if (document == null || selection == null || textDocument == null)
            {
                throw new InvalidOperationException("Nenhum editor de texto está ativo.");
            }

            if (string.IsNullOrWhiteSpace(document.FullName) || !File.Exists(document.FullName))
            {
                throw new InvalidOperationException("Salve o arquivo antes de copiá-lo.");
            }

            var solutionRoot = GetSolutionRoot(dte!);
            var relativePath = GetSafeRelativePath(solutionRoot, document.FullName);
            var range = LineRange.FromSelection(
                selection.TopPoint.Line,
                selection.BottomPoint.Line,
                selection.BottomPoint.LineCharOffset,
                selection.IsEmpty);

            var content = ReadFullLines(textDocument, range.StartLine, range.EndLine);
            var payload = new TltPayload
            {
                Workspace = Path.GetFileNameWithoutExtension(dte!.Solution.FullName),
                Path = relativePath,
                File = Path.GetFileName(document.FullName),
                ClassName = FindContainingClass(document, range.StartLine, range.EndLine),
                StartLine = range.StartLine,
                EndLine = range.EndLine
            };

            Clipboard.SetText(TltProtocol.Serialize(payload, content));
            var description = range.StartLine == range.EndLine
                ? $"linha {range.StartLine}"
                : $"linhas {range.StartLine}-{range.EndLine}";
            ShowMessage($"{description} copiadas com TLT.");
        }

        private async Task PasteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!Clipboard.ContainsText())
            {
                throw new FormatException("A área de transferência não contém texto.");
            }

            var block = TltProtocol.Parse(Clipboard.GetText());
            var dte = await GetDteAsync()
                ?? throw new InvalidOperationException("O Visual Studio não está disponível.");
            var solutionRoot = GetSolutionRoot(dte);
            var target = ResolveTarget(solutionRoot, block.Payload);
            if (target == null)
            {
                return;
            }

            dte.ItemOperations.OpenFile(target, EnvDTE.Constants.vsViewKindTextView);
            var document = dte.ActiveDocument;
            var textDocument = document?.Object("TextDocument") as TextDocument;
            var selection = document?.Selection as TextSelection;
            if (textDocument == null || selection == null)
            {
                throw new InvalidOperationException("Não foi possível abrir o arquivo como texto.");
            }

            var lastLine = Math.Max(1, textDocument.EndPoint.Line);
            var startLine = Math.Min(block.Payload.StartLine, lastLine);
            var endLine = Math.Min(block.Payload.EndLine, lastLine);

            selection.GotoLine(startLine, false);
            selection.MoveToLineAndOffset(endLine, 1, true);
            selection.EndOfLine(true);

            if (block.Payload.EndLine > lastLine)
            {
                ShowWarning(
                    $"O arquivo tem {lastLine} linhas; o intervalo foi ajustado.");
            }
        }

        private async Task<DTE2?> GetDteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return await package.GetServiceAsync(typeof(SDTE)) as DTE2;
        }

        private static string GetSolutionRoot(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solutionPath = dte.Solution?.FullName;
            if (string.IsNullOrWhiteSpace(solutionPath))
            {
                throw new InvalidOperationException(
                    "Abra e salve uma solução antes de usar o TechLeadTools.");
            }

            return Path.GetDirectoryName(Path.GetFullPath(solutionPath))
                ?? throw new InvalidOperationException("O caminho da solução é inválido.");
        }

        private static string GetSafeRelativePath(string root, string file)
        {
            var rootWithSeparator = AppendDirectorySeparator(Path.GetFullPath(root));
            var fullFile = Path.GetFullPath(file);
            if (!fullFile.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "O arquivo precisa pertencer à pasta da solução.");
            }

            var rootUri = new Uri(rootWithSeparator, UriKind.Absolute);
            var fileUri = new Uri(fullFile, UriKind.Absolute);
            var relative = Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString())
                .Replace('\\', '/');
            TltProtocol.Validate(new TltPayload
            {
                Workspace = "validation",
                Path = relative,
                File = Path.GetFileName(fullFile),
                ClassName = "Global",
                StartLine = 1,
                EndLine = 1
            });
            return relative;
        }

        private static string ReadFullLines(
            TextDocument textDocument,
            int startLine,
            int endLine)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var start = textDocument.StartPoint.CreateEditPoint();
            start.MoveToLineAndOffset(startLine, 1);
            var finish = textDocument.StartPoint.CreateEditPoint();
            var removeTrailingLineBreak = endLine < textDocument.EndPoint.Line;
            if (removeTrailingLineBreak)
            {
                finish.MoveToLineAndOffset(endLine + 1, 1);
            }
            else
            {
                finish.MoveToPoint(textDocument.EndPoint);
            }

            var text = start.GetText(finish);
            if (removeTrailingLineBreak)
            {
                if (text.EndsWith("\r\n", StringComparison.Ordinal))
                {
                    return text.Substring(0, text.Length - 2);
                }

                if (text.EndsWith("\n", StringComparison.Ordinal))
                {
                    return text.Substring(0, text.Length - 1);
                }
            }

            return text;
        }

        private static string FindContainingClass(
            Document document,
            int startLine,
            int endLine)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var fileCodeModel = document.ProjectItem?.FileCodeModel;
                if (fileCodeModel == null)
                {
                    return "Global";
                }

                string? best = null;
                FindContainingClass(
                    fileCodeModel.CodeElements,
                    startLine,
                    endLine,
                    Array.Empty<string>(),
                    ref best);
                return best ?? "Global";
            }
            catch
            {
                return "Global";
            }
        }

        private static void FindContainingClass(
            CodeElements elements,
            int startLine,
            int endLine,
            IReadOnlyList<string> parentClasses,
            ref string? best)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (CodeElement element in elements)
            {
                try
                {
                    if (element.StartPoint.Line > startLine || element.EndPoint.Line < endLine)
                    {
                        continue;
                    }

                    var classPath = parentClasses;
                    if (element.Kind == vsCMElement.vsCMElementClass)
                    {
                        classPath = parentClasses.Concat(new[] { element.Name }).ToArray();
                        best = string.Join(".", classPath);
                    }

                    if (element.Children != null && element.Children.Count > 0)
                    {
                        FindContainingClass(
                            element.Children,
                            startLine,
                            endLine,
                            classPath,
                            ref best);
                    }
                }
                catch
                {
                    // CodeModel é best-effort e pode falhar para linguagens específicas.
                }
            }
        }

        private static string? ResolveTarget(string solutionRoot, TltPayload payload)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var rootWithSeparator = AppendDirectorySeparator(Path.GetFullPath(solutionRoot));
            var exact = Path.GetFullPath(Path.Combine(
                solutionRoot,
                payload.Path.Replace('/', Path.DirectorySeparatorChar)));

            if (exact.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
                && File.Exists(exact))
            {
                return exact;
            }

            var matches = FindByName(solutionRoot, payload.File, 200).ToList();
            if (matches.Count == 0)
            {
                ShowError($"Não foi possível localizar “{payload.Path}” nesta solução.");
                return null;
            }

            if (matches.Count == 1)
            {
                ShowWarning($"O caminho mudou; “{payload.File}” foi localizado pelo nome.");
                return matches[0];
            }

            using (var dialog = new FileChoiceDialog(solutionRoot, matches))
            {
                return dialog.ShowDialog() == DialogResult.OK
                    ? dialog.SelectedPath
                    : null;
            }
        }

        private static IEnumerable<string> FindByName(
            string root,
            string fileName,
            int limit)
        {
            var ignoredDirectories = new HashSet<string>(
                new[] { ".git", "node_modules", "bin", "obj" },
                StringComparer.OrdinalIgnoreCase);
            var pending = new Stack<string>();
            pending.Push(root);
            var found = 0;

            while (pending.Count > 0 && found < limit)
            {
                var directory = pending.Pop();
                string[] files;
                string[] directories;
                try
                {
                    files = Directory.GetFiles(directory, fileName, SearchOption.TopDirectoryOnly);
                    directories = Directory.GetDirectories(directory);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (IOException)
                {
                    continue;
                }

                foreach (var file in files)
                {
                    yield return file;
                    found++;
                    if (found >= limit)
                    {
                        yield break;
                    }
                }

                foreach (var child in directories)
                {
                    if (!ignoredDirectories.Contains(Path.GetFileName(child)))
                    {
                        pending.Push(child);
                    }
                }
            }
        }

        private static string AppendDirectorySeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? path
                : path + Path.DirectorySeparatorChar;
        }

        private static void ShowMessage(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowMessageBox(message, OLEMSGICON.OLEMSGICON_INFO);
        }

        private static void ShowWarning(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowMessageBox(message, OLEMSGICON.OLEMSGICON_WARNING);
        }

        private static void ShowError(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ShowMessageBox(message, OLEMSGICON.OLEMSGICON_CRITICAL);
        }

        private static void ShowMessageBox(string message, OLEMSGICON icon)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            VsShellUtilities.ShowMessageBox(
                ServiceProvider.GlobalProvider,
                message,
                "TechLeadTools",
                icon,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
