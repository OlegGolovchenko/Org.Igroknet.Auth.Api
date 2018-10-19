FROM microsoft/dotnet:aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 50350
EXPOSE 44372

FROM microsoft/dotnet:sdk AS build
WORKDIR /src
COPY ["Org.Igroknet.Auth.Api/Org.Igroknet.Auth.Api.csproj", "Org.Igroknet.Auth.Api/"]
RUN dotnet restore "Org.Igroknet.Auth.Api/Org.Igroknet.Auth.Api.csproj"
COPY . .
WORKDIR "/src/Org.Igroknet.Auth.Api"
RUN dotnet build "Org.Igroknet.Auth.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Org.Igroknet.Auth.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN mkdir /usr/share/igroknet
VOLUME ["/usr/share/igroknet"]
ENTRYPOINT ["dotnet", "Org.Igroknet.Auth.Api.dll"]