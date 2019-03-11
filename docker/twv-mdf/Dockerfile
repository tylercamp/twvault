FROM microsoft/aspnetcore:2.0
WORKDIR /app/
COPY out/* ./
RUN mkdir /opt/twvault
ENTRYPOINT ["dotnet", "TW.Vault.MapDataFetcher.dll"]