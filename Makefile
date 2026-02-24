PROJECT=atlas
COMPOSE=docker compose -p $(PROJECT) -f infrastructure/docker-compose.yml -f infrastructure/docker-compose.dev.yml

up:
	$(COMPOSE) up --build -d

down:
	$(COMPOSE) down

logs:
	$(COMPOSE) logs -f

ps:
	$(COMPOSE) ps

reset:
	$(COMPOSE) down -v


migrate:
	dotnet ef database update --project source-code/src/Atlas.Infrastructure --startup-project source-code/src/Atlas.API

# test:
# 	dotnet test

# build:
# 	dotnet build