# UNDER CONSTRUCTION

## Start Postgres

```
docker-compose up -d pg
```

## Run Program

```
cd "SQL Source Control"
dotnet run --project SSC -- --help2

dotnet run --project SSC -- diff --source-db "host=localhost;port=9999;user id=postgres;password=Anduril.3791" --target ./config.json
```