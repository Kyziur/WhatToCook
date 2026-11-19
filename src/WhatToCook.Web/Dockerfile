FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Build.props
COPY Directory.Packages.props Directory.Packages.props
COPY WhatToCook.slnx WhatToCook.slnx
COPY src/WhatToCook.Web/WhatToCook.Web.csproj src/WhatToCook.Web/
COPY src/WhatToCook.ServiceDefaults/WhatToCook.ServiceDefaults.csproj src/WhatToCook.ServiceDefaults/
RUN dotnet restore src/WhatToCook.Web/WhatToCook.Web.csproj

COPY src/WhatToCook.Web/ src/WhatToCook.Web/
COPY src/WhatToCook.ServiceDefaults/ src/WhatToCook.ServiceDefaults/
RUN dotnet publish src/WhatToCook.Web/WhatToCook.Web.csproj -c Release -o /app/publish

FROM debian:bookworm-slim AS data-protection-init
RUN apt-get update \
    && apt-get install -y --no-install-recommends openssl \
    && rm -rf /var/lib/apt/lists/* \
    && install -d -o 1654 -g 1654 -m 0700 /var/lib/whattocook/data-protection /var/lib/whattocook/data-protection-certificate
COPY deploy/web-data-protection-init.sh /usr/local/bin/web-data-protection-init
USER 1654:1654
ENTRYPOINT ["/bin/sh", "/usr/local/bin/web-data-protection-init"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && install -d -o "$APP_UID" -g "$APP_UID" -m 0700 /var/lib/whattocook/data-protection /var/lib/whattocook/data-protection-certificate
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "WhatToCook.Web.dll"]
