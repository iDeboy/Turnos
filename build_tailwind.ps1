$files = Get-Content "tailwind-files.txt"
$tailwind = "C:\tailwindcss\tailwindcss.exe" <# Tailwind command #>

$workingDir = $pwd.Path

Write-Host "Working Dir: $workingDir"

foreach ($line in $files) {
    $parts = $line -split "\|"
    $i = $parts[0]
    $o = $parts[1]

    Write-Host "Input: $i | Output: $o"

    if (!(Test-Path $i)) {
        Write-Host "El archivo de entrada $i no existe."
        continue
    }

    & $tailwind -i $i -o $o --cwd $workingDir --minify *>$null 2>$null

    if ($LASTEXITCODE -ne 0) {
        Write-Host "No se genero el archivo $o."
    } else {
        Write-Host "Compilación exitosa: $o"
    }

}
