#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM node:22-slim AS playwright-browsers
ENV PLAYWRIGHT_BROWSERS_PATH=/playwright-browsers
RUN npx -y playwright@1.58.0 install chromium

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["blastcms.web/blastcms.web.csproj", "blastcms.web/"]
RUN dotnet restore "blastcms.web/blastcms.web.csproj"
COPY . .
WORKDIR "/src/blastcms.web"
RUN dotnet build "blastcms.web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "blastcms.web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends \
    libglib2.0-0t64 libnss3 libnspr4 libatk1.0-0 libatk-bridge2.0-0 libcups2 libdrm2 \
    libgbm1 libgtk-3-0 libxkbcommon0 libxcomposite1 libxdamage1 libxfixes3 libxrandr2 \
    libx11-xcb1 libpango-1.0-0 libcairo2 libasound2t64 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
COPY --from=playwright-browsers /playwright-browsers /app/.playwright-browsers
ENV PLAYWRIGHT_BROWSERS_PATH=/app/.playwright-browsers
ENTRYPOINT ["dotnet", "blastcms.web.dll"]
