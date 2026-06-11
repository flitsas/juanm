# Instala dependencias de sistema para OCR de comparendos escaneados (Windows).
# Requiere winget. Ejecutar una vez por máquina de desarrollo.

$ErrorActionPreference = "Stop"

Write-Host "Instalando Poppler (pdf -> imagen)..." -ForegroundColor Cyan
winget install --id oschwartz10612.Poppler -e --accept-package-agreements --accept-source-agreements

Write-Host "Instalando Tesseract OCR..." -ForegroundColor Cyan
winget install --id UB-Mannheim.TesseractOCR -e --accept-package-agreements --accept-source-agreements

$tessdataDir = Join-Path $PSScriptRoot "..\services\python-ml\tessdata"
New-Item -ItemType Directory -Force -Path $tessdataDir | Out-Null

$spaUrl = "https://github.com/tesseract-ocr/tessdata_best/raw/main/spa.traineddata"
$spaFile = Join-Path $tessdataDir "spa.traineddata"
if (-not (Test-Path $spaFile)) {
    Write-Host "Descargando idioma spa ($spaUrl)..." -ForegroundColor Cyan
    Invoke-WebRequest -Uri $spaUrl -OutFile $spaFile -UseBasicParsing
}

$engSource = "C:\Program Files\Tesseract-OCR\tessdata\eng.traineddata"
$engTarget = Join-Path $tessdataDir "eng.traineddata"
if ((Test-Path $engSource) -and -not (Test-Path $engTarget)) {
    Copy-Item $engSource $engTarget
}

Write-Host ""
Write-Host "Listo. Reinicia python-ml:" -ForegroundColor Green
Write-Host "  py -m uvicorn app.main:app --app-dir services/python-ml --host 127.0.0.1 --port 4012 --reload"
