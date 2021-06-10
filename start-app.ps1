#docker run -d --name dev-postgres -e POSTGRES_PASSWORD=trident1814 -v c:/data/safecardgamepostgresqldata/:/var/lib/postgresql/data -p 5432:5432 postgres
docker start dev-postgres
dotnet fake build --target run