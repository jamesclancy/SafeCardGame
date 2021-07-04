FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

RUN apk update
RUN apk add nodejs
RUN apk add bash


COPY /deploy /app
WORKDIR /app


EXPOSE 8085
ENTRYPOINT [ "dotnet", "Server.dll" ]