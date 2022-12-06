 
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

~~~bash  
git clone https://github.com/Kyziur/WhatToCook.git
~~~
.NET version 7.0 is also required to run this project. You can download .NET at https://dotnet.microsoft.com/en-us/download.
Next step is to download nodejs from:
https://nodejs.org/en/download/

~~~
Then go to the project directory  

~~~bash  
npm install -g @angular/cli

~~~

Download postgres from https://www.postgresql.org/download/

Then install EF Core CLI tools
~~~
dotnet tool install --global dotnet-ef
~~~

Set up connection to a database in appsetting.json file by changing ConnectionStrings with data that you have written, eg.
~~~
"DefaultConnection": "Host=localhost;Database=WhatToCook;Port=5432;Username=YOURUSERNAME;Password=YOURPASSWORD" 
~~~
Then, set up an initial migration with the command:
~~~
dotnet ef migrations add InitialMigration -o .\Infrastructure\Migrations\ --startup-project ..\WhatToCook.WebApp\WhatToCook.WebApp.csproj
~~~
Lastly, update the database:
~~~
dotnet ef database update --startup-project ..\WhatToCook.WebApp\WhatToCook.WebApp.csproj
~~~

In order to run the app, it is required to install **VisualStudio** or **VisualStudioCode** and then run the project directly through one of these programs or the command `dotnet run` and loading the project.

### Git flow
In this app, flow of the development is based on creating branches that allow to resolve issues of this app in seperate branches, and then pull request is made in order to review the code.
## Feedback  

If you have any feedback, please reach out to us at github.com via issues.

## License  
[MIT](https://choosealicense.com/licenses/mit/)
