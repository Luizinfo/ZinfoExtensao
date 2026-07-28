param(
    [Parameter(Mandatory = $true)]
    [string] $VsixPath
)

$ErrorActionPreference = 'Stop'

$resolvedVsix = (Resolve-Path -LiteralPath $VsixPath).Path
Add-Type -AssemblyName System.IO.Compression.FileSystem

$archive = [IO.Compression.ZipFile]::OpenRead($resolvedVsix)
try {
    $pkgdefEntry = $archive.Entries |
        Where-Object { $_.FullName -ieq 'TechLeadTools.VisualStudio.pkgdef' } |
        Select-Object -First 1
    $assemblyEntry = $archive.Entries |
        Where-Object { $_.FullName -ieq 'TechLeadTools.VisualStudio.dll' } |
        Select-Object -First 1

    if ($null -eq $pkgdefEntry) {
        throw 'O VSIX não contém TechLeadTools.VisualStudio.pkgdef.'
    }

    if ($null -eq $assemblyEntry) {
        throw 'O VSIX não contém TechLeadTools.VisualStudio.dll.'
    }

    $reader = [IO.StreamReader]::new(
        $pkgdefEntry.Open(),
        [Text.UTF8Encoding]::new($false)
    )
    try {
        $pkgdef = $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }
}
finally {
    $archive.Dispose()
}

$expectedCodeBase =
    '"CodeBase"="$PackageFolder$\TechLeadTools.VisualStudio.dll"'

if ($pkgdef.IndexOf(
    $expectedCodeBase,
    [StringComparison]::OrdinalIgnoreCase
) -lt 0) {
    throw "O pkgdef empacotado não contém o CodeBase esperado: $expectedCodeBase"
}

if ($pkgdef.IndexOf(
    '[$RootKey$\Menus]',
    [StringComparison]::OrdinalIgnoreCase
) -lt 0) {
    throw 'O pkgdef empacotado não registra o recurso Menus.ctmenu.'
}

Write-Host 'VSIX validado: CodeBase, assembly e recurso de menus presentes.'
