FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

RUN usermod -l dev -d /home/dev -m ubuntu && \
    groupmod -n dev ubuntu && \
    mkdir -p /data/financial-documents && \
    chown dev:dev /app /data /data/financial-documents

USER dev

ENV PATH="$PATH:/home/dev/.dotnet/tools"
ENV ASPNETCORE_HTTP_PORTS=8080

RUN dotnet tool install --global dotnet-ef

COPY --chown=dev:dev . .

EXPOSE 8080

ENTRYPOINT ["dotnet", "watch", "--no-restore", "--no-launch-profile"]
