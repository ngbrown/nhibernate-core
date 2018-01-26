FROM buildpack-deps:jessie-scm

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        unzip \
        libc6 \
        libcurl3 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu52 \
        liblttng-ust0 \
        libssl1.0.0 \
        libstdc++6 \
        libunwind8 \
        libuuid1 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/*

# Install .NET Core runtime - https://github.com/dotnet/dotnet-docker/blob/master/1.0/runtime/jessie/Dockerfile
ENV DOTNET_VERSION 1.0.5
ENV DOTNET_DOWNLOAD_SHA 55481b0254a72d8c342ba6ccca3908ffb5c99d7eeb54f83dec6cc93c6b4cc3ae
ENV DOTNET_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-debian-x64.$DOTNET_VERSION.tar.gz

RUN curl -SL $DOTNET_DOWNLOAD_URL --output dotnet.tar.gz \
    && echo "$DOTNET_DOWNLOAD_SHA dotnet.tar.gz" | sha256sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz

# Install .NET Core SDK - https://github.com/dotnet/dotnet-docker/blob/master/2.0/sdk/jessie/amd64/Dockerfile
ENV DOTNET_SDK_VERSION 2.0.0
ENV DOTNET_SDK_DOWNLOAD_URL https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-sdk-$DOTNET_SDK_VERSION-linux-x64.tar.gz
ENV DOTNET_SDK_DOWNLOAD_SHA E457F3A5685382F7F24851A2E76EDBE75B575948C8A7F43220F159BA29C329A5008BBE7220C18DFB31EAF0398FC72177B1948B65E19B34ED0D907EFB459CF4B0

RUN curl -SL $DOTNET_SDK_DOWNLOAD_URL --output dotnet.tar.gz \
    && echo "$DOTNET_SDK_DOWNLOAD_SHA dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# Trigger the population of the local package cache
ENV NUGET_XMLDOC_MODE skip
RUN mkdir warmup \
    && cd warmup \
    && dotnet new \
    && cd .. \
    && rm -rf warmup \
    && rm -rf /tmp/NuGetScratch
