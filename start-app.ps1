#docker run -d --name dev-postgres -e POSTGRES_PASSWORD=whatever-you-config-in-your-app-secrets -v c:/data/safecardgamepostgresqldata/:/var/lib/postgresql/data -p 5432:5432 postgres 
docker start dev-postgres
dotnet fake build --target run
