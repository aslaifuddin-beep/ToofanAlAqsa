# ===================================================
#  طوفان الأقصى - GitHub CI/CD Setup Script
#  Run this to push your project to GitHub
# ===================================================

Write-Host "=== طوفان الأقصى - GitHub CI/CD Setup ===" -ForegroundColor Cyan
Write-Host ""

$repoPrompt = "Enter your GitHub repository URL (e.g., https://github.com/username/ToofanAlAqsa): "
$repoUrl = Read-Host -Prompt $repoPrompt

if ([string]::IsNullOrWhiteSpace($repoUrl)) {
    Write-Host "No URL provided. Exiting." -ForegroundColor Red
    exit 1
}

# Set remote and push
git remote add origin $repoUrl
if ($?) {
    Write-Host "Remote added: $repoUrl" -ForegroundColor Green
} else {
    Write-Host "Failed to add remote. Check URL." -ForegroundColor Red
    exit 1
}

# Push to GitHub
Write-Host "Pushing to GitHub..." -ForegroundColor Yellow
git push -u origin master

if ($?) {
    Write-Host ""
    Write-Host "=== SUCCESS ===" -ForegroundColor Green
    Write-Host "Code pushed to: $repoUrl" -ForegroundColor Green
    Write-Host ""
    Write-Host "NEXT STEPS:" -ForegroundColor Cyan
    Write-Host "1. Go to: $repoUrl/actions" -ForegroundColor White
    Write-Host "2. You'll see the 'Build Android APK' workflow" -ForegroundColor White
    Write-Host "3. Go to Settings > Secrets and variables > Actions" -ForegroundColor White
    Write-Host "4. Add these secrets:" -ForegroundColor Yellow
    Write-Host "   - UNITY_EMAIL: Your Unity account email" -ForegroundColor Yellow
    Write-Host "   - UNITY_PASSWORD: Your Unity account password" -ForegroundColor Yellow
    Write-Host "   - UNITY_SERIAL: Your Unity serial key (for Plus/Pro)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "For Personal License:" -ForegroundColor Cyan
    Write-Host "1. Run the 'Acquire Unity License' workflow from Actions tab" -ForegroundColor White
    Write-Host "2. Download the activation file artifact" -ForegroundColor White
    Write-Host "3. Upload it to https://license.unity3d.com/manual" -ForegroundColor White
    Write-Host "4. Save the .ulf file and add to repo or use in CI" -ForegroundColor White
    Write-Host ""
    Write-Host "Once secrets are configured, push a new commit or run the workflow manually!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Push failed. Make sure:" -ForegroundColor Red
    Write-Host "1. The repository exists on GitHub" -ForegroundColor White
    Write-Host "2. You have push access" -ForegroundColor White
    Write-Host "3. Try: git push -u origin master --force" -ForegroundColor White
}
