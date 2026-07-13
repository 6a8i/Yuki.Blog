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
COPY Core/Visma.Yuki.Blog.Domain/Visma.Yuki.Blog.Domain.csproj Core/Visma.Yuki.Blog.Domain/
COPY Core/Visma.Yuki.Blog.Application/Visma.Yuki.Blog.Application.csproj Core/Visma.Yuki.Blog.Application/
COPY Adapters/Driven/Visma.Yuki.Blog.Infrastructure/Visma.Yuki.Blog.Infrastructure.csproj Adapters/Driven/Visma.Yuki.Blog.Infrastructure/
COPY Orchestration/Visma.Yuki.Blog.Shared/Visma.Yuki.Blog.Shared.csproj Orchestration/Visma.Yuki.Blog.Shared/
COPY Adapters/Driving/Visma.Yuki.Blog.Api/Visma.Yuki.Blog.Api.csproj Adapters/Driving/Visma.Yuki.Blog.Api/

RUN dotnet restore Adapters/Driving/Visma.Yuki.Blog.Api/Visma.Yuki.Blog.Api.csproj

COPY Core/Visma.Yuki.Blog.Domain/ Core/Visma.Yuki.Blog.Domain/
COPY Core/Visma.Yuki.Blog.Application/ Core/Visma.Yuki.Blog.Application/
COPY Adapters/Driven/Visma.Yuki.Blog.Infrastructure/ Adapters/Driven/Visma.Yuki.Blog.Infrastructure/
COPY Orchestration/Visma.Yuki.Blog.Shared/ Orchestration/Visma.Yuki.Blog.Shared/
COPY Adapters/Driving/Visma.Yuki.Blog.Api/ Adapters/Driving/Visma.Yuki.Blog.Api/

WORKDIR /src/Adapters/Driving/Visma.Yuki.Blog.Api
RUN dotnet publish Visma.Yuki.Blog.Api.csproj -c Release -o /app/publish --no-restore

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

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Visma.Yuki.Blog.Api.dll"]
