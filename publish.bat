dotnet fake build --target Bundle
docker build -t testing-demo-card-game .
heroku container:push -a testing-demo-card-game web
heroku container:release -a testing-demo-card-game web