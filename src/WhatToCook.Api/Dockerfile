FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Build.props
COPY Directory.Packages.props Directory.Packages.props
COPY WhatToCook.slnx WhatToCook.slnx
COPY src/WhatToCook.Api/WhatToCook.Api.csproj src/WhatToCook.Api/
COPY src/WhatToCook.ServiceDefaults/WhatToCook.ServiceDefaults.csproj src/WhatToCook.ServiceDefaults/
RUN dotnet restore src/WhatToCook.Api/WhatToCook.Api.csproj

COPY src/WhatToCook.Api/ src/WhatToCook.Api/
COPY src/WhatToCook.ServiceDefaults/ src/WhatToCook.ServiceDefaults/
RUN dotnet publish src/WhatToCook.Api/WhatToCook.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER 1654:1654
ENTRYPOINT ["dotnet", "WhatToCook.Api.dll"]
