# WhatToCook

## Tech Stack

**Client:** Angular

**Server:** ASP .NET Core, PostgreSQL

## Features

### In development

## Lessons Learned

### In development

## Run Locally

To run the project, it is required to:

Clone the project

```bash
git clone https://github.com/Kyziur/WhatToCook.git
```

.NET version 8.0 is required to run this project. You can download .NET at https://dotnet.microsoft.com/en-us/download.
Next step is to download nodejs from:
https://nodejs.org/en/download/

Open PowerShell and navigate to the repository. Run the script using:

```powershell
.\SetupEnvironment.ps1
```

Follow Prompts: The script will prompt you to install any missing dependencies (like .NET, Node.js, or PostgreSQL) and will ask for your PostgreSQL connection string.

Complete Setup: After the script completes, you should have a fully set up environment ready for development.

In order to run the app, it is required to install **VisualStudio** or **VisualStudioCode** and then run the project directly through one of these programs or the command `dotnet run` and loading the project.

### Git flow

In this app, flow of the development is based on creating branches that allow to resolve issues of this app in seperate branches, and then pull request is made in order to review the code.

## Feedback

If you have any feedback, please reach out to us at github.com via issues.

## License

[MIT](https://choosealicense.com/licenses/mit/)
