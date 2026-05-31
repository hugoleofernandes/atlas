ROOT_DIR := $(shell git rev-parse --show-toplevel)

PROJECT=atlas

ENV_FILE=$(ROOT_DIR)/.env

COMPOSE=docker compose --env-file $(ENV_FILE) -p $(PROJECT) -f infrastructure/docker-compose.yml -f infrastructure/docker-compose.dev.yml

SRC=$(ROOT_DIR)/source-code/src
API=$(SRC)/Atlas.API

# =========================================
# CONTEXTS (GENERIC)
# =========================================

identity_PROJECT=$(SRC)/Atlas.Identity.Infrastructure
identity_CONTEXT=IdentityDbContext

staff_PROJECT=$(SRC)/Atlas.Staff.Infrastructure
staff_CONTEXT=StaffDbContext

platform_PROJECT=$(SRC)/Atlas.Platform.Infrastructure
platform_CONTEXT=PlatformDbContext

TARGET_PROJECT=$($(context)_PROJECT)
TARGET_CONTEXT=$($(context)_CONTEXT)

# =========================================
# DOCKER
# =========================================

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

# =========================================
# EF GENERIC COMMANDS
# =========================================

migrate:
	dotnet ef migrations add $(name) --project $(TARGET_PROJECT) --startup-project $(API) --context $(TARGET_CONTEXT) --output-dir Persistence/Migrations

update:
	dotnet ef database update --project $(TARGET_PROJECT) --startup-project $(API) --context $(TARGET_CONTEXT)

reset-db:
	dotnet ef database drop  --project $(TARGET_PROJECT) --startup-project $(API) --context $(TARGET_CONTEXT) --force
	dotnet ef database update --project $(TARGET_PROJECT) --startup-project $(API) --context $(TARGET_CONTEXT)
	@echo "Done. Start the API to apply seed data."

# SAMPLE:
# make migrate context=identity name=Initial
# make migrate context=staff name=Initial
# make migrate context=platform name=Initial
# make update context=identity
# make update context=staff
# make update context=platform
# make reset-db context=identity
# make reset-db context=staff
# make reset-db context=platform

# =========================================
# GLOBAL
# =========================================

ef-migrate-all:
	make migrate context=identity name=$(name)_Identity
	make migrate context=staff name=$(name)_Staff
	make migrate context=platform name=$(name)_Platform

ef-update-all:
	make update context=identity
	make update context=staff
	make update context=platform

ef-reset-db-all:
	make reset-db context=identity
	make reset-db context=staff
	make reset-db context=platform

# SAMPLE
# make ef-migrate-all name=Initial
# make ef-update-all
# make ef-reset-db-all

# =========================================
# DOCS (DOCFX)
# =========================================

SHELL := powershell.exe
.SHELLFLAGS := -NoProfile -Command

DOCS_DIR=source-code/docs

docs-clean:
	if (Test-Path "$(DOCS_DIR)\api") { Remove-Item -Recurse -Force "$(DOCS_DIR)\api" }
	if (Test-Path "$(DOCS_DIR)\_site") { Remove-Item -Recurse -Force "$(DOCS_DIR)\_site" }

docs-metadata:
	cd $(DOCS_DIR); docfx metadata docfx.json

docs-build:
	cd $(DOCS_DIR); docfx build docfx.json

docs-serve:
	cd $(DOCS_DIR); docfx serve _site --port 9000

docs: docs-clean docs-metadata docs-build

docs-v: docs-clean
	cd $(DOCS_DIR); docfx metadata docfx.json --logLevel verbose
	cd $(DOCS_DIR); docfx build docfx.json --logLevel verbose


# =========================================
# PROJECT STRUCTURE EXPORT
# =========================================
# tree "source-code/src" /F /A
# tree "source-code/src" /F /A | clip


# =========================================
# CLEAN MIGRATIONS
# =========================================

clean-migrations:
	powershell -Command "if (Test-Path '$(TARGET_PROJECT)/Persistence/Migrations') { Remove-Item -Recurse -Force '$(TARGET_PROJECT)/Persistence/Migrations' }"

# SAMPLE
# make clean-migrations context=identity
# make clean-migrations context=staff

# CLEAN ALL MIGRATIONS
ef-clean-all-m:
	make clean-migrations context=identity
	make clean-migrations context=staff
	make clean-migrations context=platform

# SAMPLE
# make ef-clean-all-m
