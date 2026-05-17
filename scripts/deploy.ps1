# ShareFlow Deploy Script (Windows PowerShell)
# Usage: .\scripts\deploy.ps1

param(
    [string]$Server     = "43.129.23.249",
    [string]$SshUser    = "ubuntu",
    [string]$RemotePath = "/var/www/shareflow"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$StagingDir  = Join-Path $env:TEMP "shareflow"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  ShareFlow Deploy" -ForegroundColor Cyan
Write-Host "  Server : ${SshUser}@${Server}" -ForegroundColor Cyan
Write-Host "  Path   : $RemotePath" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Stage files
Write-Host "[1/4] Staging files..." -ForegroundColor Yellow

if (Test-Path $StagingDir) { Remove-Item -Recurse -Force $StagingDir }
New-Item -ItemType Directory -Path $StagingDir -Force | Out-Null

robocopy $ProjectRoot $StagingDir /E /XD bin obj node_modules .git Logs .vs dist /XF *.user .env *.log /NFL /NDL /NJH /NJS

if ($LASTEXITCODE -ge 8) {
    Write-Host "  ERROR: robocopy failed (exit code $LASTEXITCODE)" -ForegroundColor Red
    exit 1
}
Write-Host "  Done: $StagingDir" -ForegroundColor Green

# Step 2: Upload
Write-Host ""
Write-Host "[2/4] Uploading... (enter SSH password when prompted)" -ForegroundColor Yellow

ssh "${SshUser}@${Server}" "sudo mkdir -p /var/www && sudo chown ${SshUser}:${SshUser} /var/www"
scp -r "$StagingDir" "${SshUser}@${Server}:/var/www/"

if ($LASTEXITCODE -ne 0) {
    Write-Host "  ERROR: Upload failed" -ForegroundColor Red
    exit 1
}

Remove-Item -Recurse -Force $StagingDir
Write-Host "  Upload complete" -ForegroundColor Green

# Step 3: Check .env
Write-Host ""
Write-Host "[3/4] Checking .env on server..." -ForegroundColor Yellow

$envExists = ssh "${SshUser}@${Server}" "test -f ${RemotePath}/.env && echo yes || echo no"

if ($envExists.Trim() -ne "yes") {
    Write-Host "  WARNING: .env not found on server!" -ForegroundColor Red
    Write-Host "  SSH in and run:" -ForegroundColor Yellow
    Write-Host "    cd $RemotePath && cp .env.example .env && nano .env" -ForegroundColor White
    Write-Host "  Then re-run this script." -ForegroundColor Yellow
    exit 0
}
Write-Host "  .env OK" -ForegroundColor Green

# Step 4: Deploy via temp shell script
Write-Host ""
Write-Host "[4/4] Starting Docker containers..." -ForegroundColor Yellow
Write-Host "  (First build may take 5-10 minutes)" -ForegroundColor Gray
Write-Host ""

$tmpScript = Join-Path $env:TEMP "sf_deploy.sh"
$scriptContent = "#!/bin/bash`nset -e`ncd $RemotePath`ndocker compose up -d --build`ndocker compose ps"
[System.IO.File]::WriteAllText($tmpScript, $scriptContent, [System.Text.Encoding]::UTF8)

scp "$tmpScript" "${SshUser}@${Server}:/tmp/sf_deploy.sh"
ssh "${SshUser}@${Server}" "bash /tmp/sf_deploy.sh ; rm /tmp/sf_deploy.sh"

Remove-Item $tmpScript -Force

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Deployment complete!" -ForegroundColor Green
Write-Host "  Overseas API : http://${Server}:5001/health" -ForegroundColor Green
Write-Host "  Domestic API : http://${Server}:5002/health" -ForegroundColor Green
Write-Host "  Frontend     : http://${Server}:5173" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
