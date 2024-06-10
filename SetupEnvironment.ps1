# Install .NET 8.0 (if not already installed)
Write-Host "Checking for .NET 8.0..."
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "Downloading and installing .NET 8.0..."
    Start-Process -FilePath "https://dotnet.microsoft.com/en-us/download"
    Write-Host "Please install .NET 8.0 manually and then continue the script."
    Read-Host "Press Enter to continue after installation..."
}

# Install Node.js (if not already installed)
Write-Host "Checking for Node.js..."
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "Downloading and installing Node.js..."
    Start-Process -FilePath "https://nodejs.org/en/download/"
    Write-Host "Please install Node.js manually and then continue the script."
    Read-Host "Press Enter to continue after installation..."
}

# Install npm packages
Write-Host "Installing npm packages in root..."
npm install

# Run npm prepare
Write-Host "Running npm prepare to setup git hooks..."
npm run prepare

# Restore .NET tools
Write-Host "Restoring .NET tools..."
dotnet tool restore

# Install Angular CLI globally
Write-Host "Installing Angular CLI Globally..."
npm install -g @angular/cli

# Navigate to the frontend directory and install npm packages
Write-Host "Navigating to the frontend directory and installing npm packages..."
Set-Location "src/frontend"
npm install

# Navigate back to the root directory
Set-Location ../../

# Download and install PostgreSQL (if not already installed)
Write-Host "Checking for PostgreSQL..."
if (-not (Get-Command psql -ErrorAction SilentlyContinue)) {
    Write-Host "Downloading and installing PostgreSQL..."
    Start-Process -FilePath "https://www.postgresql.org/download/"
    Write-Host "Please install PostgreSQL manually and then continue the script."
    Read-Host "Press Enter to continue after installation..."
}

# Set up database connection in appsettings.json

$choice = Read-Host "Do you want to Set up database connection in appsettings.json? (y/n)"

# Convert the input to lowercase for case-insensitivity
$choice = $choice.ToLower()

# Check the user's choice and perform actions accordingly
if ($choice -eq "y") {
    Write-Host "Setting up database connection..."
    $connectionString = Read-Host "Enter your PostgreSQL connection string (e.g., Host=localhost;Database=WhatToCook;Port=5432;Username=YOURUSERNAME;Password=YOURPASSWORD)"
    $appsettingsPath = "src/WhatToCook.WebApp/appsettings.json"
    $appsettingsContent = Get-Content $appsettingsPath -Raw
    $appsettingsContent = $appsettingsContent -replace '"DefaultConnection": ".*"', ('"DefaultConnection": "' + $connectionString + '"')
    Set-Content -Path $appsettingsPath -Value $appsettingsContent
} else {
    Write-Host "Skipping the step..."
}

# Set up initial migration
Write-Host "Setting up initial migration..."
dotnet ef migrations add InitialMigration -o .\Infrastructure\Migrations\ --startup-project src\WhatToCook.WebApp\WhatToCook.WebApp.csproj

# Update the database
Write-Host "Updating the database..."
dotnet ef database update --startup-project src\WhatToCook.WebApp\WhatToCook.WebApp.csproj

# Final instructions
Write-Host "Environment setup complete!"
Write-Host "Please install Visual Studio or Visual Studio Code to run the project."
Write-Host "You can now run the project using 'dotnet run' or through your IDE."