FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app/
COPY out/ ./
RUN mkdir /opt/twvault
ENTRYPOINT ["dotnet", "TW.Vault.MapDataFetcher.dll"]