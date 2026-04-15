FROM node:22-slim AS playwright-browsers
ENV PLAYWRIGHT_BROWSERS_PATH=/playwright-browsers
RUN npx -y playwright@1.58.0 install chromium

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

FROM build AS publish
RUN dotnet publish "blastcms.sln" -c Release -o /app 

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends \
    libglib2.0-0t64 libnss3 libnspr4 libatk1.0-0 libatk-bridge2.0-0 libcups2 libdrm2 \
    libgbm1 libgtk-3-0 libxkbcommon0 libxcomposite1 libxdamage1 libxfixes3 libxrandr2 \
    libx11-xcb1 libpango-1.0-0 libcairo2 libasound2t64 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app . 
COPY --from=playwright-browsers /playwright-browsers /app/.playwright-browsers
ENV PLAYWRIGHT_BROWSERS_PATH=/app/.playwright-browsers
ENTRYPOINT ["dotnet", "blastcms.web.dll"]
