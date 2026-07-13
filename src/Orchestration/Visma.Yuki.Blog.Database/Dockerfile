FROM ubuntu:24.04 AS build
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y --no-install-recommends \
    wget ca-certificates unzip libicu74 tzdata \
    && rm -rf /var/lib/apt/lists/*
RUN wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet \
    && rm /tmp/dotnet-install.sh
ENV PATH="/usr/share/dotnet:${PATH}"
ENV DOTNET_ROOT=/usr/share/dotnet

WORKDIR /src
COPY Orchestration/Visma.Yuki.Blog.Database/Visma.Yuki.Blog.Database.csproj Orchestration/Visma.Yuki.Blog.Database/
RUN dotnet restore Orchestration/Visma.Yuki.Blog.Database/Visma.Yuki.Blog.Database.csproj

COPY Orchestration/Visma.Yuki.Blog.Database/ Orchestration/Visma.Yuki.Blog.Database/

WORKDIR /src/Orchestration/Visma.Yuki.Blog.Database
RUN dotnet publish Visma.Yuki.Blog.Database.csproj -c Release -o /app/publish --no-restore

FROM ubuntu:24.04 AS final
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y --no-install-recommends \
    wget ca-certificates libicu74 tzdata \
    && rm -rf /var/lib/apt/lists/*
RUN wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet --runtime aspnetcore \
    && rm /tmp/dotnet-install.sh
ENV PATH="/usr/share/dotnet:${PATH}"
ENV DOTNET_ROOT=/usr/share/dotnet

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Visma.Yuki.Blog.Database.dll"]
